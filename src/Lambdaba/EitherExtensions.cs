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
        /// <summary>
        /// Extracts the <see cref="Left{L}"/> value.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="Right{R}"/>.
        /// </summary>
        public L FromLeft => e.TryGetValue(out Left<L>? l)
            ? l!.Value
            : throw new InvalidOperationException("Either.FromLeft: Right");

        /// <summary>
        /// Extracts the <see cref="Right{R}"/> value.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="Left{L}"/>.
        /// </summary>
        public R FromRight => e.TryGetValue(out Right<R>? r)
            ? r!.Value
            : throw new InvalidOperationException("Either.FromRight: Left");
    }

    /// <summary>
    /// Extracts all <see cref="Left{L}"/> values from a list.
    /// Equivalent to Haskell's <c>lefts</c>.
    /// </summary>
    extension<L, R>(List<Either<L, R>> xs)
    {
        public List<L> Lefts => xs switch
        {
            [] => [],
            [var head, .. var rest] when head.TryGetValue(out Left<L>? l) => [l!.Value, .. rest.Lefts],
            [_, .. var rest] => rest.Lefts,
        };

        /// <summary>
        /// Extracts all <see cref="Right{R}"/> values from a list.
        /// Equivalent to Haskell's <c>rights</c>.
        /// </summary>
        public List<R> Rights => xs switch
        {
            [] => [],
            [var head, .. var rest] when head.TryGetValue(out Right<R>? r) => [r!.Value, .. rest.Rights],
            [_, .. var rest] => rest.Rights,
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
                    if (item.TryGetValue(out Left<L>? l))
                        lefts = [.. lefts, l!.Value];
                    else if (item.TryGetValue(out Right<R>? r))
                        rights = [.. rights, r!.Value];
                }
                return (lefts, rights);
            }
        }
    }
}
