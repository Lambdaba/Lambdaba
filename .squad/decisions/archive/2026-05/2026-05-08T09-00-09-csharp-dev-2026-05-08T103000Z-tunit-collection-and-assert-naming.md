---
id: csharp-dev-2026-05-08T103000Z-tunit-collection-and-assert-naming
agent: csharp-dev
verdict: INFO
scope: decision
created: 2026-05-08T10:30:00Z
targets:
  - path: src/Lambdaba.Tests/GlobalUsings.cs
  - path: src/Lambdaba.Tests/EitherTests.cs
  - path: src/Lambdaba.Tests/ListTests.cs
blockers: []
high: []
medium: []
good: []
references:
  - architect-2026-05-08T120000Z-tunit-migration-from-xunit
---

Two conventions established during the xUnit → TUnit migration (PR `test/1-tunit-migration`).

## 1. Assert name collision with `using static Lambdaba.Base`

`Lambdaba.Base` exports a generic method `Assert<A>()` (a Haskell-style assertion combinator). Any test file that imports it via `using static Lambdaba.Base;` will have the `Assert` identifier resolve to `Base.Assert<A>()` rather than `TUnit.Assertions.Assert`. This shadows TUnit's assertion class.

**Convention:** Add `using Assert = TUnit.Assertions.Assert;` at file scope in every test file that also has `using static Lambdaba.Base;`. The file-level alias takes precedence over the static import. The `GlobalUsings.cs` also carries a global alias as a fallback for files that do not use the static import.

## 2. Collection assertions: IsEqualTo vs IsEquivalentTo

TUnit's `IsEqualTo` resolves via `Equals()`. For types implementing `IReadOnlyList<T>` (including `Types.List<A>`), TUnit uses reference comparison rather than element-wise sequence comparison. xUnit's `Assert.Equal` for `IEnumerable<T>` did element-wise comparison.

**Convention:** When asserting equality on `Types.List<T>` values (or any custom `IReadOnlyList<T>` that does not override `Equals` with sequence semantics), use `IsEquivalentTo(typed-expected-variable)`. The expected value must be explicitly typed as the same `Types.List<T>` — do not use collection literal inference alone, as `[1, 2]` may be inferred as `int[]` rather than `Types.List<Base.Int>`, causing a generic constraint failure on `IsEquivalentTo<TCollection, TItem>`.

## 3. dotnet test --nologo is not supported with MTP

On .NET SDK 10+, TUnit uses Microsoft.Testing.Platform (MTP) rather than VSTest. In MTP mode, `dotnet test` forwards all unknown flags to the test binary. The `--nologo` flag is not recognised by the TUnit test runner and causes early exit with "Unknown option '--nologo'" and zero tests running.

**Convention:** Use `dotnet test` (without `--nologo`) or `dotnet test --disable-logo` to run tests. The `global.json` at the repo root opts the project into the MTP test runner via `"test": { "runner": "Microsoft.Testing.Platform" }`.
