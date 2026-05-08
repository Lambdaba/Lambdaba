---
id: architect-2026-05-08T120000Z-tunit-migration-from-xunit
agent: architect
verdict: INFO
scope: decision
created: 2026-05-08T12:00:00Z
targets:
  - path: src/Lambdaba.Tests/Lambdaba.Tests.csproj
  - path: src/Lambdaba.Tests/GlobalUsings.cs
  - path: src/Lambdaba.Tests/EitherTests.cs
  - path: src/Lambdaba.Tests/PreludeTest.cs
  - path: src/Lambdaba.Tests/ListTests.cs
  - path: src/Lambdaba.Tests/UnitTest1.cs
blockers: []
high: []
medium: []
good: []
references:
  - architect-20260508T120100Z-native-discriminated-unions
---

Migrate `src/Lambdaba.Tests` from xUnit to TUnit as a precursor PR before any new test code lands; this is a framework-compliance change with no behaviour impact.

## Context

The four xUnit test files in `src/Lambdaba.Tests/` (`EitherTests.cs` — 9 methods, `PreludeTest.cs` — 5 methods, `ListTests.cs` — 8 methods, plus the placeholder `UnitTest1.cs`) predate the Squad framework being loaded into `.claude/`. The framework binds tests to TUnit only at `.claude/docs/decisions.md:105-106` (*"TUnit Only — TUnit is the only test framework. No xUnit, no NUnit, no MSTest."*) and the principles-enforcement directive at `.claude/docs/principles-enforcement.md` requires explicit approval for any deviation. There is no documented deviation; therefore the existing xUnit usage is non-compliant from the moment the framework loaded and needs to be reconciled before any new tests are written.

The `Lambdaba.Tests.csproj` currently references `xunit 2.9.3`, `xunit.runner.visualstudio 3.0.2`, `coverlet.collector 6.0.4`, and `Microsoft.NET.Test.Sdk 17.13.0`. `GlobalUsings.cs` contains a single line: `global using Xunit;`. The ADR for native discriminated unions (`architect-20260508T120100Z-native-discriminated-unions`) introduces three new test files (`MaybeTests.cs`, `ValidatedTests.cs`, `UnionPrimitiveTests.cs`) and adds methods to `EitherTests.cs`. Letting that work land on top of xUnit would compound the violation; landing it on top of a TUnit migration is mechanical.

## Decision

Land a single precursor PR — branch `test/1-tunit-migration` — that converts `Lambdaba.Tests` to TUnit and removes the placeholder file. Scope is intentionally narrow: framework swap only, no new tests, no behaviour changes.

### Csproj changes

- **Remove**: `xunit`, `xunit.runner.visualstudio`, `coverlet.collector` package references.
- **Add**: `TUnit` (latest GA, Security-Expert SCA-reviewed per `decisions.md:241-244` before merge). Keep `Microsoft.NET.Test.Sdk 17.13.0`.

### GlobalUsings.cs

Replace `global using Xunit;` with the TUnit assertion namespace globals (`global using TUnit;` plus whichever assertion namespace the chosen TUnit version exposes — the C# Dev confirms at implementation time and records the exact lines in the PR).

### Per-file conversion

For each of `EitherTests.cs`, `PreludeTest.cs`, `ListTests.cs`:

1. `[Fact]` → `[Test]`.
2. `[Theory]` + `[InlineData(…)]` → `[Test]` + `[Arguments(…)]` (none of the four current files use `[Theory]` per visual inspection, but the rule is recorded for any future port).
3. Each test method becomes `async Task` (was `void`).
4. `Xunit.Assert.Equal(expected, actual)` → `await Assert.That(actual).IsEqualTo(expected)`.
5. `Xunit.Assert.True(x)` → `await Assert.That(x).IsTrue()`; `Xunit.Assert.False(x)` → `await Assert.That(x).IsFalse()`.
6. `Xunit.Assert.Throws<E>(action)` → `await Assert.That(action).Throws<E>()` (or the TUnit-idiomatic equivalent the C# Dev confirms).
7. **No** `// Arrange` / `// Act` / `// Assert` comments are introduced — `decisions.md:108-109` forbids them.

### Placeholder removal

Delete `src/Lambdaba.Tests/UnitTest1.cs`. It is a `dotnet new` template artefact with no business value. Boy Scout rule applies (`decisions.md:174-179`): if the file is touched in a sweep, leave it removed rather than re-port.

### Verification before PR review

- `dotnet test --nologo` succeeds with 21 tests (was 22 — the one in `UnitTest1.cs` is gone; the other 22 remain).
- No file under `src/Lambdaba.Tests/` references the `Xunit` namespace.
- The PR commit message follows Conventional Commits with the breaking marker: `test(tests)!: migrate from xUnit to TUnit`.
- Reviewer subagent passes per `.claude/agents/reviewer.md` and `principles-enforcement.md:128`.

## Why

Three reinforcing reasons:

1. **Framework compliance.** `decisions.md:105-106` is unambiguous and the project is in the framework now. There is no documented deviation, so xUnit usage is a 🔴 Must Fix the moment a Reviewer touches the code (`principles-enforcement.md:128` — *"undocumented deviations block merge unconditionally"*).
2. **Sequencing hygiene.** The native-unions PR (`architect-20260508T120100Z-native-discriminated-unions`) adds tests. Doing the migration first means every new test is born on the canonical framework; doing it second forces the unions PR to either land non-compliant TUnit-on-top-of-xUnit hybrid code or rewrite tests twice.
3. **Smaller blast radius.** The migration PR is mechanical — pattern substitutions across four files, one csproj, one globals file. Reviewing it independently of a behaviour change is significantly cheaper than reviewing it interleaved with the unions work.

## Consequences

- One extra PR before the unions work begins. Acceptable; the user has signed off on the split per the user-decided plan at `/Users/mauricepeters/.claude/plans/is-this-folder-ready-dazzling-finch.md`.
- A new TUnit dependency enters the test project. Security Expert SCA review is required before merge; the dependency-approval flow at `decisions.md:236-253` applies.
- `coverlet.collector` is removed without a replacement. If coverage is needed in CI later, the C# Dev proposes a TUnit-compatible coverage tool in a follow-up decision drop. Not in scope here.

## Spec-by-Example status

This is the framework-compliance migration carve-out — no behaviour change, no new feature surface, no new public API. The Spec-by-Example skip rule applies (`decisions.md:120` — *"Skip Spec-by-Example for: pure refactoring with no behavior change"*). The existing test bodies are themselves the regression contract: if `dotnet test` reports the same green count post-migration as pre-migration (modulo the deleted placeholder), the migration is correct.
