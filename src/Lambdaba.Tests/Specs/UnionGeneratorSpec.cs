// SPDX-Identifier: Architect-approved Spec-by-Example test for the [Union] source generator.
// Originally approved 2026-05-08 by the Architect.
// Re-approved 2026-05-08 by the user, via the Lead, after closing the
// `TryGet<Case>` vs `TryGetValue` open question in favour of the
// spec-blessed non-boxing access pattern (`TryGetValue` overloads + `HasValue`).
//
// Immutable during implementation per `.claude/docs/decisions.md:117-118` and the
// `spec-by-example` skill. If implementation reveals this test is wrong, the
// implementer pauses and the test change is re-approved by the user before
// continuing — silent edits are a 🔴 Must Fix at review.
//
// Layer: workflow-direct (pure domain library, no AppHost, no UI, no IO).
// Author of record after handoff: csharp-dev.
// Decision references: ADR-0001-native-discriminated-unions (Accepted 2026-05-08),
//                      architect-2026-05-08T120000Z-tunit-migration-from-xunit.

using Lambdaba;
using static Lambdaba.Base;

namespace Lambdaba.Tests.Specs;

/// <summary>
/// Executable specification for the <c>[Union]</c> source generator's emission contract.
///
/// The generator is the subject under test. The behaviours below are the contract the
/// ADR commits to. If every test in this class is green against an unmodified test file,
/// the generator implementation is done — by definition.
///
/// What this spec asserts (the contract, locked 2026-05-08):
///  1. A <c>[Union] partial class Maybe&lt;A&gt;</c> can round-trip its case values
///     through the generated instance <c>Match</c> visitor — for both <c>Just</c>
///     and <c>Nothing</c> arms.
///  2. The generator emits <c>bool IsJust</c> / <c>bool IsNothing</c> per-case
///     discriminator properties (orthogonal to <c>HasValue</c>).
///  3. The generator emits <c>bool TryGetValue(out Just&lt;A&gt;? value)</c> and
///     <c>bool TryGetValue(out Nothing? value)</c> overloads on the union (the C# 15
///     spec's non-boxing access pattern, picked up by the compiler's optimised
///     pattern-matching lowering). Both overloads must be exercised on both
///     branches (<c>Just</c> value and <c>Nothing</c> value).
///  4. The generator emits a <c>bool HasValue</c> property as prescribed by the
///     non-boxing access pattern.
///  5. The generator emits a brand-side static helper
///     <c>Maybe.Match&lt;A, R&gt;(Data&lt;Maybe, A&gt;, Func&lt;Just&lt;A&gt;, R&gt;, Func&lt;Nothing, R&gt;)</c>
///     that hides the <c>(Maybe&lt;A&gt;)data</c> cast and dispatches on the wrapped
///     value — exercised on both <c>Just</c> and <c>Nothing</c> branches.
///
/// What this spec does NOT assert (deliberately):
///  - The exact text of the generated source (no string-equality checks against `.g.cs`).
///  - Internals of the IIncrementalGenerator pipeline (the contract is the emitted API,
///    not the implementation strategy).
///  - The <c>LMBD001</c> shadow warning (covered by a separate diagnostic test the
///    C# Dev adds during implementation; not part of the user-facing happy path).
///  - Edge cases for arities other than the <c>Maybe</c>-family two-case shape; the
///    other unions get their own test files (<c>ValidatedTests.cs</c>,
///    <c>EitherTests.cs</c>, <c>UnionPrimitiveTests.cs</c>) added during implementation.
///
/// If this spec passes, the generator's contract is proven on the canonical two-case
/// HKT-bearing union (<c>Maybe&lt;A&gt;</c>). The other unions exercise the same
/// emission shape; their tests are covered by the implementation work item.
/// </summary>
public class UnionGeneratorSpec
{
    [Test]
    public async Task Generated_instance_Match_routes_a_Just_value_to_the_Just_arm()
    {
        Maybe<int> just = new Just<int>(42);

        var result = just.Match(
            onJust: j => $"got {j.Value}",
            onNothing: _ => "empty");

        await Assert.That(result).IsEqualTo("got 42");
    }

    [Test]
    public async Task Generated_instance_Match_routes_a_Nothing_value_to_the_Nothing_arm()
    {
        Maybe<int> nothing = new Nothing();

        var result = nothing.Match(
            onJust: j => $"got {j.Value}",
            onNothing: _ => "empty");

        await Assert.That(result).IsEqualTo("empty");
    }

    [Test]
    public async Task Generated_IsJust_returns_true_for_a_Just_value_and_false_for_a_Nothing_value()
    {
        Maybe<int> just = new Just<int>(7);
        Maybe<int> nothing = new Nothing();

        await Assert.That(just.IsJust).IsTrue();
        await Assert.That(nothing.IsJust).IsFalse();
    }

    [Test]
    public async Task Generated_IsNothing_returns_true_for_a_Nothing_value_and_false_for_a_Just_value()
    {
        Maybe<int> just = new Just<int>(7);
        Maybe<int> nothing = new Nothing();

        await Assert.That(nothing.IsNothing).IsTrue();
        await Assert.That(just.IsNothing).IsFalse();
    }

    [Test]
    public async Task Generated_TryGetValue_for_Just_overload_succeeds_with_the_carried_value_when_the_union_is_Just()
    {
        Maybe<int> just = new Just<int>(123);

        var ok = just.TryGetValue(out Just<int>? value);

        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(123);
    }

    [Test]
    public async Task Generated_TryGetValue_for_Just_overload_fails_when_the_union_is_Nothing()
    {
        Maybe<int> nothing = new Nothing();

        var ok = nothing.TryGetValue(out Just<int>? _);

        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task Generated_TryGetValue_for_Nothing_overload_succeeds_when_the_union_is_Nothing()
    {
        Maybe<int> nothing = new Nothing();

        var ok = nothing.TryGetValue(out Nothing? value);

        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsNotNull();
    }

    [Test]
    public async Task Generated_TryGetValue_for_Nothing_overload_fails_when_the_union_is_Just()
    {
        Maybe<int> just = new Just<int>(7);

        var ok = just.TryGetValue(out Nothing? _);

        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task Generated_HasValue_is_true_for_a_constructed_Just_value()
    {
        Maybe<int> just = new Just<int>(1);

        await Assert.That(just.HasValue).IsTrue();
    }

    [Test]
    public async Task Generated_HasValue_is_true_for_a_constructed_Nothing_value()
    {
        Maybe<int> nothing = new Nothing();

        await Assert.That(nothing.HasValue).IsTrue();
    }

    [Test]
    public async Task Brand_side_Match_dispatches_a_Data_of_Maybe_through_the_Just_arm()
    {
        Data<Maybe, int> data = new Maybe<int>(new Just<int>(99));

        var result = Maybe.Match<int, string>(
            data,
            onJust: j => $"brand-got {j.Value}",
            onNothing: _ => "brand-empty");

        await Assert.That(result).IsEqualTo("brand-got 99");
    }

    [Test]
    public async Task Brand_side_Match_dispatches_a_Data_of_Maybe_through_the_Nothing_arm()
    {
        Data<Maybe, int> data = new Maybe<int>(new Nothing());

        var result = Maybe.Match<int, string>(
            data,
            onJust: j => $"brand-got {j.Value}",
            onNothing: _ => "brand-empty");

        await Assert.That(result).IsEqualTo("brand-empty");
    }
}
