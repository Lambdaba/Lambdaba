# ADR-0001 — Native discriminated-union support in Lambdaba

## Status

Accepted (2026-05-08)

## Date

2026-05-08

## Decision Makers

- Architect (Beast Mode 4.3 design pass — Dreamer / Realist / Critic / Spec-by-Example)
- Lead (orchestration, scope-gathering across two question rounds with the user)
- User (ratifies the constraints, the C#-15-keyword choice for primitives, the `[Union] partial class` choice for HKT-bearing types, and the `Union<X,X>` no-mitigation policy)

## Supersedes

None. This is the first ADR in the project.

## Context

C# 15 ships with native discriminated-union support: a `[System.Runtime.CompilerServices.UnionAttribute]` marker, an `IUnion` interface, a new `union` keyword, exhaustive `switch` matching over union case types, and compiler-emitted implicit conversions (per the C# language proposal — see References). The runtime stubs are not yet in the BCL on net11 preview-2, so the project ships its own `UnionAttribute` and `IUnion` declarations under the canonical `System.Runtime.CompilerServices` namespace at `src/Lambdaba/Union.cs:1-17` to forward-port the contract.

Today the codebase uses the `[Union]` attribute on three HKT-bearing types but hand-rolls the basic union pattern at every site:

- `src/Lambdaba/Maybe.cs:101` — `[Union] public class Maybe<A>` declares two single-parameter ctors at lines 106–107, the `Value` property at lines 109–110, and the implicit conversion operators at lines 112–113. These remain hand-rolled: the generator augments each `[Union] partial class` with `Match` / `HasValue` / `Is<Case>` / `TryGetValue` members and (for HKT brands) a brand-side `Match<A, TResult>` helper, but does not emit constructors, `Value`, or implicit operators. Retaining the hand-rolled body was necessary because (a) implicit-operator emission for arbitrary case types is not yet reliable on net11 preview-2, and (b) the brand-side helper requires `Value` to be accessible at compile time.
- `src/Lambdaba/Maybe.cs:121` — `[Union] public class MaybeMonoid<A>` repeats the same hand-rolled body (lines 126–130, 132–133).
- `src/Lambdaba/Validated.cs:49` — `[Union] public class Validated<A>` does the same (lines 54–58, 60–61).

`Either<L, R>` does **not** use the `[Union]` attribute today. It is shaped as `abstract record Either<L>` (`src/Lambdaba/Either.cs:10`) plus `abstract record Either<L,R> : Either<L>, Data<Either<L>, R>` (`src/Lambdaba/Either.cs:141`), with `sealed record Left<L,R>(L Value)` and `sealed record Right<L,R>(R Value)` cases. Every brand method on `Either<L>` uses the pattern `t switch { Left<L,A>(var l) => …, Right<L,A>(var r) => …, _ => throw new NotSupportedException() }` — nine such trailing `_ =>` arms exist (lines 19, 27, 38, 40, 51, 59, 67, 75, 83, 91, 99 in `Either.cs`). They are unreachable today and would be eliminated by union-driven exhaustiveness.

Five typeclass interfaces drive the HKT brand detection used by the planned generator: `Functor<F>`, `Applicative<F>`, `Monad<M>`, `Alternative<F>`, `MonadPlus<M>` declared at `src/Lambdaba/Base.cs:569,591,679,830,858`. The brand class for a union (e.g. `Maybe`, `Validated`) is the parameterless companion that implements one or more of these and serves as the `F` in `Data<F, A>`. That indirection — `Data<F, A>` — is what the brand-side helper exists to hide.

Two scope items emerged from the C# 15 union spec review during the Critic phase that materially shape the decision:

1. **The `union` keyword lowers to a `struct`, not a class.** Per the published lowering rules in the C# 15 unions proposal, a `union T(A, B);` declaration becomes a struct that implicitly implements `IUnion` and exposes one ctor per case type plus `Value`. A struct cannot inherit from a class. Therefore the `union` keyword can only carry the **primitive `Union<T1..Tn>` arities** in the root `Lambdaba` namespace — it cannot replace `Maybe<A>`, `Validated<A>`, or `Either<L,R>`, which all extend a brand class.
2. **`Union<X, X>` instantiation collides at substitution time.** The lowering emits one ctor per case type. When two type parameters substitute to the same type (e.g. `Union<int, int>`), the lowering produces two constructors with identical signatures, which is a CS0111 conflict at the use site. The C# 15 spec offers no language-level mitigation. The user has accepted this constraint as documented behaviour.

The Squad framework's principles enforcement directive (`.claude/docs/principles-enforcement.md`) requires that every code change land with documented agreement on every deviation from established patterns. This ADR fulfils that contract for the union work.

## Decision

We will introduce native discriminated-union support in Lambdaba via four reinforcing pieces:

### (a) Primitive ad-hoc unions via the C# 15 `union` keyword

Declare arities `Union<T1, T2>` through `Union<T1, T2, T3, T4, T5, T6, T7, T8>` in `src/Lambdaba/Union.Primitives.cs` using the C# 15 `union` keyword:

```csharp
public union Union<T1, T2>(T1, T2);
// … through arity 8
```

The compiler lowering provides ctors per case type, the `Value` property, `IUnion` implementation, and the exhaustive-switch contract. No generator pass is needed for the primitives — the `union` keyword is sufficient.

### (b) `[Union] partial class` plus an IIncrementalGenerator for HKT-bearing types

For types that must extend a brand class to participate in HKT machinery (everything that today appears in `Maybe.cs`, `Validated.cs`, `Either.cs`), declare the type as `[Union] partial class` and let a Roslyn `IIncrementalGenerator` emit the basic union pattern. The pipeline uses `context.SyntaxProvider.ForAttributeWithMetadataName("System.Runtime.CompilerServices.UnionAttribute", …)` — the canonical attribute-driven entry point that is roughly two orders of magnitude faster than `CreateSyntaxProvider` per the Roslyn incremental-generator cookbook.

The generator project ships at `src/Lambdaba.SourceGenerators/Lambdaba.SourceGenerators.csproj` targeting `netstandard2.0` (mandatory for Roslyn analyzers) with a `PackageReference` to `Microsoft.CodeAnalysis.CSharp` version **5.3.0** — the first release whose syntax tree understands the C# 15 `union` and `field` keywords; 4.x cannot parse them.

### (c) Clean `Either` with single-parameter `Left<L>` / `Right<R>` cases

Drop the redundant carrier type parameter on the case records. `Left<L,R>(L Value)` becomes `Left<L>(L Value)`; `Right<L,R>(R Value)` becomes `Right<R>(R Value)`. The brand `Either<L>` and the union `[Union] partial class Either<L,R> : Either<L>, Data<Either<L>, R>, IUnion` continue to carry the L/R pair; cases only carry the type they hold. This eliminates the visual noise at every constructor and pattern site and aligns with how Haskell, F#, and Scala spell the cases. All call sites in `Either.cs:14-138`, `EitherExtensions.cs`, and `EitherTests.cs` are rewritten in the same PR.

### (d) Generator emission contract

For every `[Union] partial` type, the generator emits:

- **`Match(onCase1, …, onCaseN)`** — an instance method `R Match<R>(Func<Case1, R> onCase1, …, Func<CaseN, R> onCaseN)` that dispatches on the wrapped value. This is the user-facing visitor; it is **not** part of the C# 15 union member contract — it lives alongside the language-prescribed `Value` property as an ergonomic helper.
- **`Is<Case>`** — one `bool IsCaseN { get; }` property per case (e.g. `IsJust`, `IsNothing`, `IsLeft`, `IsRight`). Kept alongside `HasValue` as a per-case, non-overloaded discriminator — orthogonal to the spec's `HasValue` (which is union-wide presence) and useful in plain `if`/conditional contexts where overload resolution on `TryGetValue` is unhelpful.
- **`TryGetValue` overloads (one per case)** — one `bool TryGetValue(out CaseN? value)` overload per case. The overload set distinguishes cases by the `out` parameter type at the call site. **This matches the C# 15 spec's "non-boxing access pattern"** — the compiler recognises `HasValue` + `TryGetValue` and uses them for optimised pattern matching, eliding boxing on value-type cases. The earlier `TryGet<Case>` spelling (e.g. `TryGetJust`, `TryGetNothing`) is rejected: it would not be picked up by the compiler's optimised lowering and would duplicate intent already encoded in the `out` parameter type.
- **`HasValue` property** — one `bool HasValue { get; }` per `[Union] partial` type, as prescribed by the spec's non-boxing access pattern. Returns `true` when the union holds any case (the basic union pattern always holds exactly one case post-construction, so this is `true` after any normal construction; it exists for the spec contract and for the compiler's lowering, not as a runtime nullability check).
- **Brand-side `Match<A, R>(Data<F, A> data, …)`** — emitted only when the union's brand class is detected to implement one of the five HKT typeclass interfaces (`Functor<F>`, `Applicative<F>`, `Monad<M>`, `Alternative<F>`, `MonadPlus<M>` at `src/Lambdaba/Base.cs:569,591,679,830,858`) where the type parameter `F` matches the brand's own `OriginalDefinition`. The body is a one-liner: `((Union<A>)data).Match(onCase1, …)`. This is the call site that lets brand methods like `FMap` collapse to a single `Match` call — eliminating every `_ => throw new NotSupportedException()` arm in `Either<L>`.

