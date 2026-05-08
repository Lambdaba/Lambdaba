namespace Lambdaba.Tests;

using static Lambdaba.Base;
using Assert = TUnit.Assertions.Assert;

/// <summary>
/// Tests for the Maybe monad covering brand-side operations (Bind, FMap, Apply,
/// SelectMany, Or, Where) and generator-emitted accessors (Match, IsJust, IsNothing,
/// TryGetValue, HasValue) plus the MaybeExtensions.Fold helper.
///
/// Assertions extract inner values via generated TryGetValue or Match rather than
/// comparing whole <see cref="Maybe{A}"/> instances (which are reference-type classes
/// without value equality).
/// </summary>
public class MaybeTests
{
    // ──────────────────────────────────────────────
    // Bind
    // ──────────────────────────────────────────────

    [Test]
    public async Task Bind_Just_AppliesFunction()
    {
        Maybe<int> just = new Just<int>(3);
        var result = (Maybe<int>)Maybe.Bind(just, x => (Data<Maybe, int>)new Maybe<int>(new Just<int>(x * 2)));

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(6);
    }

    [Test]
    public async Task Bind_Nothing_ReturnsNothing()
    {
        Maybe<int> nothing = new Nothing();
        var result = (Maybe<int>)Maybe.Bind(nothing, x => (Data<Maybe, int>)new Maybe<int>(new Just<int>(x * 2)));

        await Assert.That(result.IsNothing).IsTrue();
    }

    // ──────────────────────────────────────────────
    // FMap
    // ──────────────────────────────────────────────

    [Test]
    public async Task FMap_Just_TransformsValue()
    {
        Maybe<string> just = new Just<string>("hello");
        var result = (Maybe<int>)Maybe.FMap<string, int>(s => s.Length, just);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(5);
    }

    [Test]
    public async Task FMap_Nothing_ReturnsNothing()
    {
        Maybe<string> nothing = new Nothing();
        var result = (Maybe<int>)Maybe.FMap<string, int>(s => s.Length, nothing);

        await Assert.That(result.IsNothing).IsTrue();
    }

    // ──────────────────────────────────────────────
    // Apply — all four (Just × Nothing) combinations
    // ──────────────────────────────────────────────

    [Test]
    public async Task Apply_JustFunction_JustValue_AppliesFunction()
    {
        Maybe<Func<int, int>> f = new Just<Func<int, int>>(x => x + 10);
        Maybe<int> v = new Just<int>(5);
        var result = (Maybe<int>)Maybe.Apply(f, v);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(15);
    }

    [Test]
    public async Task Apply_JustFunction_Nothing_ReturnsNothing()
    {
        Maybe<Func<int, int>> f = new Just<Func<int, int>>(x => x + 10);
        Maybe<int> v = new Nothing();
        var result = (Maybe<int>)Maybe.Apply(f, v);

        await Assert.That(result.IsNothing).IsTrue();
    }

    [Test]
    public async Task Apply_Nothing_JustValue_ReturnsNothing()
    {
        Maybe<Func<int, int>> f = new Nothing();
        Maybe<int> v = new Just<int>(5);
        var result = (Maybe<int>)Maybe.Apply(f, v);

        await Assert.That(result.IsNothing).IsTrue();
    }

    [Test]
    public async Task Apply_Nothing_Nothing_ReturnsNothing()
    {
        Maybe<Func<int, int>> f = new Nothing();
        Maybe<int> v = new Nothing();
        var result = (Maybe<int>)Maybe.Apply(f, v);

        await Assert.That(result.IsNothing).IsTrue();
    }

    // ──────────────────────────────────────────────
    // SelectMany
    // ──────────────────────────────────────────────

    [Test]
    public async Task SelectMany_BothJust_ProjectsResult()
    {
        Maybe<int> mx = new Just<int>(3);
        Maybe<int> my = new Just<int>(4);
        var result = (Maybe<int>)Maybe.SelectMany<int, int, int>(mx, _ => my, (x, y) => x + y);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(7);
    }

    [Test]
    public async Task SelectMany_FirstNothing_ReturnsNothing()
    {
        Maybe<int> mx = new Nothing();
        Maybe<int> my = new Just<int>(4);
        var result = (Maybe<int>)Maybe.SelectMany<int, int, int>(mx, _ => my, (x, y) => x + y);

        await Assert.That(result.IsNothing).IsTrue();
    }

