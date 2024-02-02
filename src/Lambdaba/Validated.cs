using System;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// The Validated type constructor
/// </summary>
public abstract record Validated :
    Monad<Validated>,
    Data<Validated>
{
    public static Data<Validated, B> FMap<A, B>(Func<A, B> f, Data<Validated, A> t) =>
        Bind(t, x => new Valid<B>(f(x)));

    public static Data<Validated, A> Pure<A>(A a) =>
        new Valid<A>(a);

    public static Data<Validated, B> Apply<A, B>(Data<Validated, Func<A, B>> f, Data<Validated, A> t) =>
        (f, t) switch
        {
            (Valid<Func<A, B>>(var g), Valid<A>(var x)) => Pure(g(x)),
            (Invalid<Func<A, B>>(var error), Valid<A>(_)) => new Invalid<B>(error),
            (Valid<Func<A, B>>(_), Invalid<A>(var error)) => new Invalid<B>(error),
            (Invalid<Func<A, B>>({ } reasons), Invalid<A>({ } otherReasons)) => new Invalid<B>([.. reasons, .. otherReasons]),
            _ => throw new NotSupportedException("Unlikely")
        };

    public static Data<Validated, B> Bind<A, B>(Data<Validated, A> t, Func<A, Data<Validated, B>> f) =>
        t switch
        {
            Valid<A>(var valid) => f(valid),
            Invalid<A>(var reasons) => new Invalid<B>(reasons),
            _ => throw new NotSupportedException("Unlikely")
        };

    public static Data<Validated, C> SelectMany<A, B, C>(Data<Validated, A> t, Func<A, Data<Validated, B>> f, Func<A, B, C> project) =>
        t switch
        {
            Valid<A>(var valid) => Bind(f(valid), r => new Valid<C>(project(valid, r))),
            Invalid<A>(var reasons) => new Invalid<C>(reasons),
            _ => throw new NotSupportedException("Unlikely")
        };
}


/// <summary>
/// The Valid data constructor
/// </summary>
/// <typeparam name="A"></typeparam>
/// <param name="Value"></param>
public sealed record Valid<A>(A Value) : Data<Validated, A>;

/// <summary>
/// The Invalid data constructor
/// </summary>
/// <typeparam name="A"></typeparam>
/// <param name="Reasons"></param>
public sealed record Invalid<A>(params string[] Reasons) : Data<Validated, A>;
