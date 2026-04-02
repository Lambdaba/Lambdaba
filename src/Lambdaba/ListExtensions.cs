using System;
using static Lambdaba.Base;
using static Lambdaba.Types;
using ListOps = Lambdaba.Data.List;

namespace Lambdaba;

/// <summary>
/// C# 14 extension members for <see cref="List{A}"/> — mirrors the Haskell Prelude / Data.List API.
/// </summary>
public static class ListExtensions
{
    extension<A>(List<A> xs)
    {
        // ── Basic accessors ──────────────────────────────────────────────

        /// <summary><c>True</c> if the list is empty. O(1). Equivalent to Haskell's <c>null</c>.</summary>
        public bool Null => xs is Empty<A>;

        /// <summary>
        /// The first element. Throws on empty list.
        /// Equivalent to Haskell's partial <c>head</c>.
        /// </summary>
        public A Head => xs switch
        {
            NonEmpty<A>(var h, _) => h,
            _ => throw new InvalidOperationException("List.Head: empty list")
        };

        /// <summary>
        /// All elements after the first. Throws on empty list.
        /// Equivalent to Haskell's partial <c>tail</c>.
        /// </summary>
        public List<A> Tail => xs switch
        {
            NonEmpty<A>(_, var t) => t,
            _ => throw new InvalidOperationException("List.Tail: empty list")
        };

        /// <summary>
        /// The last element. O(n). Throws on empty list.
        /// Equivalent to Haskell's partial <c>last</c>.
        /// </summary>
        public A Last => xs switch
        {
            NonEmpty<A>(var h, var t) => t is Empty<A> ? h : t.Last,
            _ => throw new InvalidOperationException("List.Last: empty list")
        };

        /// <summary>
        /// All elements except the last. O(n). Throws on empty list.
        /// Equivalent to Haskell's partial <c>init</c>.
        /// </summary>
        public List<A> Init => xs switch
        {
            NonEmpty<A>(_, Empty<A>) => [],
            NonEmpty<A>(var h, var t) => new NonEmpty<A>(h, t.Init),
            _ => throw new InvalidOperationException("List.Init: empty list")
        };

        // ── Transformations ──────────────────────────────────────────────

        /// <summary>Apply <paramref name="f"/> to every element. Equivalent to Haskell's <c>map</c>.</summary>
        public List<B> Map<B>(Func<A, B> f) => Base.Map(f, xs);

        /// <summary>Retain elements satisfying <paramref name="p"/>. Equivalent to Haskell's <c>filter</c>.</summary>
        public List<A> Filter(Func<A, Bool> p) => ListOps.Filter(p, xs);

        /// <summary>
        /// Right-associative fold. Equivalent to Haskell's <c>foldr</c>.
        /// </summary>
        public B FoldR<B>(Func<A, B, B> f, B z) => Base.FoldR(f, z, xs);

        /// <summary>
        /// Reverse the list. O(n).
        /// </summary>
        public List<A> Reverse =>
            Base.FoldR<A, List<A>>((x, acc) => acc + [x], [], xs);

        // ── Sub-lists ────────────────────────────────────────────────────

        /// <summary>Take the first <paramref name="n"/> elements. Equivalent to Haskell's <c>take</c>.</summary>
        public List<A> Take(int n) => (n, xs) switch
        {
            (<= 0, _) => [],
            (_, Empty<A>) => [],
            (_, NonEmpty<A>(var h, var t)) => new NonEmpty<A>(h, t.Take(n - 1)),
            _ => throw new NotSupportedException()
        };

