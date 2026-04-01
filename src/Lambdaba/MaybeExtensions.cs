using System;
using static Lambdaba.Types;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// C# 14 extension members for <see cref="Maybe{A}"/> — mirrors Haskell's Data.Maybe helpers.
/// </summary>
public static class MaybeExtensions
{
    /// <summary>Returns <c>true</c> if the value is <see cref="Just{A}"/>.</summary>
    extension<A>(Maybe<A> m)
    {
        public bool IsJust => m switch { Just<A> => true, _ => false };

        /// <summary>Returns <c>true</c> if the value is <see cref="Nothing"/>.</summary>
        public bool IsNothing => m switch { Nothing => true, _ => false };

        /// <summary>
        /// Extracts the value from <see cref="Just{A}"/>.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="Nothing"/>.
        /// </summary>
        public A FromJust => m switch
        {
            Just<A>(var x) => x,
            _ => throw new InvalidOperationException("Maybe.FromJust: Nothing")
        };

        /// <summary>Returns the value inside <see cref="Just{A}"/>, or <paramref name="defaultValue"/> if <see cref="Nothing"/>.</summary>
        public A FromMaybe(A defaultValue) => m switch
        {
            Just<A>(var x) => x,
            _ => defaultValue
        };

        /// <summary>
        /// Returns a singleton list for <see cref="Just{A}"/>, or an empty list for <see cref="Nothing"/>.
        /// Equivalent to Haskell's <c>maybeToList</c>.
        /// </summary>
        public List<A> MaybeToList => m switch
        {
            Just<A>(var x) => [x],
            _ => []
        };

        /// <summary>
        /// Applies <paramref name="f"/> to the contained value and returns the result,
        /// or returns <paramref name="defaultValue"/> for <see cref="Nothing"/>.
        /// Equivalent to Haskell's <c>maybe def f m</c>.
        /// </summary>
        public B Match<B>(B defaultValue, Func<A, B> f) => m switch
        {
            Just<A>(var x) => f(x),
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts a list to a <see cref="Maybe{A}"/>: <see cref="Nothing"/> for empty,
    /// <see cref="Just{A}"/> of the first element otherwise.
    /// Equivalent to Haskell's <c>listToMaybe</c>.
    /// </summary>
    extension<A>(List<A> xs)
    {
        public Maybe<A> ListToMaybe => xs switch
        {
            [] => global::Lambdaba.Maybe.MkNothing<A>(),
            [var x, ..] => global::Lambdaba.Maybe.MkJust(x),
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    /// Collects the <see cref="Just{A}"/> values from a list, discarding <see cref="Nothing"/>s.
    /// Equivalent to Haskell's <c>catMaybes</c>.
    /// </summary>
    extension<A>(List<Maybe<A>> ms)
    {
        public List<A> CatMaybes => ms switch
        {
            [] => [],
            [Just<A>(var x), .. var rest] => [x, .. rest.CatMaybes],
            [Nothing, .. var rest] => rest.CatMaybes,
            _ => throw new NotSupportedException()
        };
    }
}
