
namespace Lambdaba.Tests;

using static Lambdaba.Base;

public class PreludeTests
{
    [Fact]
    public void TestId()
    {
        Xunit.Assert.Equal(1, Id<int>()(1));
    }

    [Fact]
    public void TestConstant()
    {
        Xunit.Assert.Equal(1, Const<int, int>()(1)(2));
    }

    [Fact]
    public void TestCompose()
    {
        Xunit.Assert.Equal(3, Compose(a => a + 1, Id<int>())(2));
    }

    [Fact]
    public void TestFlip()
    {
        var flipStringConcat = Flip<string, string, string>(a => b => a + b);
        var result = flipStringConcat("Hello")("World");

        Xunit.Assert.Equal("WorldHello", result);


        flipStringConcat = Flip<string, string, string>()(a => b => a + b);
        result = flipStringConcat("Hello")("World");

        Xunit.Assert.Equal("WorldHello", result);
    }

    

    [Fact]
    public void TestSTimes()
    {
        static A f<A>(A xs, Int multiplier) 
            where A : Semigroup<A> => 
                A.STimes(multiplier, xs);

        Types.List<Int> xs = [1];

        Types.List<Int> expected = [1, 1, 1];

        Xunit.Assert.Equal(expected, f(xs, 3));
    }

    
}
