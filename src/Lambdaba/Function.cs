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

public record Function<A, B, C> :
    Function,
    Data<Function, A, B, C>
{
    public Func<A, B, C> Func { get; }

    public static implicit operator Function<A, B, C>(Func<A, B, C> func)
    {
        return new Function<A, B, C>(func);
    }

    public static implicit operator Func<A, B, C>(Function<A, B, C> function)
    {
        return function.Func;
    }

    internal Function(Func<A, B, C> func)
    {
        Func = func;
    }

    public C Invoke(A arg1, B arg2)
    {
        return Func(arg1, arg2);
    }
}

public record Function<A, B, C, D>(Func<A, B, C, D> Func)
{
    // Implicit conversion from Func<A, B, C, D> to Function<A, B, C, D>
    public static implicit operator Function<A, B, C, D>(Func<A, B, C, D> func) => new Function<A, B, C, D>(func);

    // Implicit conversion from Function<A, B, C, D> to Func<A, B, C, D>
    public static implicit operator Func<A, B, C, D>(Function<A, B, C, D> function) => function.Func;

    // Method to invoke the function
    public D Invoke(A arg1, B arg2, C arg3) => Func(arg1, arg2, arg3);
}

public record Function<A, B, C, D, E>(Func<A, B, C, D, E> Func)
{
    // Implicit conversion from Func<A, B, C, D, E> to Function<A, B, C, D, E>
    public static implicit operator Function<A, B, C, D, E>(Func<A, B, C, D, E> func) => new Function<A, B, C, D, E>(func);

    // Implicit conversion from Function<A, B, C, D, E> to Func<A, B, C, D, E>
    public static implicit operator Func<A, B, C, D, E>(Function<A, B, C, D, E> function) => function.Func;

    // Method to invoke the function
    public E Invoke(A arg1, B arg2, C arg3, D arg4) => Func(arg1, arg2, arg3, arg4);
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

public sealed record FunctionMonoid<A, B, C> : 
    Function<A, B, C>, 
    Monoid<FunctionMonoid<A, B, C>> 
        where C : Monoid<C>
{
    public FunctionMonoid(Func<A, B, C> func) : base(func) { }

    public static implicit operator FunctionMonoid<A, B, C>(Func<A, B, C> func) => new FunctionMonoid<A, B, C>(func);
    public static implicit operator Func<A, B, C>(FunctionMonoid<A, B, C> funcMonoid) => funcMonoid.Func;

    public static FunctionMonoid<A, B, C> Combine(FunctionMonoid<A, B, C> a, FunctionMonoid<A, B, C> b) => 
        new FunctionMonoid<A, B, C>((x, y) => C.Combine(a.Func(x, y), b.Func(x, y)));

    public static FunctionMonoid<A, B, C> Mempty() => new FunctionMonoid<A, B, C>((x, y) => C.Mempty());

    public static FunctionMonoid<A, B, C> STimes<D>(D y0, FunctionMonoid<A, B, C> x0)
        where D : Integral<D>
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

public sealed record FunctionMonoid<A, B, C, D> : 
    Function<A, B, C, D>, 
    Monoid<FunctionMonoid<A, B, C, D>> 
        where D : Monoid<D>
{
    public FunctionMonoid(Func<A, B, C, D> func) : base(func) { }

    public static implicit operator FunctionMonoid<A, B, C, D>(Func<A, B, C, D> func) => new FunctionMonoid<A, B, C, D>(func);
    public static implicit operator Func<A, B, C, D>(FunctionMonoid<A, B, C, D> funcMonoid) => funcMonoid.Func;

    public static FunctionMonoid<A, B, C, D> Combine(FunctionMonoid<A, B, C, D> a, FunctionMonoid<A, B, C, D> b) => 
        new FunctionMonoid<A, B, C, D>((x, y, z) => D.Combine(a.Func(x, y, z), b.Func(x, y, z)));

    public static FunctionMonoid<A, B, C, D> Mempty() => new FunctionMonoid<A, B, C, D>((x, y, z) => D.Mempty());

    public static FunctionMonoid<A, B, C, D> STimes<E>(E y0, FunctionMonoid<A, B, C, D> x0)
        where E : Integral<E>
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

public sealed record FunctionMonoid<A, B, C, D, E> : 
    Function<A, B, C, D, E>, 
    Monoid<FunctionMonoid<A, B, C, D, E>> 
        where E : Monoid<E>
{
    public FunctionMonoid(Func<A, B, C, D, E> func) : base(func) { }

    public static implicit operator FunctionMonoid<A, B, C, D, E>(Func<A, B, C, D, E> func) => new FunctionMonoid<A, B, C, D, E>(func);
    public static implicit operator Func<A, B, C, D, E>(FunctionMonoid<A, B, C, D, E> funcMonoid) => funcMonoid.Func;

    public static FunctionMonoid<A, B, C, D, E> Combine(FunctionMonoid<A, B, C, D, E> a, FunctionMonoid<A, B, C, D, E> b) => 
        new FunctionMonoid<A, B, C, D, E>((w, x, y, z) => E.Combine(a.Func(w, x, y, z), b.Func(w, x, y, z)));

    public static FunctionMonoid<A, B, C, D, E> Mempty() => new FunctionMonoid<A, B, C, D, E>((w, x, y, z) => E.Mempty());

    public static FunctionMonoid<A, B, C, D, E> STimes<F>(F y0, FunctionMonoid<A, B, C, D, E> x0)
        where F : Integral<F>
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

