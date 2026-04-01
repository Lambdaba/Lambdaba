using System;
using Lambdaba.Data;
using static Lambdaba.Base;

namespace Lambdaba;

public static class DataExtensions
{
    extension<T, A, B>(Data<T, A> t) where T : Functor<T>
    {
        public Data<T, B> Select(Func<A, B> f) => T.FMap(f, t);
    }

    extension<T, A, B, C>(Data<T, A> t) where T : Monad<T>
    {
        public Data<T, C> SelectMany(Func<A, Data<T, B>> f, Func<A, B, C> project) =>
            T.SelectMany(t, f, project);
    }

    extension<M, A, B>(Data<M, A> a) where M : Monad<M>
    {
        public Data<M, B> Bind(Func<A, Data<M, B>> f) => M.Bind(a, f);
    }

    extension<F, A>(A a) where F : Foldable<F> where A : Monoid<A>
    {
        public A MConcat(Data<F, A> @as) => A.MConcat(@as);
    }

    extension<A>(A a) where A : Monoid<A>
    {
        public A MAppend(A b) => A.Mappend(a, b);
    }

    extension<T, A>(Data<T, A> t) where T : Monad<T>, Alternative<T>
    {
        public Data<T, A> Where(Func<A, bool> predicate) =>
            T.Bind(t, a => predicate(a) ? T.Pure<A>(a) : T.Empty<A>());
    }
}
