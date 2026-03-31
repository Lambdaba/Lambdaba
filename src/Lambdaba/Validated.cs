using System;
using System.Runtime.CompilerServices;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// The Validated type constructor (brand type for HKTs).
/// </summary>
public class Validated :
    Monad<Validated>,
    Data<Validated>
{
    public static Data<Validated, B> FMap<A, B>(Func<A, B> f, Data<Validated, A> t) =>
        Bind(t, x => new Validated<B>(new Valid<B>(f(x))));

    public static Data<Validated, A> Pure<A>(A a) =>
        new Validated<A>(new Valid<A>(a));

    public static Data<Validated, B> Apply<A, B>(Data<Validated, Func<A, B>> f, Data<Validated, A> t) =>
        (((Validated<Func<A, B>>)f), ((Validated<A>)t)) switch
        {
            (Valid<Func<A, B>>(var g), Valid<A>(var x)) => Pure(g(x)),
            (Invalid(var error), Valid<A>(_)) => new Validated<B>(new Invalid(error)),
            (Valid<Func<A, B>>(_), Invalid(var error)) => new Validated<B>(new Invalid(error)),
            (Invalid({ } reasons), Invalid({ } otherReasons)) => new Validated<B>(new Invalid([.. reasons, .. otherReasons])),
        };

    public static Data<Validated, B> Bind<A, B>(Data<Validated, A> t, Func<A, Data<Validated, B>> f) =>
        ((Validated<A>)t) switch
        {
            Valid<A>(var valid) => f(valid),
            Invalid(var reasons) => new Validated<B>(new Invalid(reasons)),
        };

    public static Data<Validated, C> SelectMany<A, B, C>(Data<Validated, A> t, Func<A, Data<Validated, B>> f, Func<A, B, C> project) =>
        ((Validated<A>)t) switch
        {
            Valid<A>(var valid) => Bind(f(valid), r => new Validated<C>(new Valid<C>(project(valid, r)))),
            Invalid(var reasons) => new Validated<C>(new Invalid(reasons)),
        };
}

/// <summary>
/// The Validated applied type — a union of Valid and Invalid.
/// </summary>
/// <typeparam name="A">The contained value type.</typeparam>
[Union]
public class Validated<A> :
    Validated,
    Data<Validated, A>,
    IUnion
{
    private readonly object? _value;

    public Validated(Valid<A> value) { _value = value; }
    public Validated(Invalid value) { _value = value; }

    public object? Value => _value;

    public static implicit operator Validated<A>(Valid<A> value) => new(value);
    public static implicit operator Validated<A>(Invalid value) => new Invalid();
}

/// <summary>
/// The Valid data constructor
/// </summary>
/// <typeparam name="A"></typeparam>
/// <param name="Value"></param>
public sealed record Valid<A>(A Value);

/// <summary>
/// The Invalid data constructor
/// </summary>
/// <typeparam name="A"></typeparam>
/// <param name="Reasons"></param>
public sealed record Invalid(params string[] Reasons);
