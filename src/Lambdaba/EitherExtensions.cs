using System;
using static Lambdaba.Types;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// C# 14 extension members for <see cref="Either{L,R}"/> — mirrors Haskell's Data.Either helpers.
/// </summary>
public static class EitherExtensions
{
    extension<L, R>(Either<L, R> e)
    {
        /// <summary>Returns <c>true</c> if the value is <see cref="Left{L,R}"/>.</summary>
        public bool IsLeft => e switch
        {
            Left<L, R> => true,
            _ => false
        };

        /// <summary>Returns <c>true</c> if the value is <see cref="Right{L,R}"/>.</summary>
        public bool IsRight => e switch
        {
            Right<L, R> => true,
            _ => false
        };

        /// <summary>
        /// Extracts the <see cref="Left{L,R}"/> value.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="Right{L,R}"/>.
        /// </summary>
        public L FromLeft => e switch
        {
            Left<L, R>(var l) => l,
            _ => throw new InvalidOperationException("Either.FromLeft: Right")
        };

        /// <summary>
        /// Extracts the <see cref="Right{L,R}"/> value.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="Left{L,R}"/>.
        /// </summary>
        public R FromRight => e switch
        {
            Right<L, R>(var r) => r,
            _ => throw new InvalidOperationException("Either.FromRight: Left")
        };

        /// <summary>
        /// Case analysis: apply <paramref name="onLeft"/> or <paramref name="onRight"/> depending on the variant.
        /// Equivalent to Haskell's <c>either f g e</c>.
        /// </summary>
        public T Match<T>(Func<L, T> onLeft, Func<R, T> onRight) => e switch
        {
            Left<L, R>(var l) => onLeft(l),
            Right<L, R>(var r) => onRight(r),
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    /// Extracts all <see cref="Left{L,R}"/> values from a list.
    /// Equivalent to Haskell's <c>lefts</c>.
    /// </summary>
    extension<L, R>(List<Either<L, R>> xs)
    {
        public List<L> Lefts => xs switch
        {
            [] => [],
            [Left<L, R>(var l), .. var rest] => [l, .. rest.Lefts],
            [Right<L, R>, .. var rest] => rest.Lefts,
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// Extracts all <see cref="Right{L,R}"/> values from a list.
        /// Equivalent to Haskell's <c>rights</c>.
        /// </summary>
        public List<R> Rights => xs switch
        {
            [] => [],
            [Right<L, R>(var r), .. var rest] => [r, .. rest.Rights],
            [Left<L, R>, .. var rest] => rest.Rights,
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// Partitions a list of <see cref="Either{L,R}"/> into lefts and rights.
        /// Equivalent to Haskell's <c>partitionEithers</c>.
        /// </summary>
        public (List<L> Lefts, List<R> Rights) PartitionEithers
        {
            get
            {
                List<L> lefts = [];
                List<R> rights = [];
                foreach (var item in xs)
                {
                    if (item is Left<L, R>(var l))
                        lefts = [.. lefts, l];
                    else if (item is Right<L, R>(var r))
                        rights = [.. rights, r];
                }
                return (lefts, rights);
            }
        }
    }
}