    // ──────────────────────────────────────────────
    // Or
    // ──────────────────────────────────────────────

    [Test]
    public async Task Or_FirstJust_ReturnsFirst()
    {
        Maybe<int> first = new Just<int>(1);
        Maybe<int> second = new Just<int>(2);
        var result = (Maybe<int>)Maybe.Or(first, second);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Or_FirstNothing_ReturnsSecond()
    {
        Maybe<int> first = new Nothing();
        Maybe<int> second = new Just<int>(42);
        var result = (Maybe<int>)Maybe.Or(first, second);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(42);
    }

    // ──────────────────────────────────────────────
    // Where
    // ──────────────────────────────────────────────

    [Test]
    public async Task Where_PredicateTrue_KeepsValue()
    {
        Maybe<int> just = new Just<int>(10);
        var result = (Maybe<int>)Maybe.Where(just, x => x > 5);

        result.TryGetValue(out Just<int>? value);
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo(10);
    }

    [Test]
    public async Task Where_PredicateFalse_DropsToNothing()
    {
        Maybe<int> just = new Just<int>(3);
        var result = (Maybe<int>)Maybe.Where(just, x => x > 5);

        await Assert.That(result.IsNothing).IsTrue();
    }

    // ──────────────────────────────────────────────
    // Generator-emitted instance Match
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_Match_Just_RoutesToJustArm()
    {
        Maybe<string> just = new Just<string>("world");
        var result = just.Match(
            onJust: j => $"just:{j.Value}",
            onNothing: _ => "nothing");
        await Assert.That(result).IsEqualTo("just:world");
    }

    [Test]
    public async Task Generated_Match_Nothing_RoutesToNothingArm()
    {
        Maybe<string> nothing = new Nothing();
        var result = nothing.Match(
            onJust: j => $"just:{j.Value}",
            onNothing: _ => "nothing");
        await Assert.That(result).IsEqualTo("nothing");
    }

    // ──────────────────────────────────────────────
    // Generator-emitted IsJust / IsNothing
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_IsJust_IsNothing_Discriminators_Work()
    {
        Maybe<string> just = new Just<string>("x");
        Maybe<string> nothing = new Nothing();

        await Assert.That(just.IsJust).IsTrue();
        await Assert.That(just.IsNothing).IsFalse();
        await Assert.That(nothing.IsNothing).IsTrue();
        await Assert.That(nothing.IsJust).IsFalse();
    }

    // ──────────────────────────────────────────────
    // Generator-emitted TryGetValue overloads
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_TryGetValue_Just_SucceedsOnJust()
    {
        Maybe<string> just = new Just<string>("payload");
        var ok = just.TryGetValue(out Just<string>? value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsNotNull();
        await Assert.That(value!.Value).IsEqualTo("payload");
    }

    [Test]
    public async Task Generated_TryGetValue_Just_FailsOnNothing()
    {
        Maybe<string> nothing = new Nothing();
        var ok = nothing.TryGetValue(out Just<string>? _);
        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task Generated_TryGetValue_Nothing_SucceedsOnNothing()
    {
        Maybe<string> nothing = new Nothing();
        var ok = nothing.TryGetValue(out Nothing? value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsNotNull();
    }

    // ──────────────────────────────────────────────
    // Generator-emitted HasValue
    // ──────────────────────────────────────────────

    [Test]
    public async Task Generated_HasValue_AlwaysTrueForConstructedUnion()
    {
        Maybe<string> just = new Just<string>("x");
        Maybe<string> nothing = new Nothing();

        await Assert.That(just.HasValue).IsTrue();
        await Assert.That(nothing.HasValue).IsTrue();
    }

    // ──────────────────────────────────────────────
    // MaybeExtensions.Fold (Haskell: maybe def f m)
    // ──────────────────────────────────────────────

    [Test]
    public async Task Fold_Just_AppliesFunction()
    {
        Maybe<int> just = new Just<int>(7);
        var result = just.Fold(defaultValue: 0, f: x => x * 3);
        await Assert.That(result).IsEqualTo(21);
    }

    [Test]
    public async Task Fold_Nothing_ReturnsDefault()
    {
        Maybe<int> nothing = new Nothing();
        var result = nothing.Fold(defaultValue: 99, f: x => x * 3);
        await Assert.That(result).IsEqualTo(99);
    }
}
