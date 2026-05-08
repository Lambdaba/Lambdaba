---
id: security-expert-20260508T140000Z-sca-microsoft-codeanalysis-csharp-5-3-0
agent: security-expert
verdict: PASS
scope: decision
created: 2026-05-08T14:00:00Z
targets:
  - path: src/Lambdaba.SourceGenerators/Lambdaba.SourceGenerators.csproj
blockers: []
high:
  - "Microsoft.CodeAnalysis.Analyzers 5.3.0-2.25625.1 is a prerelease transitive dependency; pin it explicitly in Directory.Packages.props when that version reaches stable GA to remove the prerelease label from the restore graph."
medium:
  - "No NuGetAudit or Directory.Packages.props entry is currently visible for the SourceGenerators project; confirm NuGetAudit=true and NuGetAuditMode=all are inherited from Directory.Build.props so future CVE disclosures break the build automatically."
good:
  - "PrivateAssets='all' is correctly declared on the PackageReference, ensuring Microsoft.CodeAnalysis and its entire transitive closure never propagate to consumers of the Lambdaba library."
  - "All packages in the dependency tree (Microsoft.CodeAnalysis.CSharp 5.3.0, Microsoft.CodeAnalysis.Common 5.3.0, Microsoft.CodeAnalysis.Analyzers 5.3.0-2.25625.1) carry MIT licenses with no licensing risk."
references: []
---

SCA verdict PASS for Microsoft.CodeAnalysis.CSharp 5.3.0 in Lambdaba.SourceGenerators: zero CVEs found across the full transitive tree; PrivateAssets isolation is correctly applied; one high advisory regarding prerelease transitive stabilisation.

## Package under review

| Field | Value |
|---|---|
| Package | `Microsoft.CodeAnalysis.CSharp` |
| Version | `5.3.0` |
| Project | `src/Lambdaba.SourceGenerators/Lambdaba.SourceGenerators.csproj` |
| Isolation | `PrivateAssets="all"` |
| Consumer exposure | None — generator output only |

## 1. Vulnerability scan

Sources queried: OSV (via API), GitHub Advisory Database (GHSA), NVD, NuGet Gallery vulnerability tab, Microsoft Security Advisory search (MSRC).

### Direct dependency

**Microsoft.CodeAnalysis.CSharp 5.3.0** — no advisories found on NuGet Gallery, no matching GHSA entries, no NVD record. The only notable issue surfaced by search is a regression bug (dotnet/roslyn #82780: generated interceptors not applied) which is a correctness defect, not a security vulnerability, and has no CVE assignment.

### Transitive dependency tree (netstandard2.0 target frame)

| Package | Version | Vulnerabilities found |
|---|---|---|
| Microsoft.CodeAnalysis.Common | 5.3.0 | None |
| Microsoft.CodeAnalysis.Analyzers | 5.3.0-2.25625.1 | None |
| System.Reflection.Metadata | 9.0.0 | None |
| System.Collections.Immutable | 9.0.0 | None |
| System.Buffers | 4.6.0 | None |
| System.Memory | 4.6.0 | None |
| System.Numerics.Vectors | 4.6.0 | None |
| System.Runtime.CompilerServices.Unsafe | 6.1.0 | None |
| System.Text.Encoding.CodePages | 8.0.0 | None |
| System.Threading.Tasks.Extensions | 4.6.0 | None |

The two recent high-severity .NET advisories that appeared in search (GHSA-37gx-xxp4-5rgx / CVE-2026-33116 and GHSA-w3x6-4m5h-cxqf / CVE-2026-26171) were investigated. Both exclusively affect `System.Security.Cryptography.Xml`. Neither `Microsoft.CodeAnalysis.CSharp` nor any package in this tree pulls in `System.Security.Cryptography.Xml`. Those advisories are not applicable.

**Result: zero known CVEs or GHSA advisories affecting any package in this dependency tree as of 2026-05-08.**

## 2. License check

All packages in the tree are published by Microsoft under the MIT license. No copyleft, no proprietary, no dual-license edge case. License risk: none.

## 3. Prerelease transitive dependency analysis

`Microsoft.CodeAnalysis.Analyzers 5.3.0-2.25625.1` is a prerelease build pulled transitively by both `Microsoft.CodeAnalysis.CSharp 5.3.0` and `Microsoft.CodeAnalysis.Common 5.3.0`. As of the date of this review (2026-05-08) its NuGet Gallery page records 1,362,316 downloads and was last updated 2026-03-11, indicating broad real-world use.

**Risk assessment for a generator project:**

The Analyzers package is consumed exclusively at build time within the generator project. Its purpose is to verify that the generator's own code correctly implements the `IIncrementalGenerator` contract; it emits Roslyn diagnostic warnings during compilation of the generator itself. It does not ship in the generator's output NuGet package to library consumers, and it does not execute at the runtime of any application.

`PrivateAssets="all"` on the top-level `Microsoft.CodeAnalysis.CSharp` reference causes the NuGet SDK to treat the entire closure — including this prerelease transitive — as build-private. The `.nupkg` that ships to consumers of `Lambdaba` contains none of these assemblies and does not list them as dependencies. Consumer `dotnet restore` operations are unaffected.

The remaining risk is narrow: a prerelease package can introduce breaking API changes or be yanked without a stable replacement, which would break the generator's own build. That is a build-stability concern, not a security concern. No attack surface is introduced to end users.

**Recommendation (high, non-blocking):** When `Microsoft.CodeAnalysis.Analyzers 5.3.0` stable ships (expected to follow the same release train as `Microsoft.CodeAnalysis.CSharp 5.3.0` stable), pin the stable version in `Directory.Packages.props` to eliminate the prerelease label from the restore graph and satisfy stricter supply-chain policies.

## 4. Supply-chain and publisher trust

All packages originate from `github.com/dotnet/roslyn` (Microsoft, the .NET Compiler Platform team). The source commit for `Microsoft.CodeAnalysis.Common 5.3.0` is publicly recorded on NuGet (`16f9bd284cd49604ac82998bfe778a8eb16d4347`). This is the same team that ships the C# compiler and the Roslyn SDK; there is no third-party supply-chain vector.

## 5. Verdict and reviewer re-request summary

**Verdict: PASS.**

The reviewer's flag is resolved. `Microsoft.CodeAnalysis.CSharp 5.3.0` and its full transitive tree carry zero known CVEs or GHSA advisories as of 2026-05-08, are uniformly MIT-licensed, and originate from Microsoft's own Roslyn team. The prerelease transitive `Microsoft.CodeAnalysis.Analyzers 5.3.0-2.25625.1` is build-time only, isolated behind `PrivateAssets="all"`, and never reaches library consumers; its prerelease status is a build-stability advisory rather than a security finding. No blocker exists. The high advisory above (stabilise the Analyzers pin when GA lands) is a future hygiene action, not a merge gate. PR #2 may proceed from a security standpoint.
