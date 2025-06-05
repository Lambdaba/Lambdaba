namespace Lambdaba.Tests;

using Lambdaba;
using static Lambdaba.Base;

public class EitherTests
{
    [Fact]
    public void Bind_Right_PropagatesValue()
    {
        Data<Either<string>, int> e = new Right<string, int>(1);
        var result = Either<string>.Bind(e, x => new Right<string, int>(x + 1));
        Xunit.Assert.Equal(new Right<string, int>(2), result);
    }

    [Fact]
    public void Bind_Left_StaysLeft()
    {
        Data<Either<string>, int> e = new Left<string, int>("err");
        var result = Either<string>.Bind(e, x => new Right<string, int>(x + 1));
        Xunit.Assert.Equal(new Left<string, int>("err"), result);
    }

    [Fact]
    public void SelectMany_ProjectsResult()
    {
        Data<Either<string>, int> e = new Right<string, int>(1);
        var result = Either<string>.SelectMany(e, x => new Right<string, int>(x + 1), (a, b) => a + b);
        Xunit.Assert.Equal(new Right<string, int>(3), result);
    }

    [Fact]
    public void Match_PicksCorrectBranch()
    {
        Data<Either<string>, int> e = new Right<string, int>(2);
        var result = Either<string>.Match(e, l => l.Length, r => r * 2);
        Xunit.Assert.Equal(4, result);
    }

    [Fact]
    public void IsLeft_IsRight_Work()
    {
        Data<Either<string>, int> left = new Left<string, int>("err");
        Data<Either<string>, int> right = new Right<string, int>(1);

        Xunit.Assert.Equal(new True(), Either<string>.IsLeft(left));
        Xunit.Assert.Equal(new False(), Either<string>.IsRight(left));

        Xunit.Assert.Equal(new False(), Either<string>.IsLeft(right));
        Xunit.Assert.Equal(new True(), Either<string>.IsRight(right));
    }

    [Fact]
    public void MapLeft_TransformsLeftValue()
    {
        Data<Either<string>, int> left = new Left<string, int>("err");
        var result = Either<string>.MapLeft<int, int>(s => s.Length, left);
        Xunit.Assert.Equal(new Left<int, int>(3), result);
    }

    [Fact]
    public void Bimap_TransformsBothSides()
    {
        Data<Either<string>, int> right = new Right<string, int>(1);
        var result = Either<string>.Bimap<int, int, int>(s => s.Length, x => x + 1, right);
        Xunit.Assert.Equal(new Right<int, int>(2), result);
    }

    [Fact]
    public void Swap_FlipsConstructors()
    {
        Data<Either<string>, int> right = new Right<string, int>(1);
        var swapped = Either<string>.Swap(right);
        Xunit.Assert.Equal(new Left<int, string>(1), swapped);
    }

    [Fact]
    public void Either_Function_Works()
    {
        Data<Either<string>, int> val = new Right<string, int>(2);
        var res = Either<string>.Either(s => s.Length, x => x + 1, val);
        Assert.Equal(3, res);
    }

    [Fact]
    public void Lefts_Rights_Partition_Work()
    {
        Types.List<Either<string, int>> xs = [new Left<string, int>("a"), new Right<string, int>(1), new Left<string, int>("b")];
        var lefts = Either<string>.Lefts(xs);
        var rights = Either<string>.Rights(xs);
        var part = Either<string>.Partition(xs);

        Xunit.Assert.Equal(["a", "b"], lefts);
        Xunit.Assert.Equal([1], rights);
        Xunit.Assert.Equal((["a", "b"], [1]), part);
    }
}
