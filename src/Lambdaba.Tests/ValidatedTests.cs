namespace Lambdaba.Tests;

using static Lambdaba.Base;
using Assert = TUnit.Assertions.Assert;

/// <summary>
/// Tests for the Validated applicative covering brand-side operations (Pure, FMap, Bind,
/// Apply, SelectMany) and generator-emitted accessors (Match, IsValid, IsInvalid,
/// TryGetValue, HasValue).
///
/// Assertions extract inner values via generated TryGetValue or Match rather than
/// comparing whole <see cref="Validated{A}"/> instances (which are reference-type
/// classes without value equality).
/// </summary>
public class ValidatedTests
{
    // ──────────────────────────────────────────────
    // Pure
    // ──────────────────────────────────────────────

    [Test]
    public async Task Pure_ProducesValid()
    {
        var result = (Validated<int>)Validated.Pure<int>(42);

        result.TryGetValue(out Valid<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(42);
    }

    // ──────────────────────────────────────────────
    // FMap
    // ──────────────────────────────────────────────

    [Test]
    public async Task FMap_Valid_TransformsValue()
    {
        Validated<string> valid = new Valid<string>("hello");
        var result = (Validated<int>)Validated.FMap<string, int>(s => s.Length, valid);

        result.TryGetValue(out Valid<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(5);
    }

    [Test]
    public async Task FMap_Invalid_PropagatesReasons()
    {
        Validated<string> invalid = new Invalid("reason-1");
        var result = (Validated<int>)Validated.FMap<string, int>(s => s.Length, invalid);

        result.TryGetValue(out Invalid? inv);
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "reason-1" });
    }

    // ──────────────────────────────────────────────
    // Bind
    // ──────────────────────────────────────────────

    [Test]
    public async Task Bind_Valid_AppliesFunction()
    {
        Validated<int> valid = new Valid<int>(5);
        var result = (Validated<int>)Validated.Bind(valid, x => (Data<Validated, int>)new Validated<int>(new Valid<int>(x + 1)));

        result.TryGetValue(out Valid<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(6);
    }

    [Test]
    public async Task Bind_Invalid_ShortCircuits()
    {
        Validated<int> invalid = new Invalid("bad-input");
        var result = (Validated<int>)Validated.Bind(invalid, x => (Data<Validated, int>)new Validated<int>(new Valid<int>(x + 1)));

        result.TryGetValue(out Invalid? inv);
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "bad-input" });
    }

    // ──────────────────────────────────────────────
    // Apply — all four combinations, including the accumulation case
    // ──────────────────────────────────────────────

    [Test]
    public async Task Apply_BothValid_AppliesFunction()
    {
        Validated<Func<int, int>> f = new Valid<Func<int, int>>(x => x * 2);
        Validated<int> v = new Valid<int>(4);
        var result = (Validated<int>)Validated.Apply(f, v);

        result.TryGetValue(out Valid<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(8);
    }

    [Test]
    public async Task Apply_InvalidFunction_ValidValue_PropagatesFunctionReasons()
    {
        Validated<Func<int, int>> f = new Invalid("fn-error");
        Validated<int> v = new Valid<int>(4);
        var result = (Validated<int>)Validated.Apply(f, v);

        result.TryGetValue(out Invalid? inv);
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "fn-error" });
    }

    [Test]
    public async Task Apply_ValidFunction_InvalidValue_PropagatesValueReasons()
    {
        Validated<Func<int, int>> f = new Valid<Func<int, int>>(x => x * 2);
        Validated<int> v = new Invalid("val-error");
        var result = (Validated<int>)Validated.Apply(f, v);

        result.TryGetValue(out Invalid? inv);
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "val-error" });
    }

    [Test]
    public async Task Apply_BothInvalid_AccumulatesBothReasonLists()
    {
        // Key validation-applicative behaviour: errors accumulate, not short-circuit.
        Validated<Func<int, int>> f = new Invalid("fn-error-1", "fn-error-2");
        Validated<int> v = new Invalid("val-error-1");
        var result = (Validated<int>)Validated.Apply(f, v);

        await Assert.That(result.IsInvalid).IsTrue();
        var ok = result.TryGetValue(out Invalid? inv);
        await Assert.That(ok).IsTrue();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "fn-error-1", "fn-error-2", "val-error-1" });
    }

    // ──────────────────────────────────────────────
    // SelectMany
    // ──────────────────────────────────────────────

    [Test]
    public async Task SelectMany_BothValid_ProjectsResult()
    {
        Validated<int> va = new Valid<int>(3);
        Validated<int> vb = new Valid<int>(4);
        var result = (Validated<int>)Validated.SelectMany<int, int, int>(va, _ => vb, (a, b) => a + b);

        result.TryGetValue(out Valid<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(7);
    }

    [Test]
    public async Task SelectMany_FirstInvalid_ShortCircuits()
    {
        Validated<int> va = new Invalid("first-invalid");
        Validated<int> vb = new Valid<int>(4);
        var result = (Validated<int>)Validated.SelectMany<int, int, int>(va, _ => vb, (a, b) => a + b);

        result.TryGetValue(out Invalid? inv);
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "first-invalid" });
    }

    // ──────────────────────────────────────────────
    // Generator-emitted instance Match
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_Match_Valid_RoutesToValidArm()
    {
        Validated<int> valid = new Valid<int>(99);
        var result = valid.Match(
            onValid: v => $"valid:{v.Value}",
            onInvalid: _ => "invalid");
        await Assert.That(result).IsEqualTo("valid:99");
    }

    [Test]
    public async Task Generated_Match_Invalid_RoutesToInvalidArm()
    {
        Validated<int> invalid = new Invalid("e1", "e2");
        var result = invalid.Match(
            onValid: _ => "valid",
            onInvalid: inv => string.Join(",", inv.Reasons));
        await Assert.That(result).IsEqualTo("e1,e2");
    }

    // ──────────────────────────────────────────────
    // Generator-emitted IsValid / IsInvalid
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_IsValid_IsInvalid_Discriminators_Work()
    {
        Validated<int> valid = new Valid<int>(1);
        Validated<int> invalid = new Invalid("x");

        await Assert.That(valid.IsValid).IsTrue();
        await Assert.That(valid.IsInvalid).IsFalse();
        await Assert.That(invalid.IsInvalid).IsTrue();
        await Assert.That(invalid.IsValid).IsFalse();
    }

    // ──────────────────────────────────────────────
    // Generator-emitted TryGetValue overloads
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_TryGetValue_Valid_SucceedsOnValid()
    {
        Validated<int> valid = new Valid<int>(7);
        var ok = valid.TryGetValue(out Valid<int>? value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value!.Value).IsEqualTo(7);
    }

    [Test]
    public async Task Generated_TryGetValue_Invalid_SucceedsOnInvalid()
    {
        Validated<int> invalid = new Invalid("reason");
        var ok = invalid.TryGetValue(out Invalid? inv);
        await Assert.That(ok).IsTrue();
        await Assert.That(inv).IsNotNull();
        await Assert.That(inv!.Reasons).IsEquivalentTo(new[] { "reason" });
    }

    // ──────────────────────────────────────────────
    // Generator-emitted HasValue
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_HasValue_AlwaysTrueForConstructedUnion()
    {
        Validated<int> valid = new Valid<int>(1);
        Validated<int> invalid = new Invalid("e");

        await Assert.That(valid.HasValue).IsTrue();
        await Assert.That(invalid.HasValue).IsTrue();
    }
}