The user, on 2026-05-08, signed off on the spec-blessed `TryGetValue` overloads + `HasValue` shape (closing the open implementation question that earlier spelt `TryGet<Case>`). `Is<Case>` is retained alongside `HasValue` because the two discriminators serve different idioms: `HasValue` participates in the compiler's pattern-matching lowering, `Is<Case>` reads naturally in plain conditional code without forcing the caller through `TryGetValue`.

The brand helper is **not** emitted for `MaybeMonoid<A>` (which implements `Monoid<MaybeMonoid<A>>` directly on the union, with no separate brand class) nor for the primitive `Union<T1..Tn>` arities (no HKT brand). The instance-side `Match` / `Is` / `TryGet` members are still emitted in those cases, since they only require the `[Union]` marker.

### (e) Conflict policy: skip + `LMBD001` warning

Before emitting each generated member, the generator scans `INamedTypeSymbol.GetMembers(name)` for an existing same-named member with a compatible signature. If one exists, the generator **skips emission** and emits a `Diagnostic` with id **`LMBD001`**, severity `Warning`, message *"User-declared member shadows generated union helper. Remove the user member to use the generated one, or rename it to silence this warning."* The build does not fail. This is the architecturally clean choice: the user's intent always wins and the warning surfaces the override. It also gives migration headroom — the conversion of `MaybeExtensions.Match` to `Fold` happens in the same PR but the warning would catch any forgotten call site before merge.

