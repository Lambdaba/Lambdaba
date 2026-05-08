namespace Lambdaba.Tests;

public class ListTests
{
    [Test]
    public async Task TestAdd()
    {
        Types.List<Base.Int> xs = [1, 2, 3];
        var ys = xs.Add(4);

        Types.List<Base.Int> expected = [4, 1, 2, 3];

        await Assert.That(ys).IsEquivalentTo(expected);
    }

    [Test]
    public async Task TestSTimes()
    {
        var xs = Types.List<Base.Int>.STimes(2, [1]);

        Types.List<Base.Int> expected = [1, 1];

        await Assert.That(xs).IsEquivalentTo(expected);
    }

    [Test]
    public async Task TestCount()
    {
        Types.List<Base.Int> xs = [1, 1];
        await Assert.That(xs.Count).IsEqualTo(2);
    }

    [Test]
    public async Task TestMConcat()
    {
        Types.List<Types.List<Base.Int>> xs = [[1], [1], [1], [1]];

        Types.List<Base.Int> expected = [1, 1, 1, 1];

        await Assert.That(Types.List<Base.Int>.MConcat(xs)).IsEquivalentTo(expected);
    }

    [Test]
    public async Task TestCombine()
    {
        Types.List<Base.Int> xs = [1, 2, 3];
        Types.List<Base.Int> ys = [4, 5, 6];

        Types.List<Base.Int> expected = [1, 2, 3, 4, 5, 6];

        await Assert.That(Types.List<Base.Int>.Combine(xs, ys)).IsEquivalentTo(expected);
    }

    [Test]
    public async Task TestContents()
    {
        Types.List<Base.Int> nonEmpty = [1, 2, 3];

        var x = nonEmpty switch
        {
            [] => [],
            [var ax, .. var xs] => xs
        };

        Types.List<Base.Int> expected = [2, 3];

        await Assert.That(x).IsEquivalentTo(expected);
    }
}
