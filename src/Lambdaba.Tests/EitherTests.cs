namespace Lambdaba.Tests;

using static Lambdaba.Base;
using Assert = TUnit.Assertions.Assert;

public class EitherTests
{
    [Test]
    public async Task Bind_Right_PropagatesValue()
    {
        Either<string, int> e = new Right<int>(1);
        var result = (Either<string, int>)Either<string>.Bind(e, x => (Data<Either<string>, int>)new Either<string, int>(new Right<int>(x + 1)));

        result.TryGetValue(out Right<int>? r);
        await Assert.That(r).IsNotNull();
        await Assert.That(r!.Value).IsEqualTo(2);
    }

    [Test]
    public async Task Bind_Left_StaysLeft()
    {
        Either<string, int> e = new Left<string>("err");
        var result = (Either<string, int>)Either<string>.Bind(e, x => (Data<Either<string>, int>)new Either<string, int>(new Right<int>(x + 1)));

        result.TryGetValue(out Left<string>? l);
        await Assert.That(l).IsNotNull();
        await Assert.That(l!.Value).IsEqualTo("err");
    }

    [Test]
    public async Task SelectMany_ProjectsResult()
    {
        Either<string, int> e = new Right<int>(1);
        var result = (Either<string, int>)Either<string>.SelectMany(e,
            x => (Data<Either<string>, int>)new Either<string, int>(new Right<int>(x + 1)),
            (a, b) => a + b);

        result.TryGetValue(out Right<int>? r);
        await Assert.That(r).IsNotNull();
        await Assert.That(r!.Value).IsEqualTo(3);
    }

    [Test]
    public async Task Match_PicksCorrectBranch()
    {
        Either<string, int> e = new Right<int>(2);
        var result = Either<string>.Match(e, l => l.Length, r => r * 2);
        await Assert.That(result).IsEqualTo(4);
    }

    [Test]
    public async Task IsLeft_IsRight_Work()
    {
        Either<string, int> left = new Left<string>("err");
        Either<string, int> right = new Right<int>(1);

        await Assert.That(Either<string>.IsLeft(left)).IsEqualTo(new True());
        await Assert.That(Either<string>.IsRight(left)).IsEqualTo(new False());

        await Assert.That(Either<string>.IsLeft(right)).IsEqualTo(new False());
        await Assert.That(Either<string>.IsRight(right)).IsEqualTo(new True());
    }

    [Test]
    public async Task MapLeft_TransformsLeftValue()
    {
        Either<string, int> left = new Left<string>("err");
        var result = (Either<int, int>)Either<string>.MapLeft<int, int>(s => s.Length, left);

        result.TryGetValue(out Left<int>? l);
        await Assert.That(l).IsNotNull();
        await Assert.That(l!.Value).IsEqualTo(3);
    }

    [Test]
    public async Task Bimap_TransformsBothSides()
    {
        Either<string, int> right = new Right<int>(1);
        var result = (Either<int, int>)Either<string>.Bimap<int, int, int>(s => s.Length, x => x + 1, right);

        result.TryGetValue(out Right<int>? r);
        await Assert.That(r).IsNotNull();
        await Assert.That(r!.Value).IsEqualTo(2);
    }

    [Test]
    public async Task Swap_FlipsConstructors()
    {
        Either<string, int> right = new Right<int>(1);
        var swapped = (Either<int, string>)Either<string>.Swap(right);

        swapped.TryGetValue(out Left<int>? l);
        await Assert.That(l).IsNotNull();
        await Assert.That(l!.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Lefts_Rights_Partition_Work()
    {
        Types.List<Either<string, int>> xs =
        [
            new Either<string, int>(new Left<string>("a")),
            new Either<string, int>(new Right<int>(1)),
            new Either<string, int>(new Left<string>("b")),
        ];
        var lefts = Either<string>.Lefts(xs);
        var rights = Either<string>.Rights(xs);
        var (partLefts, partRights) = Either<string>.Partition(xs);

        Types.List<string> expectedLefts = ["a", "b"];
        Types.List<int> expectedRights = [1];

        await Assert.That(lefts).IsEquivalentTo(expectedLefts);
        await Assert.That(rights).IsEquivalentTo(expectedRights);
        await Assert.That(partLefts).IsEquivalentTo(expectedLefts);
        await Assert.That(partRights).IsEquivalentTo(expectedRights);
    }
}
