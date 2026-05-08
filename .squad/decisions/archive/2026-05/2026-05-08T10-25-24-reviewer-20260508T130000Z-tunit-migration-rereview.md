---
id: reviewer-20260508T130000Z-tunit-migration-rereview
agent: reviewer
verdict: PASS
scope: review
created: 2026-05-08T13:00:00Z
targets:
  - "src/Lambdaba.Tests/Lambdaba.Tests.csproj line 14"
  - "src/Lambdaba.Tests/ListTests.cs lines 13,67"
  - "src/Lambdaba.Tests/PreludeTest.cs line 51"
  - ".squad/decisions/inbox/security-expert-20260508T120000Z-sca-tunit-1-43-11.md"
blockers: []
high: []
medium: []
good:
  - "All six prior 🔴 must-fix items resolved cleanly with a surgical fix-up; scope discipline held."
  - "Microsoft.NET.Test.Sdk reverted 18.5.1 → 17.13.0 honouring the architect decision drop."
  - "ListTests.cs:13 / ListTests.cs:67 / PreludeTest.cs:51 IsEqualTo → IsEquivalentTo applied for Types.List<> consistency."
  - "Security-Expert SCA drop committed; verdict PASS, MIT throughout transitive tree, zero CVEs."
  - "csharp-dev decision drop committed; audit trail restored."
  - "GPG signature verified; conventional-commit subject `test(tests)!:` correct."
  - "dotnet build 0/0; dotnet test 20/20; grep for Xunit/[Fact]/[Theory] empty."
references:
  - architect-2026-05-08T120000Z-tunit-migration-from-xunit
  - csharp-dev-2026-05-08T103000Z-tunit-collection-and-assert-naming
  - security-expert-20260508T120000Z-sca-tunit-1-43-11
---

PR #1 (`test/1-tunit-migration`) at SHA `2e0295e` approved (PASS). Author unblocked.

## Verification harness

- `git show -1 --show-signature 2e0295e` — Good GPG signature (RSA 6E6F142A...414A3), Conventional Commit `test(tests)!:` with breaking marker.
- `dotnet build Lambdaba.sln` — 0 errors, 20 warnings (all pre-existing in `src/Lambdaba/` production code, out of PR scope).
- `dotnet test --disable-logo` — 20/20 passing in 387ms.
- `grep -rE 'Xunit|\[Fact\]|\[Theory\]' src/Lambdaba.Tests/` — empty.
- `git ls-files .squad/decisions/` — all three decision drops present.

## Fix-up confirmed (diff between `755c639` and `2e0295e`)

- `src/Lambdaba.Tests/Lambdaba.Tests.csproj:14` — Microsoft.NET.Test.Sdk 18.5.1 → 17.13.0
- `src/Lambdaba.Tests/ListTests.cs:13` — `IsEqualTo` → `IsEquivalentTo` (TestAdd)
- `src/Lambdaba.Tests/ListTests.cs:67` — `IsEqualTo` → `IsEquivalentTo` (TestContents)
- `src/Lambdaba.Tests/PreludeTest.cs:51` — `IsEqualTo` → `IsEquivalentTo` (TestSTimes)
- Security Expert SCA drop and csharp-dev decision drop now both committed.

## Scope discipline

The fix-up commit changed only what the prior review flagged. No drive-by edits, no scope creep, no new files outside the decision-drop audit trail.

## Three prior 🟡 Should-Fix items

Remain advisory and not upgraded to 🔴:

- `LangVersion=preview` rationale is now visible in the commit body and architect drop (parity with main project for C# 14 extension members).
- `EitherTests` record-equality usage is idiomatic — `Right<>` / `Left<>` / `True` / `False` records have structural equality; only `Types.List<T>` collection assertions need `IsEquivalentTo`.
- The dual `Assert` alias (global + file-scoped) is a documented fallback per the csharp-dev drop, not redundancy.

## Verdict

**PASS.** Ready to merge to `main`.
