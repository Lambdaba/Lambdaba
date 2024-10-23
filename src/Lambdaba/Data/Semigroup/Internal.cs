using System;
using static Lambdaba.Base;

namespace Lambdaba.Data.Semigroup;

public static class Internal
{
    public class Endo<A>(Func<A, A> function) : Monoid<Endo<A>>
    {
        private readonly Func<A, A> _function = function;

        public A Apply(A value)
        {
            return _function(value);
        }

        public static Endo<A> Combine(Endo<A> a, Endo<A> b) => throw new NotImplementedException();
        public static Endo<A> Mempty() => throw new NotImplementedException();

        public static implicit operator Endo<A>(Func<A, A> function)
        {
            return new Endo<A>(function);
        }

        public static Endo<A> operator *(Endo<A> a, Endo<A> b)
        {
            return new Endo<A>(x => a._function(b._function(x)));
        }
    }
}
