using System;
using Lambdaba.Control;
using static Lambdaba.Base;

namespace Lambdaba;

public static class FunctionExtensions
{
/// <summary>
    /// Uncurries a function with two arguments.
    /// </summary>
    public static Data<Function, (A, B), C> Uncurry<A, B, C>(this Data<Function, A, Func<B, C>> func) =>
        new Function<(A, B), C>(tuple => ((Function<B, C>)func).Func(tuple.Item2));

    public static Data<Function, A, Func<B, C>> Curry<A, B, C>(this Data<Function, (A, B), C> func) =>
        new Function<A, Func<B, C>>(a => new Function<B, C>(b => ((Function<(A, B), C>)func).Func((a, b))).Func);

    public static Function<(A, B), C> Uncurry<A, B, C>(this Function<A, Function<B, C>> func) =>
        new Function<(A, B), C>(tuple => func.Func(tuple.Item1).Func(tuple.Item2));

    public static Function<A, Function<B, C>> Curry<A, B, C>(this Function<(A, B), C> func) => 
        new Function<A, Function<B, C>>(a => new Function<B, C>(b => func.Func((a, b))));


    public static Data<Function, A, B, C> Compose<A, B, C>(this Data<Function, B, C> catBC, Data<Function, A, B> catAB) =>
        new Function<A, B, C>((a, b) => ((Function<B, C>)catBC).Func(((Function<A, B>)catAB).Func(a)));

    public static Data<Function, A, B, C> Compose<A, B, C>(this Data<Function, C> catBC, Data<Function, B, C> catAB) =>  
        new Function<A, B, C>((a, b) => ((Function<B, C>)catBC).Func(((Function<A, B>)catAB).Func(a)));
}

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

    



    /// <summary>
    /// Partially applies a function with one argument.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <param name="func"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Function<B, C> Partial<A, B, C>(Function<A, B, C> func, A a) => 
        new(b => func.Func(a, b));

    
}

public record Function<A> :
    Function,
    Data<Function, A>
    
{
    public Func<A> Func { get; }   

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

    public B Invoke(A arg) => Func(arg);

    public B Apply(A arg) => Func(arg);

    internal Function(Func<A, B> func)
    {
        Func = func;
    }  
    
    /// <summary>
    /// Overload the | operator for curried function application. Like in F#.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static B operator |(A arg, Function<A, B> func) => func.Func(arg);
}


public record Function<A, B, C> :
    Function,
    Data<Function, A, B, C>
{
    public Func<A, B, C> Func { get; }

    internal Function(Func<A, B, C> func)
    {
        Func = func;
    }

    public C Apply(A arg1, B arg2) => Func(arg1, arg2);

    public C Invoke(A arg1, B arg2) => Apply(arg1, arg2);

     public static C operator |((A, B) args, Function<A, B, C> func) => func.Func(args.Item1, args.Item2);
}

public record Function<A, B, C, D>(Func<A, B, C, D> Func)
{
       // Method to invoke the function
    public D Invoke(A arg1, B arg2, C arg3) => Apply(arg1, arg2, arg3);

    public D Apply(A arg1, B arg2, C arg3) => Func(arg1, arg2, arg3);
}

public record Function<A, B, C, D, E>(Func<A, B, C, D, E> Func)
{ 

    // Method to invoke the function
    public E Invoke(A arg1, B arg2, C arg3, D arg4) => Apply(arg1, arg2, arg3, arg4);

    public E Apply(A arg1, B arg2, C arg3, D arg4) => Func(arg1, arg2, arg3, arg4);
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
            new(x => B.Combine(a.Func(x), b.Func(x)));
    public static FunctionMonoid<A, B> Mempty() => new FunctionMonoid<A, B>(_ => B.Mempty());

    public static FunctionMonoid<A, B> STimes<C>(C y0, FunctionMonoid<A, B> x0)
            where C : Integral<C> 
            {
                var y = y0;
                var x = x0;
                var result = Mempty();
                while (y > y0)
                {
                    result = Combine(result, x);
                    y -= y0;
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

    public static implicit operator FunctionMonoid<A, B, C>(Func<A, B, C> func) => new(func);
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
            y -= y0;
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
            y -= y0;
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
            y -= y0;
        }
        return result;
    }
}

// Using the | operator to chain function calls
public class Foo
{

    public Foo()
    {
        var increment = new Function<int, int>(x => x + 1);
        var doubleFunc = new Function<int, int>(x => x * 2);
        var addCurried = new Function<int, Function<int, int>>(x => new Function<int, int>(y => x + y));

        int result = 
        5
        | increment
        | doubleFunc
        | (3 | addCurried); // 5 + 1 * 2 + 3 = 10
    }
}