using System;
using Lambdaba.Data;
using static Lambdaba.Base;

namespace Lambdaba;

public static class DataExtensions
{
    // ── Functor / LINQ ───────────────────────────────────────────────────

    extension<T, A, B>(Data<T, A> t) where T : Functor<T>
    {
        public Data<T, B> Select(Func<A, B> f) => T.FMap(f, t);
    }

    // ── Monad / LINQ ─────────────────────────────────────────────────────

    extension<T, A, B, C>(Data<T, A> t) where T : Monad<T>
    {
        public Data<T, C> SelectMany(Func<A, Data<T, B>> f, Func<A, B, C> project) =>
            T.SelectMany(t, f, project);
    }

    extension<M, A, B>(Data<M, A> a) where M : Monad<M>
    {
        public Data<M, B> Bind(Func<A, Data<M, B>> f) => M.Bind(a, f);
    }

    // ── Join (flatten one monad layer) ───────────────────────────────────

    extension<M, A>(Data<M, Data<M, A>> mma) where M : Monad<M>
    {
        /// <summary>Remove one level of monadic structure. Equivalent to Haskell's <c>join</c>.</summary>
        public Data<M, A> Join() => M.Bind(mma, x => x);
    }

    // ── Applicative ──────────────────────────────────────────────────────

    extension<M, A, B>(Data<M, Func<A, B>> mf) where M : Monad<M>
    {
        /// <summary>Apply a wrapped function to a wrapped value. Equivalent to Haskell's <c><*></c>.</summary>
        public Data<M, B> Ap(Data<M, A> ma) => M.Apply(mf, ma);
    }

    // ── LiftM2 ───────────────────────────────────────────────────────────

    extension<M, A>(Data<M, A> ma) where M : Monad<M>
    {
        /// <summary>
        /// Lift a binary function into the monad.
        /// Equivalent to Haskell's <c>liftM2 f ma mb</c>.
        /// </summary>
        public Data<M, C> LiftM2<B, C>(Func<A, B, C> f, Data<M, B> mb) =>
            M.Bind(ma, a => M.FMap(b => f(a, b), mb));
    }

    // ── Sequence ─────────────────────────────────────────────────────────

    extension<M, A>(Types.List<Data<M, A>> mas) where M : Monad<M>
    {
        /// <summary>
        /// Evaluate each action left-to-right and collect results.
        /// Equivalent to Haskell's <c>sequence</c>.
        /// </summary>
        public Data<M, Types.List<A>> Sequence() => Base.Sequence<M, A>(mas);

        /// <summary>
        /// Evaluate each action left-to-right, discarding results.
        /// Equivalent to Haskell's <c>sequence_</c>.
        /// </summary>
        public Data<M, Unit> Sequence_() => Base.Sequence_<M, A>(mas);
    }

    // ── MapM ─────────────────────────────────────────────────────────────

    extension<M, A>(Types.List<A> xs) where M : Monad<M>
    {
        /// <summary>
        /// Map a monadic action over the list and collect results.
        /// Equivalent to Haskell's <c>mapM</c>.
        /// </summary>
        public Data<M, Types.List<B>> MapM<B>(Func<A, Data<M, B>> f) =>
            Base.MapM<M, A, B>(f, xs);
    }

    // ── When ─────────────────────────────────────────────────────────────

    extension<F>(Data<F, Unit> m) where F : Applicative<F>
    {
        /// <summary>
        /// Perform <paramref name="m"/> only when <paramref name="condition"/> is <see cref="True"/>.
        /// Equivalent to Haskell's <c>when condition m</c>.
        /// </summary>
        public Data<F, Unit> When(Bool condition) => condition switch
        {
            True  => m,
            False => F.Pure(new Unit()),
            _     => throw new NotSupportedException()
        };
    }

    // ── Monoid helpers ───────────────────────────────────────────────────

    extension<F, A>(A a) where F : Foldable<F> where A : Monoid<A>
    {
        public A MConcat(Data<F, A> @as) => A.MConcat(@as);
    }

    extension<A>(A a) where A : Monoid<A>
    {
        public A MAppend(A b) => A.Mappend(a, b);
    }

    // ── Alternative / MonadPlus ──────────────────────────────────────────

    extension<T, A>(Data<T, A> t) where T : Monad<T>, Alternative<T>
    {
        public Data<T, A> Where(Func<A, bool> predicate) =>
            T.Bind(t, a => predicate(a) ? T.Pure<A>(a) : T.Empty<A>());
    }
}
