using static Lambdaba.Base;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Lambdaba;


public static class Prelude
{
    /// <summary>
    /// Derived by types whose values can be compare for equality and inequality. You need
    /// to override at least 1 of the operators, otherwise it will recurse infinitely
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public interface Eq<in A>
        where A : Eq<A>
    {
        static virtual bool operator ==(A x, A y) => Not(x != y);

        static virtual bool operator !=(A x, A y) => Not(x == y);
    }

    /// <summary>
    /// Derived by types that are instances of Eq, but in addition whose values
    /// are totally (linearly) ordered
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public interface Ord<A> : Eq<A>
        where A : Ord<A>
    {
        static virtual Ordering Compare(A x, A y) =>
            (x == y) switch
            {
                true => new EQ(),
                false => (x <= y) switch
                {
                    true => new LT(),
                    false => new GT()
                }
            };

        static virtual bool operator <=(A x, A y) => A.Compare(x, y) != new GT();

        static virtual bool operator <(A x, A y) => A.Compare(x, y) == new LT();

        static virtual bool operator >=(A x, A y) => A.Compare(x, y) != new LT();

        static virtual bool operator >(A x, A y) => A.Compare(x, y) == new GT();

        static virtual A Max(A x, A y) => x <= y ? y : x;

        static virtual A Min(A x, A y) => x <= y ? x : y;

        
    }

    public static A Fst<A, B>((A, B) pair) =>
        pair switch
        {
            (var x, _) => x
        };

    public static B Snd<A, B>((A, B) pair) =>
        pair switch
        {
            (_, var y) => y
        };


}