        /// <summary>Drop the first <paramref name="n"/> elements. Equivalent to Haskell's <c>drop</c>.</summary>
        public List<A> Drop(int n) => (n, xs) switch
        {
            (<= 0, _) => xs,
            (_, Empty<A>) => [],
            (_, NonEmpty<A>(_, var t)) => t.Drop(n - 1),
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// Split into the longest prefix satisfying <paramref name="p"/> and the rest.
        /// Returns <c>(Prefix, Rest)</c>. Equivalent to Haskell's <c>span</c>.
        /// </summary>
        public (List<A>, List<A>) Span(Func<A, Bool> p)
        {
            if (xs is NonEmpty<A> ne && (bool)p(ne.Head))
            {
                var (prefix, rest) = ne.Tail.Span(p);
                return (new NonEmpty<A>(ne.Head, prefix), rest);
            }
            return ([], xs);
        }

        /// <summary>
        /// Split at position <paramref name="n"/>. Equivalent to Haskell's <c>splitAt</c>.
        /// </summary>
        public (List<A>, List<A>) SplitAt(int n) => (xs.Take(n), xs.Drop(n));

        // ── Searching ────────────────────────────────────────────────────

        /// <summary>
        /// The index of the first occurrence of <paramref name="a"/>, or <see cref="Nothing"/>.
        /// Equivalent to Haskell's <c>elemIndex</c>.
        /// </summary>
        public Maybe<Int> ElemIndex(A a) => ListOps.ElemIndex(a, xs);

        /// <summary>
        /// The index of the first element satisfying <paramref name="p"/>, or <see cref="Nothing"/>.
        /// Equivalent to Haskell's <c>findIndex</c>.
        /// </summary>
        public Maybe<Int> FindIndex(Func<A, Bool> p) => ListOps.FindIndex(p, xs);

        /// <summary>
        /// Whether any element equals <paramref name="a"/>. Equivalent to Haskell's <c>elem</c>.
        /// </summary>
        public bool Elem(A a) => xs.ElemIndex(a).IsJust;

        /// <summary>Map then concatenate. Equivalent to Haskell's <c>concatMap</c>.</summary>
        public List<B> ConcatMap<B>(Func<A, List<B>> f) =>
            Base.FoldR<A, List<B>>((x, acc) => f(x) + acc, [], xs);

        // ── Zipping ──────────────────────────────────────────────────────

        /// <summary>Zip two lists into pairs. Equivalent to Haskell's <c>zip</c>.</summary>
        public List<(A, B)> Zip<B>(List<B> ys) => ListOps.Zip(xs, ys);

        /// <summary>Zip two lists, applying <paramref name="f"/> to each pair. Equivalent to Haskell's <c>zipWith</c>.</summary>
        public List<C> ZipWith<B, C>(Func<A, B, C> f, List<B> ys) => (xs, ys) switch
        {
            (Empty<A>, _) => [],
            (_, Empty<B>) => [],
            (NonEmpty<A>(var x, var xs2), NonEmpty<B>(var y, var ys2)) =>
                new NonEmpty<C>(f(x, y), xs2.ZipWith(f, ys2)),
            _ => throw new NotSupportedException()
        };

        // ── Iteration helpers ────────────────────────────────────────────

        /// <summary>
        /// Repeat this list <paramref name="count"/> times. Equivalent to a bounded Haskell <c>cycle</c>.
        /// </summary>
        public List<A> Cycle(int count) => ListOps.Cycle(xs, count);
    }

    extension<A>(List<A> xs) where A : Ord<A>
    {
        /// <summary>Maximum element. O(n). Throws on empty list.</summary>
        public A Maximum => xs switch
        {
            Empty<A> => throw new InvalidOperationException("List.Maximum: empty list"),
            NonEmpty<A>(var h, Empty<A>) => h,
            NonEmpty<A>(var h, var t) => A.Max(h, t.Maximum),
            _ => throw new NotSupportedException()
        };

        /// <summary>Minimum element. O(n). Throws on empty list.</summary>
        public A Minimum => xs switch
        {
            Empty<A> => throw new InvalidOperationException("List.Minimum: empty list"),
            NonEmpty<A>(var h, Empty<A>) => h,
            NonEmpty<A>(var h, var t) => A.Min(h, t.Minimum),
            _ => throw new NotSupportedException()
        };
    }

    extension<A>(List<A> xs) where A : Num<A>
    {
        /// <summary>Sum of elements. Equivalent to Haskell's <c>sum</c>.</summary>
        public A Sum => Base.FoldR<A, A>((x, acc) => A.Add(x, acc), A.FromInteger(0), xs);

        /// <summary>Product of elements. Equivalent to Haskell's <c>product</c>.</summary>
        public A Product => Base.FoldR<A, A>((x, acc) => x * acc, A.FromInteger(1), xs);
    }

    extension<A>(List<List<A>> xss)
    {
        /// <summary>
        /// Concatenate a list of lists. Equivalent to Haskell's <c>concat</c>.
        /// </summary>
        public List<A> Concat => Base.FoldR<List<A>, List<A>>((x, acc) => x + acc, [], xss);
    }
}
