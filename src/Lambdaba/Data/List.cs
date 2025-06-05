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
                return Maybe.Just(index);
            }
            index++;
        }
        return Maybe.Nothing<Int>();
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
        => ListToMaybe(Map(x => Fst(x), Filter(x => p(Snd(x)), Zip(Iterate((Int x) => x + 1, 0), list))));

    public static Maybe<A> ListToMaybe<A>(Types.List<A> list) =>
        list switch
        {
            [] => Maybe.Nothing<A>(),
            [var x, .. var _] => Maybe.Just(x),
            _ => throw new NotSupportedException()
        };

    public static Types.List<A> Filter<A>(Func<A, Bool> p, Types.List<A> list) =>
        list switch
        {
            [] => list,
            [var x, .. var xs] => [x, ..Filter(p, xs)],
            _ => throw new NotSupportedException()
        };

    public static Types.List<A> Iterate<A>(Func<A, A> f, A x)
    {
        // implement without using recursion
        Types.List<A> result = new Types.Empty<A>();
        while (true)
        {
            result = new Types.NonEmpty<A>(x, result);
            x = f(x);
        }
    }

    public static Types.List<A> Cycle<A>(Types.List<A> xs) =>
        xs + Cycle(xs);

    public static A Head<A>(Types.List<A> xs) => xs switch
    {
        [var x, .. _] => x,
        [] => throw new InvalidOperationException("empty list"),
        _ => throw new NotSupportedException()
    };

    public static Types.List<A> Tail<A>(Types.List<A> xs) => xs switch
    {
        [_, .. var rest] => rest,
        [] => throw new InvalidOperationException("empty list"),
        _ => throw new NotSupportedException()
    };

    public static Types.List<A> Reverse<A>(Types.List<A> xs)
    {
        Types.List<A> acc = [];
        foreach (var a in xs)
        {
            acc = new Types.NonEmpty<A>(a, acc);
        }
        return acc;
    }

    public static Bool Elem<A>(A x, Types.List<A> xs) =>
        xs switch
        {
            [] => new False(),
            [var y, .. var ys] => y!.Equals(x) ? new True() : Elem(x, ys),
            _ => throw new NotSupportedException()
        };

    public static Bool NotElem<A>(A x, Types.List<A> xs) => Not(Elem(x, xs));

    public static Bool Null<A>(Types.List<A> xs) => xs switch
    {
        [] => new True(),
        _ => new False()
    };

    public static A Last<A>(Types.List<A> xs) => xs switch
    {
        [] => throw new InvalidOperationException("empty list"),
        [var x] => x,
        [_, .. var rest] => Last(rest),
        _ => throw new NotSupportedException()
    };

    public static Types.List<A> Init<A>(Types.List<A> xs) => xs switch
    {
        [] => throw new InvalidOperationException("empty list"),
        [_,] => [],
        [var x, .. var rest] => [x, ..Init(rest)],
        _ => throw new NotSupportedException()
    };

    public static Types.List<A> Take<A>(Int n, Types.List<A> xs)
    {
        if ((int)n <= 0 || xs is [])
            return [];
        if (xs is [var x, .. var rest])
            return [x, ..Take(n - 1, rest)];
        throw new NotSupportedException();
    }

    public static Types.List<A> Drop<A>(Int n, Types.List<A> xs)
    {
        if ((int)n <= 0)
            return xs;
        return xs switch
        {
            [] => [],
            [_, .. var rest] => Drop(n - 1, rest),
            _ => throw new NotSupportedException()
        };
    }

    public static (Types.List<A>, Types.List<A>) SplitAt<A>(Int n, Types.List<A> xs) =>
        (Take(n, xs), Drop(n, xs));

    public static Types.List<A> TakeWhile<A>(Func<A, Bool> p, Types.List<A> xs) =>
        xs switch
        {
            [] => [],
            [var x, .. var rest] => p(x) switch
            {
                True => [x, ..TakeWhile(p, rest)],
                False => [],
                _ => throw new NotSupportedException()
            },
            _ => throw new NotSupportedException()
        };

    public static Types.List<A> DropWhile<A>(Func<A, Bool> p, Types.List<A> xs) =>
        xs switch
        {
            [] => [],
            [var x, .. var rest] => p(x) switch
            {
                True => DropWhile(p, rest),
                False => xs,
                _ => throw new NotSupportedException()
            },
            _ => throw new NotSupportedException()
        };

    public static (Types.List<A>, Types.List<A>) Span<A>(Func<A, Bool> p, Types.List<A> xs) =>
        xs switch
        {
            [] => ([], []),
            [var x, .. var rest] => p(x) switch
            {
                True =>
                    Span(p, rest) switch { var res => ([x, ..res.Item1], res.Item2) },
                False => ([], xs),
                _ => throw new NotSupportedException()
            },
            _ => throw new NotSupportedException()
        };

    public static (Types.List<A>, Types.List<A>) Break<A>(Func<A, Bool> p, Types.List<A> xs) =>
        Span(a => Not(p(a)), xs);

    public static Types.List<A> Repeat<A>(A x) => new Types.NonEmpty<A>(x, Repeat(x));

    public static Types.List<A> Replicate<A>(Int n, A x)
    {
        if ((int)n <= 0)
            return [];
        return new Types.NonEmpty<A>(x, Replicate(n - 1, x));
    }

    public static Types.List<C> ZipWith<A, B, C>(Func<A, Func<B, C>> f, Types.List<A> xs, Types.List<B> ys) =>
        (xs, ys) switch
        {
            ([], _) => [],
            (_, []) => [],
            ([var x, .. var xs1], [var y, .. var ys1]) => [f(x)(y), ..ZipWith(f, xs1, ys1)],
            _ => throw new NotSupportedException()
        };

    public static Types.List<(A, B, C)> Zip3<A, B, C>(Types.List<A> xs, Types.List<B> ys, Types.List<C> zs) =>
        (xs, ys, zs) switch
        {
            ([], _, _) => [],
            (_, [], _) => [],
            (_, _, []) => [],
            ([var x, .. var xs1], [var y, .. var ys1], [var z, .. var zs1]) => [(x, y, z), ..Zip3(xs1, ys1, zs1)],
            _ => throw new NotSupportedException()
        };

    public static Types.List<D> ZipWith3<A, B, C, D>(Func<A, Func<B, Func<C, D>>> f, Types.List<A> xs, Types.List<B> ys, Types.List<C> zs) =>
        (xs, ys, zs) switch
        {
            ([], _, _) => [],
            (_, [], _) => [],
            (_, _, []) => [],
            ([var x, .. var xs1], [var y, .. var ys1], [var z, .. var zs1]) => [f(x)(y)(z), ..ZipWith3(f, xs1, ys1, zs1)],
            _ => throw new NotSupportedException()
        };

    
}