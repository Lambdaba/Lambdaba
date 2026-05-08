namespace Lambdaba.Tests;

using static Lambdaba.Base;
using Assert = TUnit.Assertions.Assert;

public class EitherTests
{
    [Test]
    public async Task Bind_Right_PropagatesValue()
    {
        Data<Either<string>, int> e = new Right<string, int>(1);
        var result = Either<string>.Bind(e, x => new Right<string, int>(x + 1));
        await Assert.That(result).IsEqualTo(new Right<string, int>(2));
    }

    [Test]
    public async Task Bind_Left_StaysLeft()
    {
        Data<Either<string>, int> e = new Left<string, int>("err");
        var result = Either<string>.Bind(e, x => new Right<string, int>(x + 1));
        await Assert.That(result).IsEqualTo(new Left<string, int>("err"));
    }

    [Test]
    public async Task SelectMany_ProjectsResult()
    {
        Data<Either<string>, int> e = new Right<string, int>(1);
        var result = Either<string>.SelectMany(e, x => new Right<string, int>(x + 1), (a, b) => a + b);
        await Assert.That(result).IsEqualTo(new Right<string, int>(3));
    }

    [Test]
    public async Task Match_PicksCorrectBranch()
    {
        Data<Either<string>, int> e = new Right<string, int>(2);
        var result = Either<string>.Match(e, l => l.Length, r => r * 2);
        await Assert.That(result).IsEqualTo(4);
    }

    [Test]
    public async Task IsLeft_IsRight_Work()
    {
        Data<Either<string>, int> left = new Left<string, int>("err");
        Data<Either<string>, int> right = new Right<string, int>(1);

        await Assert.That(Either<string>.IsLeft(left)).IsEqualTo(new True());
        await Assert.That(Either<string>.IsRight(left)).IsEqualTo(new False());

        await Assert.That(Either<string>.IsLeft(right)).IsEqualTo(new False());
        await Assert.That(Either<string>.IsRight(right)).IsEqualTo(new True());
    }

    [Test]
    public async Task MapLeft_TransformsLeftValue()
    {
        Data<Either<string>, int> left = new Left<string, int>("err");
        var result = Either<string>.MapLeft<int, int>(s => s.Length, left);
        await Assert.That(result).IsEqualTo(new Left<int, int>(3));
    }

    [Test]
    public async Task Bimap_TransformsBothSides()
    {
        Data<Either<string>, int> right = new Right<string, int>(1);
        var result = Either<string>.Bimap<int, int, int>(s => s.Length, x => x + 1, right);
        await Assert.That(result).IsEqualTo(new Right<int, int>(2));
    }

    [Test]
    public async Task Swap_FlipsConstructors()
    {
        Data<Either<string>, int> right = new Right<string, int>(1);
        var swapped = Either<string>.Swap(right);
        await Assert.That(swapped).IsEqualTo(new Left<int, string>(1));
    }

    [Test]
    public async Task Lefts_Rights_Partition_Work()
    {
        Types.List<Either<string, int>> xs = [new Left<string, int>("a"), new Right<string, int>(1), new Left<string, int>("b")];
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