### (f) Ancillary cleanups in the same PR

- `EitherExtensions.cs`: delete `IsLeft` (line 15), `IsRight` (line 22), `Match<T>` (line 52) — replaced by generated members. Keep `FromLeft`, `FromRight`, `Lefts`, `Rights`, `PartitionEithers` (these are list / option projections, not generated by the union contract).
- `MaybeExtensions.cs`: delete `IsJust` (line 15), `IsNothing` (line 18). **Rename** `Match<B>(B defaultValue, Func<A, B> f)` at line 52 to `Fold<B>(B defaultValue, Func<A, B> f)` — Haskell's `maybe` combinator name. This avoids collision with the generator's case-tagged `Match` and matches the established functional vocabulary.

## Consequences

### Positive

- **Generated dispatch surface.** Every `[Union] partial class` gains `Match` / `HasValue` / `Is<Case>` / `TryGetValue` members from the generator without hand-rolling them. Constructors, `Value`, and implicit operators are still hand-rolled per case (see Context); the generator does not emit those on net11 preview-2.
- **Exhaustiveness from the compiler.** The nine `_ => throw new NotSupportedException()` arms in `Either<L>` (lines 19, 27, 38, 40, 51, 59, 67, 75, 83, 91, 99) all disappear. The C# 15 exhaustiveness rule covers them; the dead-arm cleanup is mechanical.
- **Brand-side `Match` helper available.** The generator emits a brand-side `Match<A, TResult>(Data<F, A> data, …)` helper that brand methods *may* use to subsume the cast-and-switch pattern. PR #2 retains the existing `((F<A>)data) switch { … }` sites in `Maybe`, `Validated`, and `Either<L>` brand methods — adopting the helper is a follow-up Boy-Scout opportunity.
- **Cleaner `Either` shape.** Dropping the carrier type parameter from the cases (`Left<L,R>` → `Left<L>`) saves one type-argument per case site. Roughly 40 call sites across `Either.cs`, `EitherExtensions.cs`, and `EitherTests.cs` lose visual noise.
- **A reusable generator surface.** Future HKT-bearing unions still need: the `[Union] partial class` declaration, hand-rolled constructors per case, the `Value` property, and implicit operators per case. The generator augments with `Match` / `HasValue` / `Is<Case>` / `TryGetValue` and (when an HKT brand is detected) the brand-side `Match<A, TResult>` helper.
- **Native primitive `Union<T1..T8>`** become available in the root namespace for ad-hoc product-of-sum scenarios where defining a named brand is overkill.

