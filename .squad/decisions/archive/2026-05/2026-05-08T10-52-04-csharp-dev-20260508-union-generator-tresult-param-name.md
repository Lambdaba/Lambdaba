agent: csharp-dev
date: 2026-05-08
verdict: CONVENTION
scope: decision

# Generator: use `TResult` for return type parameter in generated Match and brand-side Match

## Decision

The source generator (`UnionGenerator.cs`) uses `TResult` (not `R`) as the name of the
return type parameter in both emitted `Match` method shapes:

1. **Instance `Match<TResult>` on union types** — avoids shadowing any class-level type
   parameter the union type already declares. `Either<L, R>` has `R` at class level; if
   the generator emitted `Match<R>`, the method's `R` would shadow the class's `R`
   (CS0693 warning) and break type inference in the brand helper cast.

2. **Brand-side `Match<{valueTyParam}, TResult>` on brand classes** — same reason: the
   value slot type param may conflict with a class-level name; `TResult` is safe regardless.

## Context

Discovered during the Either migration (step 7 of PR #2). `Either<L, R>` is the first
union with a type parameter named `R`, which collided with the generator's original
hardcoded `R` return-type param name. The fix is general and applies to all future
unions.

## Impact

- The public API of generated `Match` methods changes from `Match<R>(...)` to
  `Match<TResult>(...)`. Callers that pass type arguments positionally (the common case)
  are unaffected. Callers that name the return type arg explicitly must update to
  `TResult`.
- Verified: existing `UnionGeneratorSpec.cs` tests (`Maybe.Match<int, string>(...)`)
  continue to compile and pass — the positional `<int, string>` resolves correctly to
  `<A=int, TResult=string>`.
