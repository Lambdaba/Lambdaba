namespace Lambdaba.Tests;

using static Lambdaba.Base;
using Assert = TUnit.Assertions.Assert;

public class PreludeTests
{
    [Test]
    public async Task TestId()
    {
        await Assert.That(Id<int>()(1)).IsEqualTo(1);
    }

    [Test]
    public async Task TestConstant()
    {
        await Assert.That(Const<int, int>()(1)(2)).IsEqualTo(1);
    }

    [Test]
    public async Task TestCompose()
    {
        await Assert.That(Compose(a => a + 1, Id<int>())(2)).IsEqualTo(3);
    }

    [Test]
    public async Task TestFlip()
    {
        var flipStringConcat = Flip<string, string, string>(a => b => a + b);
        var result = flipStringConcat("Hello")("World");

        await Assert.That(result).IsEqualTo("WorldHello");

        flipStringConcat = Flip<string, string, string>()(a => b => a + b);
        result = flipStringConcat("Hello")("World");

        await Assert.That(result).IsEqualTo("WorldHello");
    }

    [Test]
    public async Task TestSTimes()
    {
        static A f<A>(A xs, Int multiplier)
            where A : Semigroup<A> =>
                A.STimes(multiplier, xs);

        Types.List<Int> xs = [1];

        Types.List<Int> expected = [1, 1, 1];

        await Assert.That(f(xs, 3)).IsEquivalentTo(expected);
    }
}
