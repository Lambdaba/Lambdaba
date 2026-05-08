using System;
using System.Runtime.CompilerServices;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// Represents a value of one of two possible types (a disjoint union).
/// </summary>
/// <typeparam name="L">Type of the <c>Left</c> value.</typeparam>
public abstract partial class Either<L> :
    Monad<Either<L>>,
    Data<Either<L>>
{
    public static Data<Either<L>, B> FMap<A, B>(Func<A, B> f, Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<L, B>(new Left<L>(l)),
            Right<A>(var r) => new Either<L, B>(new Right<B>(f(r))),
        };

    public static Data<Either<L>, B> Bind<A, B>(Data<Either<L>, A> t, Func<A, Data<Either<L>, B>> f) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<L, B>(new Left<L>(l)),
            Right<A>(var r) => f(r),
        };

    public static Data<Either<L>, B> Apply<A, B>(Data<Either<L>, Func<A, B>> f, Data<Either<L>, A> t) =>
        ((Either<L, Func<A, B>>)f) switch
        {
            Left<L>(var l) => new Either<L, B>(new Left<L>(l)),
            Right<Func<A, B>>(var g) =>
                ((Either<L, A>)t) switch
                {
                    Left<L>(var l2) => new Either<L, B>(new Left<L>(l2)),
                    Right<A>(var x) => new Either<L, B>(new Right<B>(g(x))),
                },
        };

    public static Data<Either<L>, A> Pure<A>(A a) => new Either<L, A>(new Right<A>(a));

    public static Data<Either<L>, C> SelectMany<A, B, C>(Data<Either<L>, A> t,
        Func<A, Data<Either<L>, B>> f, Func<A, B, C> project) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<L, C>(new Left<L>(l)),
            Right<A>(var a) => Bind(f(a), b => (Data<Either<L>, C>)new Either<L, C>(new Right<C>(project(a, b)))),
        };

    public static T Match<A, T>(Data<Either<L>, A> t, Func<L, T> onLeft, Func<A, T> onRight) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => onLeft(l),
            Right<A>(var r) => onRight(r),
        };

    public static Bool IsLeft<A>(Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Left<L> => new True(),
            Right<A> => new False(),
        };

    public static Bool IsRight<A>(Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Right<A> => new True(),
            Left<L> => new False(),
        };

    public static Data<Either<L2>, A> MapLeft<L2, A>(Func<L, L2> f, Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<L2, A>(new Left<L2>(f(l))),
            Right<A>(var r) => new Either<L2, A>(new Right<A>(r)),
        };

    public static Data<Either<L2>, B> Bimap<L2, A, B>(Func<L, L2> fLeft, Func<A, B> fRight, Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<L2, B>(new Left<L2>(fLeft(l))),
            Right<A>(var r) => new Either<L2, B>(new Right<B>(fRight(r))),
        };

    public static Data<Either<A>, L> Swap<A>(Data<Either<L>, A> t) =>
        ((Either<L, A>)t) switch
        {
            Left<L>(var l) => new Either<A, L>(new Right<L>(l)),
            Right<A>(var r) => new Either<A, L>(new Left<A>(r)),
        };

    public static Types.List<L> Lefts<R>(Types.List<Either<L, R>> xs) =>
        xs switch
        {
            [] => [],
            [var head, .. var tail] when head.TryGetValue(out Left<L>? l) => [l!.Value, .. Lefts(tail)],
            [_, .. var tail] => Lefts(tail),
        };

    public static Types.List<R> Rights<R>(Types.List<Either<L, R>> xs) =>
        xs switch
        {
            [] => [],
            [var head, .. var tail] when head.TryGetValue(out Right<R>? r) => [r!.Value, .. Rights(tail)],
            [_, .. var tail] => Rights(tail),
        };

    public static (Types.List<L> Lefts, Types.List<R> Rights) Partition<R>(Types.List<Either<L, R>> xs)
    {
        if (xs is [])
            return ([], []);

        var head = xs[0];
        Types.List<Either<L, R>> tail = [.. xs[1..]];

        if (head.TryGetValue(out Left<L>? l))
        {
            var (ls, rs) = Partition(tail);
            return ([l!.Value, .. ls], rs);
        }

        if (head.TryGetValue(out Right<R>? r))
        {
            var (ls, rs) = Partition(tail);
            return (ls, [r!.Value, .. rs]);
        }

        return Partition(tail);
    }
}

[Union]
public partial class Either<L, R> : Either<L>, Data<Either<L>, R>, IUnion
{
    public Either(Left<L> value) { Value = value; }
    public Either(Right<R> value) { Value = value; }

    /// <summary>The wrapped discriminated-union case; accessed by the <c>[Union]</c> switch rewrite.</summary>
    public object? Value { get => field; private set; }

    public static implicit operator Either<L, R>(Left<L> value) => new(value);
    public static implicit operator Either<L, R>(Right<R> value) => new(value);
}

public sealed record Left<L>(L Value);

public sealed record Right<R>(R Value);
