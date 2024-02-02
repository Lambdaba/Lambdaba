using Lambdaba.Data;
using static Lambdaba.Base;

namespace Lambdaba;

public static class DataExtensions
{
    public static Data<T, B> Select<T, A, B>(this Data<T, A> t, Func<A, B> f)
        where T : Functor<T> => T.FMap(f, t);

    public static Data<T, C> SelectMany<T, A, B, C>(this Data<T, A> t, Func<A, Data<T, B>> f, Func<A, B, C> project)
        where T : Monad<T> => T.SelectMany(t, f, project);

    public static Data<M, B> Bind<M, A, B>(this Data<M, A> a, Func<A, Data<M, B>> f)
        where M : Monad<M> =>
            M.Bind(a, f);
    
    public static A MConcat<F, A>(this A a, Data<F, A> @as) 
            where F : Foldable<F>
            where A : Monoid<A> =>
                A.MConcat(@as);
                
    public static A MAppend<A>(this A a, A b) 
        where A : Monoid<A> =>
            A.Mappend(a, b);
}
