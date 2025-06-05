using System;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// Represents a value of one of two possible types (a disjoint union).
/// </summary>
/// <typeparam name="L">Type of the <c>Left</c> value.</typeparam>
public abstract record Either<L> :
    Monad<Either<L>>,
    Data<Either<L>>
{
    public static Data<Either<L>, B> FMap<A, B>(Func<A, B> f, Data<Either<L>, A> t) =>
        t switch
        {
            Left<L, A>(var l) => new Left<L, B>(l),
            Right<L, A>(var r) => new Right<L, B>(f(r)),
            _ => throw new NotSupportedException()
        };

    public static Data<Either<L>, B> Bind<A, B>(Data<Either<L>, A> t, Func<A, Data<Either<L>, B>> f) =>
        t switch
        {
            Left<L, A>(var l) => new Left<L, B>(l),
            Right<L, A>(var r) => f(r),
            _ => throw new NotSupportedException()
        };

    public static Data<Either<L>, B> Apply<A, B>(Data<Either<L>, Func<A, B>> f, Data<Either<L>, A> t) =>
        f switch
        {
            Left<L, Func<A, B>>(var l) => new Left<L, B>(l),
            Right<L, Func<A, B>>(var g) => t switch
            {
                Left<L, A>(var l2) => new Left<L, B>(l2),
                Right<L, A>(var x) => new Right<L, B>(g(x)),
                _ => throw new NotSupportedException()
            },
            _ => throw new NotSupportedException()
        };

    public static Data<Either<L>, A> Pure<A>(A a) => new Right<L, A>(a);

    public static Data<Either<L>, C> SelectMany<A, B, C>(Data<Either<L>, A> t,
        Func<A, Data<Either<L>, B>> f, Func<A, B, C> project) =>
        t switch
        {
            Left<L, A>(var l) => new Left<L, C>(l),
            Right<L, A>(var a) => Bind(f(a), b => new Right<L, C>(project(a, b))),
            _ => throw new NotSupportedException(),
        };

    public static T Match<A, T>(Data<Either<L>, A> t, Func<L, T> onLeft, Func<A, T> onRight) =>
        t switch
        {
            Left<L, A>(var l) => onLeft(l),
            Right<L, A>(var r) => onRight(r),
            _ => throw new NotSupportedException(),
        };

    /// <summary>
    /// Haskell style either function.
    /// </summary>
    public static T Either<T, A>(Func<L, T> leftF, Func<A, T> rightF, Data<Either<L>, A> e) =>
        Match(e, leftF, rightF);

    public static Bool IsLeft<A>(Data<Either<L>, A> t) =>
        t switch
        {
            Left<L, A> => new True(),
            Right<L, A> => new False(),
            _ => throw new NotSupportedException(),
        };

    public static Bool IsRight<A>(Data<Either<L>, A> t) =>
        t switch
        {
            Right<L, A> => new True(),
            Left<L, A> => new False(),
            _ => throw new NotSupportedException(),
        };

    public static Data<Either<L2>, A> MapLeft<L2, A>(Func<L, L2> f, Data<Either<L>, A> t) =>
        t switch
        {
            Left<L, A>(var l) => new Left<L2, A>(f(l)),
            Right<L, A>(var r) => new Right<L2, A>(r),
            _ => throw new NotSupportedException(),
        };

    public static Data<Either<L2>, B> Bimap<L2, A, B>(Func<L, L2> fLeft, Func<A, B> fRight, Data<Either<L>, A> t) =>
        t switch
        {
            Left<L, A>(var l) => new Left<L2, B>(fLeft(l)),
            Right<L, A>(var r) => new Right<L2, B>(fRight(r)),
            _ => throw new NotSupportedException(),
        };

    public static Data<Either<A>, L> Swap<A>(Data<Either<L>, A> t) =>
        t switch
        {
            Left<L, A>(var l) => new Right<A, L>(l),
            Right<L, A>(var r) => new Left<A, L>(r),
            _ => throw new NotSupportedException(),
        };

    public static Types.List<L> Lefts<R>(Types.List<Either<L, R>> xs) =>
        xs switch
        {
            [] => [],
            [Left<L, R>(var l), ..var tail] => [l, ..Lefts(tail)],
            [Right<L, R> _, ..var tail] => Lefts(tail),
            _ => throw new NotSupportedException(),
        };

    public static Types.List<R> Rights<R>(Types.List<Either<L, R>> xs) =>
        xs switch
        {
            [] => [],
            [Right<L, R>(var r), ..var tail] => [r, ..Rights(tail)],
            [Left<L, R> _, ..var tail] => Rights(tail),
            _ => throw new NotSupportedException(),
        };

    public static (Types.List<L> Lefts, Types.List<R> Rights) Partition<R>(Types.List<Either<L, R>> xs)
    {
        if (xs is [])
            return ([], []);

        if (xs is [Left<L, R>(var l), ..var tail1])
        {
            var (ls, rs) = Partition(tail1);
            return ([l, ..ls], rs);
        }

        if (xs is [Right<L, R>(var r), ..var tail2])
        {
            var (ls, rs) = Partition(tail2);
            return (ls, [r, ..rs]);
        }

        throw new NotSupportedException();
    }
}

public abstract record Either<L, R> : Either<L>, Data<Either<L>, R>;

public sealed record Left<L, R>(L Value) : Either<L, R>;

public sealed record Right<L, R>(R Value) : Either<L, R>;
