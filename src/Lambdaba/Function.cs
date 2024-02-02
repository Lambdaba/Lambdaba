using Lambdaba.Control;
using static Lambdaba.Base;

namespace Lambdaba;

public abstract record Function : 
    Data<Function>, 
    Category<Function>,
    Monad<Function>
{
    public static Data<Function, B> Apply<A, B>(Data<Function, Func<A, B>> f, Data<Function, A> t) => 
        new Function<B>(() => ((Function<B>)f).Func());       

    public static Data<Function, B> Bind<A, B>(Data<Function, A> a, Func<A, Data<Function, B>> f) => 
        new Function<B>(() => ((Function<B>)f(((Function<A>)a).Func())).Func());
    public static Data<Function, A, C> Compose<A, B, C>(Data<Function, A, B> catAB, Data<Function, B, C> catBC) => 
        new Function<A, C>(x => ((Function<B, C>)catBC).Func(((Function<A, B>)catAB).Func(x)));
    public static Data<Function, B> FMap<A, B>(Func<A, B> f, Data<Function, A> a) => 
        new Function<B>(() => f(((Function<A>)a).Func()));
    public static Data<Function, A, A> Id<A>() => 
        new Function<A, A>(x => x);
    public static Data<Function, A> Pure<A>(A a) => 
        new Function<A>(() => a);
}

public record Function<A> :
    Function,
    Data<Function, A>
    
{
    public Func<A> Func { get; }

    public static implicit operator Function<A>(Func<A> func)
    {
        return new Function<A>(func);
    }

    public static implicit operator Func<A>(Function<A> function)
    {
        return function.Func;
    }

    internal Function(Func<A> func)
    {
        Func = func;
    }  
}

public record Function<A, B> :
    Function,
    Data<Function, A, B>
    
{
    public Func<A, B> Func { get; }

    public static implicit operator Function<A, B>(Func<A, B> func)
    {
        return new Function<A, B>(func);
    }

    public static implicit operator Func<A, B>(Function<A, B> function)
    {
        return function.Func;
    }

    internal Function(Func<A, B> func)
    {
        Func = func;
    }  
    
}

/// <summary>
/// A function is a monoid when and only when the codomain is a monoid
/// </summary>
/// <typeparam name="A"></typeparam>
/// <typeparam name="B"></typeparam>
public sealed record FunctionMonoid<A, B> : 
    Function<A, B>, 
    Monoid<FunctionMonoid<A, B>> 
        where B : Monoid<B>
    
{
    public FunctionMonoid(Func<A, B> original) : base(original)
    {
        
    }
    public static FunctionMonoid<A, B> Combine(FunctionMonoid<A, B> a, FunctionMonoid<A, B> b) => 
            new FunctionMonoid<A, B>(x => B.Combine(a.Func(x), b.Func(x)));
    public static FunctionMonoid<A, B> Mempty() => (FunctionMonoid<A, B>)(_ => B.Mempty());

    public static FunctionMonoid<A, B> STimes<C>(C y0, FunctionMonoid<A, B> x0)
            where C : Integral<C> 
            {
                var y = y0;
                var x = x0;
                var result = Mempty();
                while (y > y0)
                {
                    result = Combine(result, x);
                    y = y - y0;
                }
                return result;
            }  
}