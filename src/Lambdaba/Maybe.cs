using static Lambdaba.Base;

namespace Lambdaba;

/// <summary>
/// When we will have roles and extensions in c# we can probably rewrite this
/// </summary>
/// <typeparam name="T"></typeparam>
public record Maybe :
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
        f switch
        {
            Nothing<A> _ => new Nothing<B>(),
            Just<Func<A, B>>(var g) =>
                t switch
                {
                    Nothing<A> _ => new Nothing<B>(),
                    Just<A>(var tt) => new Just<B>(g(tt)),
                    _ => throw new NotImplementedException(),
                },
            _ => throw new NotSupportedException()
        };

    public static Data<Maybe, B> Bind<A, B>(Data<Maybe, A> o, Func<A, Data<Maybe, B>> f) =>
        o switch
        {
            Just<A>(var t) => f(t),
            Nothing<A> _ => new Nothing<B>(),
            _ => throw new NotSupportedException()
        };

    public static Data<Maybe, C> SelectMany<A, B, C>(Data<Maybe, A> t, Func<A, Data<Maybe, B>> f, Func<A, B, C> project) =>
        t switch
        {
            Nothing<A> => new Nothing<C>(),
            Just<A>(var a) =>
                Bind(t, f) switch
                {
                    Nothing<B> => new Nothing<C>(),
                    Just<B>(var x) => new Just<C>(project(a, x)),
                    _ => throw new NotImplementedException()
                },
            _ => throw new NotImplementedException()
        };

    public static Data<Maybe, B> FMap<A, B>(Func<A, B> f, Data<Maybe, A> o) =>
        o switch
        {
            Just<A>(var a) => new Just<B>(f(a)),
            Nothing<A> _ => new Nothing<B>(),
            _ => throw new NotSupportedException()
        };
   
    public static Data<Maybe, A> Pure<A>(A t) =>
        new Just<A>(t);
    public static Data<Maybe, A> Empty<A>() => 
        new Nothing<A>();


    /// <summary>
    /// Picks the leftmost 'Just' value, or, alternatively, 'Nothing'.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Data<Maybe, A> Or<A>(Data<Maybe, A> a, Data<Maybe, A> b) => 
        a switch
        {
            Nothing<A> _ => b,
            Just<A> => a,
            _ => throw new NotImplementedException()
        };
}

public abstract record Maybe<A> :
    Maybe,
    Data<Maybe, A>
{
    
}

/// <summary>
/// Maybe<typeparamref name="A"/>> is a monoid when and only when the A is a monoid
/// </summary>
/// <typeparam name="A"></typeparam>
public abstract record MaybeMonoid<A> :
    Maybe<A>,
    Monoid<MaybeMonoid<A>> 
    where A : Semigroup<A>
{
    public static MaybeMonoid<A> Combine(MaybeMonoid<A> a, MaybeMonoid<A> b) => 
        a switch
        {
            NothingMonoid<A> _ => b,
            JustMonoid<A>(var x) => b switch
            {
                NothingMonoid<A> _ => a,
                JustMonoid<A>(var y) => new JustMonoid<A>(A.Combine(x, y)),
                _ => throw new NotImplementedException()
            },
            _ => throw new NotImplementedException()
        };
    public static MaybeMonoid<A> Mempty() => new NothingMonoid<A>();
}

internal record Nothing<A> : Maybe<A>;

internal record Just<A>(A Value) : Maybe<A>;

public sealed record NothingMonoid<A> : MaybeMonoid<A> where A : Semigroup<A>;
public sealed record JustMonoid<A>(A Value) : MaybeMonoid<A> where A : Semigroup<A>;
