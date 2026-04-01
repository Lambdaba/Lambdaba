using static Lambdaba.Base;
using System;
using System.Collections.Generic;

namespace Lambdaba.Data;

public static class List
{
    public static Maybe<Int> ElemIndex<A>(A a, Types.List<A> list)
    {
        Int index = 0;
        foreach (var item in list)
        {
            if (item != null && item.Equals(a))
            {
                return Maybe.MkJust(index);
            }
            index++;
        }
        return Maybe.MkNothing<Int>();
    }

    public static Int Length<A>(Types.List<A> list)
    {
        return lenAcc(list, 0);
        static Int lenAcc(Types.List<A> list, Int acc) =>
            list switch
            {
                [] => acc,
                [var _, .. var tail] => lenAcc(tail, acc + 1),
                _ => throw new System.NotSupportedException()
            };
    }

    public static Types.List<(A, B)> Zip<A, B>(Types.List<A> a, Types.List<B> b) =>
    (a, b) switch
    {
        ([], _) => [],
        (_, []) => [],
        ([var x, ..var xs], [var y, ..var ys]) => [(x, y)] + Zip(xs, ys),
        _ => throw new NotSupportedException()
    };

    public static Maybe<Int> FindIndex<A>(Func<A, Bool> p, Types.List<A> list)
    {
        Int index = 0;
        foreach (var item in list)
        {
            if (p(item)) return Maybe.MkJust(index);
            index++;
        }
        return Maybe.MkNothing<Int>();
    }

    public static Maybe<A> ListToMaybe<A>(Types.List<A> list) =>
        list switch
        {
            [] => Maybe.MkNothing<A>(),
            [var x, .. var _] => Maybe.MkJust(x),
            _ => throw new NotSupportedException()
        };

    public static Types.List<A> Filter<A>(Func<A, Bool> p, Types.List<A> list) =>
        list switch
        {
            [] => list,
            [var x, .. var xs] when p(x) => [x, ..Filter(p, xs)],
            [var _, .. var xs] => Filter(p, xs),
            _ => throw new NotSupportedException()
        };

    /// <summary>
    /// Returns a finite prefix of the infinite repetition of <paramref name="f"/> applied to <paramref name="x"/>.
    /// NOTE: In Haskell, <c>iterate</c> is lazy. This eager C# version requires a <paramref name="count"/> bound.
    /// Use <see cref="IterateInfinite{A}"/> for an <see cref="IEnumerable{A}"/> variant.
    /// </summary>
    public static Types.List<A> Iterate<A>(Func<A, A> f, A x, int count)
    {
        Types.List<A> result = new Types.Empty<A>();
        for (int i = count - 1; i >= 0; i--)
        {
            A val = x;
            for (int j = 0; j < i; j++) val = f(val);
            result = new Types.NonEmpty<A>(val, result);
        }
        return result;
    }

    /// <summary>
    /// Lazy infinite sequence of repeated applications of <paramref name="f"/> to <paramref name="x"/>.
    /// Equivalent to Haskell's <c>iterate f x</c>.
    /// </summary>
    public static IEnumerable<A> IterateInfinite<A>(Func<A, A> f, A x)
    {
        while (true)
        {
            yield return x;
            x = f(x);
        }
    }

    /// <summary>
    /// Returns a finite repetition of <paramref name="xs"/> repeated <paramref name="count"/> times.
    /// NOTE: Haskell's <c>cycle</c> is lazy/infinite. This C# version requires a count bound.
    /// </summary>
    public static Types.List<A> Cycle<A>(Types.List<A> xs, int count)
    {
        Types.List<A> result = new Types.Empty<A>();
        for (int i = 0; i < count; i++) result = result + xs;
        return result;
    }
    
}