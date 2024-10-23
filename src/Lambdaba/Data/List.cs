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
    
}