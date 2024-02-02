using static Lambdaba.Base;
using static Lambdaba.Data.Semigroup.Internal;

namespace Lambdaba.Data;

public interface Foldable<T> 
    where T : Foldable<T>
{
    /// <summary>
    /// The FoldMap function is a method that maps each element of the data structure to a monoid, and then combines the results using the monoid operation. 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="M"></typeparam>
    /// <param name="f"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    static virtual M FoldMap<A, M>(Func<A, M> f, Data<T, A> t)
        where M : Monoid<M> => T.FoldR((a, m) => f(a) + m, M.Mempty(), t);

    static virtual B FoldR<A, B>(Func<A, B, B> f, B b, Data<T, A> t) =>
        T.FoldMap(a => new Endo<B>(b => f(a, b)), t).Apply(b);

    /// <summary>
    /// Fold is a method that reduces a data structure to a single summary value by using a Monoid. 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="M"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    static virtual M Fold<A, M>(Data<T, M> t)
         where M : Monoid<M> =>
        T.FoldMap(Id<M>(), t);   
        
}