### Negative

- **`Union<X, X>` cannot be instantiated.** The C# 15 `union T(A, B);` lowering emits one ctor per case, so substituting two equal type arguments produces two CS0111-conflicting ctors. **No spec mitigation.** Documented at file level in `src/Lambdaba/Union.Primitives.cs` and recorded here. Users who need a redundant-arity union can declare a named `[Union] partial class` with explicit case wrappers.
- **New build dependency.** `Microsoft.CodeAnalysis.CSharp` 5.3.0 is required for the generator project. 4.x cannot parse the C# 15 `union` keyword in source it scans. This is a leaf dependency on the generator project only — `PrivateAssets="all"` keeps it out of `Lambdaba.dll`'s public dependency surface. Security-Expert SCA review is required per `decisions.md:241-244` before this dependency lands.
- **`IUnion` lives in `System.Runtime.CompilerServices`** (per the C# 15 spec). `src/Lambdaba/Union.cs` keeps that namespace until the runtime supplies the type natively, then we remove the file. Reviewer should flag any user code that places types in `System.Runtime.CompilerServices` for any other purpose — the rule remains "this namespace is reserved for compiler-recognised contracts".
- **Generator only fires on `[Union] partial`.** Non-partial `[Union]` types still build but receive no helpers. Intentional — keeps the contract explicit. A new contributor declaring `[Union] class Foo` (no `partial`) gets a clean type with no surprise generated surface, but also no helpers; the user has to opt in by adding `partial`.
- **External callers of `Either<L>` brand methods that supplied a non-Left/Right `Data<Either<L>,A>`** lose the runtime `NotSupportedException` safety net. With exhaustive switching, those code paths now silently match against whichever case the value happens to deserialise as, or fail at the cast. There are no such callers in the repo today (verified by grep), and the user has signed off in scope-gathering. The risk is recorded here for posterity.

### Neutral

- **HKT brand-helper emission requires one of the five typeclass interfaces.** This is a deliberate detection rule, not a constraint on what users can write. Brand classes that implement none of them (the `MaybeMonoid<A>` shape, where the type itself is the monoid) get the per-instance `Match` / `Is` / `TryGet` members but no brand-side helper — correct behaviour, since there's no `Data<F, A>` indirection to hide.
- **`Lambdaba.SourceGenerators` joins the solution.** `Lambdaba.sln` gets a new project entry. The misspelled `Lamdaba.ScratchBook` GUID block is preserved verbatim — that's existing state, not in scope here.
- **Tests migrate to TUnit before this work lands.** A precursor PR (recorded in the `architect-2026-05-08T120000Z-tunit-migration-from-xunit.md` decision drop in the squad inbox) converts the four xUnit test files to TUnit and deletes `UnitTest1.cs`. This ADR's PR therefore lands on TUnit from the first new test.

## Alternatives

### (i) Keep the hand-rolled `[Union]` boilerplate

Status quo: every `[Union] partial class` continues to declare its own ctors, `Value`, and implicit operators. The brand methods continue to use `_ => throw new NotSupportedException()` defensively. **Rejected** because the duplication is mechanical and uniform — exactly the case that source generation was designed to handle. The cost of the generator pays back at the third union and grows linearly thereafter; the project already has three.

### (ii) F#-style nominal sums via record hierarchy (the squad's `functional-ddd` skill)

Model every union as `abstract record Foo` with `sealed record FooCase1 : Foo` siblings, and rely on pattern matching plus `_ => throw new UnreachableException()` defaults. This is the pattern the squad's `functional-ddd` skill recommends *for application-layer domain unions* (see the skill at `.claude/skills/functional-ddd/SKILL.md`). **Rejected for this library** because (a) Lambdaba is a *Haskell port* whose unions are HKT-bearing brands, not domain types — they need to extend a `Data<F>`-implementing class to participate in typeclass dispatch — and the C# 15 union machinery composes cleanly with that, while abstract records do not buy us the exhaustive-switch contract. (b) The C# 15 `union` keyword for the primitives is genuinely simpler than any abstract-record alternative. (c) The project is on `LangVersion=preview` precisely so it can use the new feature; using the older idiom would defeat the purpose of being on preview.

The recommendation in the `functional-ddd` skill remains correct for downstream *consumers* of Lambdaba modelling their own domain unions — they should default to the record-hierarchy pattern. This ADR carves out an exception for the library's own typeclass machinery.

### (iii) Abstract record + sealed record subtypes (the current `Either` shape)

The shape today: `abstract record Either<L>` brand + `abstract record Either<L,R> : Either<L>, Data<Either<L>, R>` + `sealed record Left<L,R>(L Value) : Either<L,R>`. **Rejected** because it conflates the brand with the union, doubles the type-parameter list at every case site, and depends on `_ => throw new NotSupportedException()` for exhaustiveness. The clean refactor in this ADR keeps the `Either<L>` brand as a `record` (it's not itself a union), promotes `Either<L,R>` to a `[Union] partial class` so the generator emits its basic union pattern, and drops the carrier type parameter from the cases. Strictly cleaner on every axis.

## Related Decisions

- **`architect-2026-05-08T120000Z-tunit-migration-from-xunit`** (sibling decision drop, this date) — the precursor TUnit migration that PR #1 ships before this ADR's implementation lands.
- The Squad framework's testing rules at `.claude/docs/decisions.md:105-106` (TUnit only) and `decisions.md:117-118` (Spec-by-Example before any new feature) — both bind this work.
- The dependency-approval flow at `.claude/docs/decisions.md:236-253` — applies to `Microsoft.CodeAnalysis.CSharp 5.3.0`. The Security Expert performs the SCA review before PR #2 merges.
- The Boy Scout rule at `decisions.md:174-179` — justifies the `UnitTest1.cs` deletion, the dead-arm removals in `Either.cs`, and the `MaybeExtensions.Match` → `Fold` rename in the same PR rather than a follow-up.
- The principles-enforcement directive at `.claude/docs/principles-enforcement.md` — requires this ADR for the architectural change and a separate decision drop for the TUnit migration.

## References

- **C# 15 unions proposal** — *Unions - C# feature specifications (preview)* on Microsoft Learn, last updated 2026-04-13: <https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/unions>. The lowering rules, well-formedness conditions, the exact text of the `[Union]` attribute and `IUnion` interface, and the resolution that union declarations can implement interfaces and have a base clause are all sourced from this document.
- **Roslyn incremental generators cookbook** — `IIncrementalGenerator` patterns and the `ForAttributeWithMetadataName` hot-path: <https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md>.
- **Lambdaba implementation plan** — the user-approved scope and step sequence at `/Users/mauricepeters/.claude/plans/is-this-folder-ready-dazzling-finch.md` (titled *"Native discriminated-union support for Lambdaba"*).
- **Existing runtime-stub forward-port** — `src/Lambdaba/Union.cs:1-17` (the project's local declaration of `UnionAttribute` and `IUnion` until net11 ships them in the BCL).
- **HKT typeclass interfaces** — `src/Lambdaba/Base.cs` lines `569`, `591`, `679`, `830`, `858` (Functor / Applicative / Monad / Alternative / MonadPlus). The generator detects the brand by walking these.
- **Hand-rolled boilerplate to be replaced** — `src/Lambdaba/Maybe.cs:101,121`, `src/Lambdaba/Validated.cs:49`, `src/Lambdaba/Either.cs:10,141`.
