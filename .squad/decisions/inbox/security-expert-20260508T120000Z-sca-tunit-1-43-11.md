---
id: security-expert-20260508T120000Z-sca-tunit-1-43-11
agent: security-expert
verdict: PASS
scope: decision
created: 2026-05-08T12:00:00Z
targets:
  - path: src/Lambdaba.Tests/Lambdaba.Tests.csproj
blockers: []
high:
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "TUnit is maintained by a single individual (thomhurst). For a test-only dependency the risk is low — the project would compile and ship fine if upstream stalled — but the squad should monitor for sustained inactivity and have a pinned-version escape hatch ready."
medium:
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "TUnit pulls Microsoft.Testing.Extensions.Telemetry 2.2.2 as a transitive dependency. Set DOTNET_CLI_TELEMETRY_OPTOUT=1 in CI so test-run telemetry does not leave the build environment."
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "EnumerableAsyncProcessor 3.8.4 (transitive, also thomhurst-authored) has no known CVEs but is a single-maintainer package. Monitor via Dependabot."
good:
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "TUnit 1.43.11 — zero known CVEs across GitHub Advisory Database, NVD, and the NuGet vulnerability feed. License: MIT."
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "Microsoft.NET.Test.Sdk 17.13.0 — Microsoft-published, MIT, no known CVEs at this version. The architect-prescribed pin is honoured; the original 18.5.1 was reverted to keep the architect's directive."
  - file: src/Lambdaba.Tests/Lambdaba.Tests.csproj
    reason: "Production assembly (src/Lambdaba/) does not transitively reference TUnit — Lambdaba.Tests is the only consumer (IsTestProject=true). No production attack-surface impact."
references:
  - architect-2026-05-08T120000Z-tunit-migration-from-xunit
---

SCA review of TUnit 1.43.11 + Microsoft.NET.Test.Sdk 17.13.0 for `src/Lambdaba.Tests/` — verdict PASS, two non-blocking medium advisories.

## Scope

Adding TUnit 1.43.11 (replacing xUnit) and pinning Microsoft.NET.Test.Sdk at the architect-prescribed 17.13.0. Test-only dependency surface; production assembly untouched.

## Findings

- **Zero CVEs** across GitHub Advisory Database, NVD, NuGet vulnerability feed for TUnit 1.43.11 and all transitives (TUnit.Engine 1.43.11, TUnit.Assertions 1.43.11, TUnit.Core 1.43.11, Microsoft.Testing.Platform 2.2.2 family, EnumerableAsyncProcessor 3.8.4).
- **License hygiene clean** — MIT throughout the transitive tree.
- **Bus factor** — TUnit and EnumerableAsyncProcessor are single-maintainer packages (thomhurst). Acceptable for a test-only dependency; flag for Dependabot monitoring.
- **Telemetry** — `Microsoft.Testing.Extensions.Telemetry` ships hashed run metadata to Microsoft when run on machines without `DOTNET_CLI_TELEMETRY_OPTOUT=1`. CI must set this env var.
- **Production isolation** — `Lambdaba` (the library) does not reference `Lambdaba.Tests`; the test dependency tree cannot reach production code.

## Mitigations

- Set `DOTNET_CLI_TELEMETRY_OPTOUT=1` in CI environment (follow-up devops task; not blocking PR #1).
- Configure Dependabot for the NuGet ecosystem to surface future CVEs in the single-maintainer transitives (follow-up devops task; not blocking PR #1).

## Verdict

**PASS.** PR #1 (`test/1-tunit-migration`, commit `755c639` and pending fix-up amend) is cleared from a security standpoint.
