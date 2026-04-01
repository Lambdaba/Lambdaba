using System;
using System.Runtime.CompilerServices;
using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// The Maybe type constructor (brand type for HKTs).
/// </summary>
public class Maybe :
    Data<Maybe>,
    Monad<Maybe>,
    Alternative<Maybe>,
    MonadPlus<Maybe>    
{
    /// <summary>
    /// Applies a function wrapped in a <see cref="Data{Maybe, Func{A, B}}"/> to a value wrapped in a <see cref="Data{Maybe, A}"/>.
    /// </summary>
    /// <typeparam name="A">The type of the input value.</typeparam>
    /// <typeparam name="B">The type of the output value.</typeparam>
    /// <param name="f">The <see cref="Data{Maybe, Func{A, B}}"/> containing the function to apply.</param>
    /// <param name="t">The <see cref="Data{Maybe, A}"/> containing the value to apply the function to.</param>
    /// <returns>A new <see cref="Data{Maybe, B}"/> containing the result of applying the function to the value.</returns>
    public static Data<Maybe, B> Apply<A, B>(Data<Maybe, Func<A, B>> f, Data<Maybe, A> t) =>
        ((Maybe<Func<A, B>>)f) switch
        {
            Nothing => new Maybe<B>(new Nothing()),
            Just<Func<A, B>>(var g) =>
                ((Maybe<A>)t) switch
                {
                    Nothing=> new Maybe<B>(new Nothing()),
                    Just<A>(var tt) => new Maybe<B>(new Just<B>(g(tt))),
                },
        };

    public static Data<Maybe, B> Bind<A, B>(Data<Maybe, A> o, Func<A, Data<Maybe, B>> f) =>
        ((Maybe<A>)o) switch
        {
            Just<A>(var t) => f(t),
            Nothing => new Maybe<B>(new Nothing()),
        };

    public static Data<Maybe, A> Where<A>(Data<Maybe, A> t, Func<A, bool> predicate) =>
        ((Maybe<A>)t) switch
        {
            Just<A>(var a) when predicate(a) => t,
            _ => new Maybe<A>(new Nothing())
        };

    public static Data<Maybe, C> SelectMany<A, B, C>(Data<Maybe, A> t, Func<A, Data<Maybe, B>> f, Func<A, B, C> project) =>
        ((Maybe<A>)t) switch
        {
            Nothing => new Maybe<C>(new Nothing()),
            Just<A>(var a) =>
                ((Maybe<B>)Bind(t, f)) switch
                {
                    Nothing => new Maybe<C>(new Nothing()),
                    Just<B>(var x) => new Maybe<C>(new Just<C>(project(a, x))),
                },
        };

    public static Data<Maybe, B> FMap<A, B>(Func<A, B> f, Data<Maybe, A> o) =>
        ((Maybe<A>)o) switch
        {
            Just<A>(var a) => new Maybe<B>(new Just<B>(f(a))),
            Nothing=> new Maybe<B>(new Nothing()),
        };
   
    public static Data<Maybe, A> Pure<A>(A t) =>
        new Maybe<A>(new Just<A>(t));

    public static Data<Maybe, A> Empty<A>() => 
        new Maybe<A>(new Nothing());

    /// <summary>
    /// Picks the leftmost 'Just' value, or, alternatively, 'Nothing'.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Data<Maybe, A> Or<A>(Data<Maybe, A> a, Data<Maybe, A> b) => 
        ((Maybe<A>)a) switch
        {
            Nothing=> b,
            Just<A> => a,
        };

    internal static Maybe<A> MkJust<A>(A a)
     => new Maybe<A>(new Just<A>(a));

    internal static Maybe<A> MkNothing<A>()
     => new Maybe<A>(new Nothing());
}

/// <summary>
/// The Maybe applied type — a union of Just and Nothing.
/// </summary>
/// <typeparam name="A">The contained value type.</typeparam>
[Union]
public class Maybe<A> :
    Maybe,
    Data<Maybe, A>,
    IUnion
{
    public Maybe(Just<A> value) { Value = value; }
    public Maybe(Nothing value) { Value = value; }

    /// <summary>The wrapped discriminated-union case; accessed by the <c>[Union]</c> switch rewrite.</summary>
    public object? Value { get => field; private set; }

    public static implicit operator Maybe<A>(Just<A> value) => new(value);
    public static implicit operator Maybe<A>(Nothing value) => new(value);
}

/// <summary>
/// Maybe<typeparamref name="A"/>> is a monoid when and only when the A is a monoid
/// </summary>
/// <typeparam name="A"></typeparam>
[Union]
public class MaybeMonoid<A> :
    IUnion,
    Monoid<MaybeMonoid<A>> 
    where A : Semigroup<A>
{
    public MaybeMonoid(JustMonoid<A> value) { Value = value; }
    public MaybeMonoid(NothingMonoid<A> value) { Value = value; }

    /// <summary>The wrapped discriminated-union case; accessed by the <c>[Union]</c> switch rewrite.</summary>
    public object? Value { get => field; private set; }

    public static implicit operator MaybeMonoid<A>(JustMonoid<A> value) => new(value);
    public static implicit operator MaybeMonoid<A>(NothingMonoid<A> value) => new(value);

    public static MaybeMonoid<A> Combine(MaybeMonoid<A> a, MaybeMonoid<A> b) => 
        a switch
        {
            NothingMonoid<A> => b,
            JustMonoid<A>(var x) => b switch
            {
                NothingMonoid<A> => a,
                JustMonoid<A>(var y) => new MaybeMonoid<A>(new JustMonoid<A>(A.Combine(x, y))),
            },
        };

    public static MaybeMonoid<A> Mempty() => new MaybeMonoid<A>(new NothingMonoid<A>());
}

public record Nothing;

public record Just<A>(A Value);

public record NothingMonoid<A> where A : Semigroup<A>;
public record JustMonoid<A>(A Value) where A : Semigroup<A>;