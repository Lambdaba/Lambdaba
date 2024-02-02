namespace Lambdaba.Tests
{
    public class ListTests
    {
        [Fact]
        public void TestAdd()
        {
            Types.List<Base.Int> xs = [1, 2, 3];
            var ys = xs.Add(4);

            Types.List<Base.Int> expected = [4, 1, 2, 3];

            Xunit.Assert.Equal(expected, ys);
        }

        [Fact]
        public void TestSTimes()
        {
            var xs = Types.List<Base.Int>.STimes(2, [1]);

            Xunit.Assert.Equal([1, 1], xs);
        }

        [Fact]
        public void TestCount()
        {
            Types.List<Base.Int> xs = [1, 1];
            Xunit.Assert.Equal(2, xs.Count);
        }

        [Fact]
        public void TestMConcat()
        {
            Types.List<Types.List<Base.Int>> xs = [[1], [1], [1], [1]];

            Xunit.Assert.Equal([1, 1, 1, 1], Types.List<Base.Int>.MConcat(xs));
        }

        [Fact]
        public void TestCombine()
        {
            Types.List<Base.Int> xs = [1, 2, 3];
            Types.List<Base.Int> ys = [4, 5, 6];

            Xunit.Assert.Equal([1, 2, 3, 4, 5, 6], Types.List<Base.Int>.Combine(xs, ys));
        }

        [Fact]
        public void TestContents()
        {
            Types.List<Base.Int> nonEmpty = [1, 2, 3];

            var x = nonEmpty switch
            {
                [] => [],
                [var ax, .. var xs] => xs
            };

            Types.List<Base.Int> expected = [2, 3];

            Xunit.Assert.Equal(expected, x);

        }
    }
}
