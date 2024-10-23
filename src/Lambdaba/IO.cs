using System;
using static Lambdaba.Base;

namespace Lambdaba;

public record IO : 
    Data<IO>,
    Monad<IO>,
    MonadPlus<IO>,
    Alternative<IO>
{
    public static Data<IO, B> Apply<A, B>(Data<IO, Func<A, B>> f, Data<IO, A> t) => 
        Ap(f, t);

    
    public static Data<IO, B> SequenceRight<A, B>(Data<IO, A> a, Data<IO, B> b) =>
        ThenIO((IO<A>)a, (IO<B>)b);

    public static Data<IO, B> Bind<A, B>(Data<IO, A> a, Func<A, Data<IO, B>> f) =>
       new IO<B>(s =>
        {
            var (s1, a1) = ((IO<A>)a).Action(s);
            return ((IO<B>)f(a1)).Action(s1);
        }); 

    public static Data<IO, A> Empty<A>() => 
        FailIO<A>("mzero");
    public static Data<IO, B> FMap<A, B>(Func<A, B> f, Data<IO, A> a) => 
        Bind(a, a => Pure(f(a)));
    public static Data<IO, A> Or<A>(Data<IO, A> a, Data<IO, A> b) => 
        new IO<A>(s =>
        {
            try
            {
                return ((IO<A>)a).Action(s);
            }
            catch
            {
                return ((IO<A>)b).Action(s);
            }
        });
    public static Data<IO, A> Pure<A>(A x) => 
        ReturnIO(x);
}

public readonly record struct WorldState;

public record IO<A>(Func<WorldState, (WorldState, A)> Action) : IO, Data<IO, A>
{
    public static implicit operator IO<A>(Func<WorldState, (WorldState, A)> action)
    {
        return new IO<A>(action);
    }

    public static implicit operator Func<WorldState, (WorldState, A)>(IO<A> io)
    {
        return io.Action;
    }
}


// public record IOMonoid<A> : IO<A>, Monoid<IOMonoid<A>>
//     where A : Monoid<A>
// {
//     public static IOMonoid<A> Combine(IOMonoid<A> a, IOMonoid<A> b) => 
        
//     public static IOMonoid<A> Mempty() => throw new NotImplementedException();
//     public static IOMonoid<A> STimes<C>(C y0, IOMonoid<A> x0) where C : Integral<C> => throw new NotImplementedException();
// }
