using System.Numerics;
using Lambdaba;
using Lambdaba.Data;
using static Lambdaba.Types;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Lambdaba;

public static class Base
{
    public record struct Word(uint Value) :
        Data<Word>,
        Bounded<Word>,
        Enum<Word>
    {
        public static Word MinBound =>
            new(0);

        public static Word MaxBound =>
            // in haskell hte value is set explicitly for 64 and 32 bits using a compile time directibe. 
            // unint.MaxValue is set correctly for 32 and 64 bits automatically
            new(uint.MaxValue);

        public static Int FromEnum(Word x) =>
            x >= 0
                ? (Int)x
                : throw new ArgumentOutOfRangeException(nameof(x), "out of range");
        public static Word ToEnum(Int x) => (Word)x;

        public static Types.List<Word> EnumFrom(Word x) =>
            Enum.EftWord(x, MaxBound);

        public static Types.List<Word> EnumFromTo(Word x, Word y) =>
            Enum.EftWord(x, y);

        public static Types.List<Word> EnumFromThen(Word x, Word y) =>
            Enum.EfdWord(x, y);

        public static Types.List<Word> EnumFromThenTo(Word x, Word y, Word z) =>
            Enum.EfdtWord(x, y, z);

        public static explicit operator Word(Int v) =>
            v >= 0
                ? new Word((uint)v.Value)
                : throw new ArgumentOutOfRangeException(nameof(v), "out of range");

        public static Word Succ(Word x) =>
            x == MaxBound
                ? throw new ArgumentOutOfRangeException(nameof(x), "maxBound")
                : x + 1;

        public static Word Pred(Word x) =>
            x == MinBound
                ? throw new ArgumentOutOfRangeException(nameof(x), "minBound")
                : x - 1;

        public static implicit operator Word(uint value) => new();

        public static implicit operator uint(Word value) => value.Value;
    }


    public abstract record Bool :
        Data<Bool>,
        Bounded<Bool>,
        Enum<Bool>
    {
        public static Bool MinBound =>
            new False();

        public static Bool MaxBound =>
            new True();

        public static implicit operator Bool(bool value) => value ? new True() : new False();

        public static implicit operator bool(Bool value) => value switch
        {
            True _ => true,
            False _ => false,
            _ => throw new NotSupportedException()
        };

        public static Bool operator !(Bool value) =>
            Not(value);

        public static Bool operator &(Bool a, Bool b) =>
            (a, b) switch
            {
                (True _, True _) => new True(),
                _ => new False()
            };

        public static Bool operator |(Bool a, Bool b) =>
            (a, b) switch
            {
                (False _, False _) => new False(),
                _ => new True()
            };

        public static Bool ToEnum(Int x) =>
            (int)x switch
            {
                0 => new False(),
                1 => new True(),
                _ => throw new NotSupportedException()
            };
        public static Int FromEnum(Bool x) =>
            x switch
            {
                False _ => new Int(0),
                True _ => new Int(1),
                _ => throw new NotSupportedException()
            };

        public static Bool Succ(Bool x) =>
            x switch
            {
                False _ => new True(),
                True _ => throw new ArgumentOutOfRangeException(nameof(x), "maxBound"),
                _ => throw new NotSupportedException()
            };

        public static Bool Pred(Bool x) =>
            x switch
            {
                True _ => new False(),
                False _ => throw new ArgumentOutOfRangeException(nameof(x), "minBound"),
                _ => throw new NotSupportedException()
            };
    }

    public record True : Bool
    {
        public override string ToString() => "True";
    }

    public record False : Bool
    {
        public override string ToString() => "False";
    }

    public static Func<Bool, Bool> Not => b =>
        b switch
        {
            True _ => new False(),
            False _ => new True(),
            _ => throw new NotSupportedException()
        };

    public static Bool Otherwise => new True();

    /// <summary>
    /// 'String' is an alias for a list of characters.
    /// </summary>
    public record String : Types.List<Char>
    {
        public static implicit operator String(Char[] chars) =>
            new String(chars);

        public static implicit operator Char[](String @string) =>
            @string;

        public static implicit operator String(string @string) =>
            new String(@string);

        public static implicit operator string(String @string) =>
            @string;

    }



    public readonly record struct Char :
        Data<Char>,
        Bounded<Char>,
        Enum<Char>
    {
        public char Value { get; }

        public static Char MinBound =>
            '\0';  // which is equivalent to Char.MinValue

        public static Char MaxBound =>
            '\uffff'; // which is equivalent to Char.MaxValue

        public Char(char value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public static Char ToEnum(Int x) =>
            Data.Char.Chr(x);

        public static Char Succ(Char x) =>
            (char)x switch
            {
                < '\uffff' => new Char((char)(x.Value + 1)),
                _ => throw new ArgumentOutOfRangeException(nameof(x), "maxBound")
            };

        public static Char Pred(Char x) =>
            (char)x switch
            {
                > '\0' => new Char((char)(x.Value - 1)),
                _ => throw new ArgumentOutOfRangeException(nameof(x), "minBound")
            };

        public static Types.List<Char> EnumFrom(Char x) =>
            Enum.EftChar(Data.Char.Ord(x), Data.Char.Ord(MaxBound));

        public static Types.List<Char> EnumFromTo(Char x, Char y) =>
            Enum.EftChar(Data.Char.Ord(x), Data.Char.Ord(y));

        public static Types.List<Char> EnumFromThen(Char x, Char y) =>
            Enum.EfdChar(Data.Char.Ord(x), Data.Char.Ord(y));

        public static Types.List<Char> EnumFromThenTo(Char x, Char y, Char z) =>
            Enum.EfdtChar(Data.Char.Ord(x), Data.Char.Ord(y), Data.Char.Ord(z));

        public static Int FromEnum(Char x) =>
            Data.Char.Ord(x);

        public static implicit operator Char(char value) => new(value);

        public static implicit operator char(Char value) => value.Value;
    }

    public readonly record struct Int :
        Integral<Int>,
        Real<Int>,
        Enum<Int>,
        Ord<Int>,
        Eq<Int>,
        Num<Int>,
        Bounded<Int>
    {
        public int Value { get; }

        public static Int MinBound => int.MinValue;

        public static Int MaxBound => int.MaxValue;

        public Int(int value)
        {
            Value = value;
        }

        public static Int Quot(Int a, Int b)
        {
            if (b == 0)
                throw new DivideByZeroException();
            if (b == -1 && a == MinBound)
                throw new OverflowException();
            return QuotInt(a, b);
        }

        public static Int Rem(Int n, Int d)
        {
            if (d == 0)
                throw new DivideByZeroException();
            if (d == -1 && n == MinBound)
                throw new OverflowException();
            return RemInt(n, d);
        }

        public static Int Div(Int n, Int d)
        {
            if (d == 0)
                throw new DivideByZeroException();
            if (d == -1 && n == MinBound)
                throw new OverflowException();
            return DivInt(n, d);
        }

        public static Int Mod(Int n, Int d)
        {
            if (d == 0)
                throw new DivideByZeroException();
            if (d == -1 && n == MinBound)
                throw new OverflowException();
            return ModInt(n, d);
        }

        public static (Int, Int) DivMod(Int n, Int d)
        {
            if (d == 0)
                throw new DivideByZeroException();
            if (d == -1 && n == MinBound)
                throw new OverflowException();
            return DivModInt(n, d);
        }

        public static (Int, Int) QuotRem(Int n, Int d)
        {
            if (d == 0)
                throw new DivideByZeroException();
            if (d == -1 && n == MinBound)
                throw new OverflowException();
            return QuotRemInt(n, d);
        }

        public override string ToString() => Value.ToString();
        public static Integer ToInteger(Int x) => new IS(x);

        public static Rational ToRational(Int x) => new Rational(ToInteger(x), ToInteger(1));
        public static Int Add(Int x, Int y) =>
            x + y;
        public static Int Abs(Int a) =>
            (int)a switch
            {
                < 0 => -a,
                _ => a
            };

        public static Int Signum(Int a) =>
            (int)a switch
            {
                < 0 => -1,
                0 => 0,
                _ => 1
            };

        public static Int FromInteger(int a) => a;
        public static Int ToEnum(Int x) => x;
        public static Int FromEnum(Int x) =>
            x;

        public static Int Succ(Int x) =>
            x == MaxBound
                ? throw new ArgumentOutOfRangeException(nameof(x), "maxBound")
                : x + 1;

        public static Int Pred(Int x) =>
            x == MinBound
                ? throw new ArgumentOutOfRangeException(nameof(x), "minBound")
                : x - 1;

        public static Types.List<Int> EnumFrom(Int x) =>
            Enum.EftInt(x, MaxBound);

        public static Types.List<Int> EnumFromTo(Int x, Int y) =>
            Enum.EftInt(x, y);

        public static Types.List<Int> EnumFromThen(Int x, Int y) =>
            Enum.EfdInt(x, y);

        public static Types.List<Int> EnumFromThenTo(Int x, Int y, Int z) =>
            Enum.EfdtInt(x, y, z);

        public static implicit operator Int(int value) => new(value);

        public static implicit operator int(Int value) => value.Value;

        public static Int operator -(Int x, Int y) =>
            x - y;
        public static Int operator *(Int x, Int y) =>
            x * y;
        public static Int operator -(Int x) =>
            -x;
        public static Int operator +(Int x) =>
            x;

        public static explicit operator uint(Int v) =>
           (uint)v.Value;

        public static explicit operator Int(Word v) =>
            new((int)v.Value);

        public static explicit operator Int(BigInteger v) =>
            new((int)v);
        
    }

    public abstract record Ordering :
        Data<Ordering>,
        Semigroup<Ordering>,
        Monoid<Ordering>,
        Bounded<Ordering>,
        Enum<Ordering>
    {
        public static Ordering MinBound => new LT();

        public static Ordering MaxBound => new GT();

        public static Ordering Combine(Ordering a, Ordering b) =>
            (a, b) switch
            {
                (LT, _) => new LT(),
                (EQ, _) => b,
                (GT, _) => new GT(),
                _ => throw new NotImplementedException()
            };

        public static Ordering STimes<B>(B n, Ordering x) where B : Integral<B> =>
            B.Compare(n, B.FromInteger(0)) switch
            {
                LT => throw new ArgumentException("Positive multiplier expected"),
                EQ => new EQ(),
                GT => x,
                _ => throw new NotImplementedException()
            };


        public static Ordering Mempty() =>
            new EQ();
        public static Ordering ToEnum(Int x) =>
            (int)x switch
            {
                0 => new LT(),
                1 => new EQ(),
                2 => new GT(),
                _ => throw new NotSupportedException()
            };

        public static Int FromEnum(Ordering x) =>
            x switch
            {
                LT _ => new Int(0),
                EQ _ => new Int(1),
                GT _ => new Int(2),
                _ => throw new NotSupportedException()
            };
    }

    public sealed record LT : Ordering;
    public sealed record EQ : Ordering;
    public sealed record GT : Ordering;


    public record struct Unit : Data<Unit>, Monoid<Unit>, Semigroup<Unit>
    {
        public static Unit Mempty() => new();
        public static Unit Combine(Unit a, Unit b) => new();
        public static Unit operator +(Unit a, Unit b) => new();
        public static Unit STimes<B>(B _, Unit a) where B : Integral<B> => new();
        public static Unit MConcat(Types.List<Unit> @as) => new();
        public override readonly string ToString() => "()";
    }

    /// <summary>
    /// Uninhabited data type
    /// </summary>
    public record struct Void : Data<Void>, Ord<Void>, Semigroup<Void>
    {
        public static Void Combine(Void a, Void b) => a;

        public static Void operator +(Void a, Void b) => a;

        public static Void STimes<B>(B _, Void a) where B : Integral<B> => a;
    }

    /// <summary>
    /// Since 'Void' values logically don't exist, 
    /// this witnesses the logical reasoning tool of "ex falso quodlibet" (from false, anything follows).
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    public static A Absurd<A>(Void a) => a switch { };

    /// <summary>
    /// If 'Void' is uninhabited then any 'Functor' that holds only values of type 'Void' is holding no values.
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Data<F, A> Vacuous<F, A>(Data<F, Void> a) where F : Functor<F> =>
        F.FMap(Absurd<A>, a);

    public interface Semigroup<A>
        where A : Semigroup<A>
    {
        /// <summary>
        /// An associative operation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static abstract A Combine(A a, A b);

        /// <summary>
        /// An associative operation (equivalent to Combine and the <> operator in Haskell)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static virtual A operator +(A a, A b) => A.Combine(a, b);

        static virtual A SConcat(NonEmpty<A> @as) =>
            @as switch
            {
            [var x, .. var xs] => A.Combine(x, A.SConcat((NonEmpty<A>)xs)),
                _ => throw new NotSupportedException()
            };

        static virtual A STimes<B>(B y0, A x0)
            where B : Integral<B>
        {
            A f(A x, B y) =>
                B.Rem(y, B.FromInteger(2)) == B.FromInteger(0)
                    ? f(A.Combine(x, x), B.Quot(y, B.FromInteger(2)))
                    : y.Equals(B.FromInteger(1))
                        ? x
                        : g(A.Combine(x, x), B.Quot(y, B.FromInteger(2)), x);


            A g(A x, B y, A z) =>
                B.Rem(y, B.FromInteger(2)) == B.FromInteger(0)
                    ? g(A.Combine(x, x), B.Quot(y, B.FromInteger(2)), z)
                    : y.Equals(B.FromInteger(1))
                        ? A.Combine(x, z)
                        : g(A.Combine(x, x), B.Quot(y, B.FromInteger(2)), A.Combine(x, z));

            return y0 switch
            {
                <= 0 => throw new ArgumentException("Positive multiplier expected"),
                _ => f(x0, y0)
            };
        }


    }

    public interface Monoid<A> : Semigroup<A>
        where A : Monoid<A>
    {
        /// <summary>
        /// Identity of 'mappend'
        /// </summary>
        /// <returns></returns>
        static abstract A Mempty();

        /// <summary>
        /// An associative operation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static virtual A Mappend(A a, A b) =>
            A.Combine(a, b);

        /// <summary>
        /// Fold a foldable using the monoid
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="as"></param>
        /// <returns></returns>
        static virtual A MConcat<F>(Data<F, A> @as)
            where F : Foldable<F> =>
                F.FoldR(A.Mappend, A.Mempty(), @as);

        static virtual A MConcat(Types.List<A> @as) =>
            @as switch
            {
            [] => A.Mempty(),
            [var x, .. var xs] => A.Mappend(x, A.MConcat(xs)),
                _ => throw new NotSupportedException()
            };

        static virtual A operator +(A a, A b) =>
            A.Mappend(a, b);

    }

    public interface Functor<F> where F : Functor<F>
    {
        public static abstract Data<F, B> FMap<A, B>(Func<A, B> f, Data<F, A> a);

        /// <summary>
        /// Replace all locations in the input with the same value. 
        /// The default definition is FMap composed with Const, 
        /// but this may be overridden with a more efficient version.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static virtual Data<F, A> Replace<A, B>(A a, Data<F, B> b) =>
            F.FMap(Const<A, B>()(a), b);
    }

    /// <summary>
    /// The equivalent of the Applicative type class. 
    /// </summary>
    /// <typeparam name="F"></typeparam>
    public interface Applicative<F> : Functor<F> where F : Applicative<F>
    {
        public static abstract Data<F, A> Pure<A>(A a);

        /// <summary>
        /// Sequential application. Dafault implementation is provided in ApplicationExtensions
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="f"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static abstract Data<F, B> Apply<A, B>(Data<F, Func<A, B>> f, Data<F, A> t);

        /// <summary>
        /// Lift a function to actions.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static virtual Data<F, C> LiftA2<A, B, C>(Func<A, Func<B, C>> f, Data<F, A> a, Data<F, B> b) =>
            F.Apply(F.FMap(f, a), b);

        /// <summary>
        /// Sequence actions, discarding the value of the first argument.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static virtual Data<F, B> SequenceRight<A, B>(Data<F, A> a, Data<F, B> b) =>
            F.LiftA2<A, B, B>((_) => (y) => y, a, b);

        /// <summary>
        /// Sequence actions, discarding the value of the second argument.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static virtual Data<F, A> SequenceLeft<A, B>(Data<F, B> b, Data<F, A> a) =>
            F.LiftA2(Const<A, B>(), a, b);

    }


    /// <summary>
    /// Applies a wrapped function to a wrapped value using the provided applicative instance. 
    /// A variant of 'Apply with the types of the arguments reversed. 
    /// </summary>
    /// <typeparam name="F">The applicative type.</typeparam>
    /// <typeparam name="A">The type of the wrapped value.</typeparam>
    /// <typeparam name="B">The type of the result.</typeparam>
    /// <param name="a">The wrapped value.</param>
    /// <param name="f">The wrapped function.</param>
    /// <returns>The result of applying the wrapped function to the wrapped value.</returns>
    public static Data<F, B> ApplyWrappedFunc<F, A, B>(Data<F, A> a, Data<F, Func<A, B>> f) where F : Applicative<F> =>
        F.Apply(f, a);

    public static Data<F, B> Lift<F, A, B>(Func<A, B> f, Data<F, A> a) where F : Applicative<F> =>
        F.Apply(F.Pure(f), a);


    public static Data<F, D> LiftA3<F, A, B, C, D>(Func<A, Func<B, Func<C, D>>> f, Data<F, A> a, Data<F, B> b, Data<F, C> c) where F : Applicative<F> =>
        F.Apply(F.LiftA2(f, a, b), c);


    /// <summary>
    /// The 'join' function is the conventional monad join operator. 
    /// It is used to remove one level of monadic structure, projecting its bound argument into the outer level.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="m"></param>
    /// <returns></returns>
    public static Data<M, A> Join<M, A>(Data<M, Data<M, A>> m) where M : Monad<M> =>
        M.Bind(m, Id<Data<M, A>>());

    /// <summary>
    /// The equivalent of the Monad type class
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public interface Monad<M> : Applicative<M> where M : Monad<M>
    {
        public static virtual Data<M, A> Return<A>(A a) =>
            M.Pure(a);


        public static abstract Data<M, B> Bind<A, B>(Data<M, A> a, Func<A, Data<M, B>> f);

        // m >> k = m >>= (\_ -> k)
        /// <summary>
        /// A convenience operator that is used to bind a monadic computation that does not require input from the previous computation in the sequence. 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Data<M, B> Bind<A, B>(Data<M, A> a, Data<M, B> b) =>
            M.Bind(a, _ => b);

        public static virtual Data<M, C> SelectMany<A, B, C>(Data<M, A> t, Func<A, Data<M, B>> f,
            Func<A, B, C> project) =>
                M.Bind(t, a => M.FMap(b => project(a, b), f(a)));
    }

    /// <summary>
    /// Same as Bind but with the arguments flipped
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="f"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Data<M, B> Bind<M, A, B>(Func<A, Data<M, B>> f, Data<M, A> a) where M : Monad<M> =>
        M.Bind(a, f);

    /// <summary>
    /// Conditional execution of 'Applicative' expressions.
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <param name="b"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Data<F, Unit> When<F>(Bool b, Data<F, Unit> m) where F : Applicative<F> =>
        b switch
        {
            True => m,
            False => F.Pure(new Unit()),
            _ => throw new NotImplementedException()
        };



    /// <summary>
    /// The sequence function takes a list of monadic computations, executes each one in turn and returns a list of the results.
    /// If any of the computations fail, then the whole function fails:
    /// Evaluate each action in the sequence from left to right, and collect the results
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="as"></param>
    /// <returns></returns>
    public static Data<M, Types.List<A>> Sequence<M, A>(Types.List<Data<M, A>> @as) where M : Monad<M> =>
        @as switch
        {
        [] => M.Return((Types.List<A>)[]),
        [var x, .. var xs] => M.Bind(x, a => M.FMap(xs => (Types.List<A>)new NonEmpty<A>(a, xs), Sequence(xs)))
        };


    /// <summary>
    /// The sequence function takes a list of monadic computations, executes each one in turn and returns a list of the results.
    /// If any of the computations fail, then the whole function fails:
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="as"></param>
    /// <returns></returns>
    public static Data<M, Unit> Sequence_<M, A>(Types.List<Data<M, A>> @as) where M : Monad<M> =>
        @as switch
        {
        [] => M.Return(new Unit()),
        [var x, .. var xs] => M.Bind(x, _ => Sequence_(xs))
        };


    /// <summary>
    /// The mapM function maps a monadic computation over a list of values and returns a list of the results.
    /// It is defined in terms of the list map function and the sequence function above:
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="f"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    public static Data<M, Types.List<B>> MapM<M, A, B>(Func<A, Data<M, B>> f, Types.List<A> @as) where M : Monad<M>
    {
        Data<M, Types.List<B>> k(A a, Data<M, Types.List<B>> r) =>
            M.Bind(f(a), x => M.FMap(xs => (Types.List<B>)new NonEmpty<B>(x, xs), r));

        return FoldR(k, M.Return((Types.List<B>)[]), @as);
    }

    /// <summary>
    /// Promote a function to a monad.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="A1"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name="f"></param>
    /// <param name="m1"></param>
    /// <returns></returns>
    public static Data<M, R> LiftM<M, A1, R>(Func<A1, R> f, Data<M, A1> m1) where M : Monad<M>
    {
        Data<M, R> k(A1 a1) => M.Return(f(a1));
        return M.Bind(m1, k);
    }

    public static Data<M, R> LiftM2<M, A1, A2, R>(Func<A1, Func<A2, R>> f, Data<M, A1> m1, Data<M, A2> m2) where M : Monad<M>
    {
        Data<M, R> k(A1 a1) => LiftM(f(a1), m2);
        return M.Bind(m1, k);
    }

    public static Data<M, R> LiftM3<M, A1, A2, A3, R>(Func<A1, Func<A2, Func<A3, R>>> f, Data<M, A1> m1, Data<M, A2> m2, Data<M, A3> m3) where M : Monad<M>
    {
        Data<M, R> k(A1 a1) => LiftM2(f(a1), m2, m3);
        return M.Bind(m1, k);
    }

    public static Data<M, R> LiftM4<M, A1, A2, A3, A4, R>(Func<A1, Func<A2, Func<A3, Func<A4, R>>>> f, Data<M, A1> m1, Data<M, A2> m2, Data<M, A3> m3, Data<M, A4> m4) where M : Monad<M>
    {
        Data<M, R> k(A1 a1) => LiftM3(f(a1), m2, m3, m4);
        return M.Bind(m1, k);
    }

    public static Data<M, R> LiftM5<M, A1, A2, A3, A4, A5, R>(Func<A1, Func<A2, Func<A3, Func<A4, Func<A5, R>>>>> f, Data<M, A1> m1, Data<M, A2> m2, Data<M, A3> m3, Data<M, A4> m4, Data<M, A5> m5) where M : Monad<M>
    {
        Data<M, R> k(A1 a1) => LiftM4(f(a1), m2, m3, m4, m5);
        return M.Bind(m1, k);
    }

    public static Data<M, B> Ap<M, A, B>(Data<M, Func<A, B>> f, Data<M, A> t) where M : Monad<M>
    {
        Data<M, B> k(Func<A, B> f1) => LiftM(f1, t);
        return M.Bind(f, k);
    }

    public interface Alternative<F> : Applicative<F> where F : Alternative<F>
    {
        public static abstract Data<F, A> Empty<A>();

        public static abstract Data<F, A> Or<A>(Data<F, A> a, Data<F, A> b);

        public static Data<F, Types.List<A>> Some<A>(Data<F, A> v)
        {
            Func<Types.List<A>, Types.List<A>> cons(A a) =>
                list => Types.List<A>.Combine([a], list);

            Data<F, Types.List<A>> ManyV() => F.Or(SomeV(), F.Pure<Types.List<A>>([]));
            Data<F, Types.List<A>> SomeV() => F.LiftA2(cons, v, ManyV());

            return SomeV();
        }

        public static virtual Data<F, Types.List<A>> Many<A>(Data<F, A> a)
        {
            Func<Types.List<A>, Types.List<A>> cons(A a) => list => Types.List<A>.Combine([a], list);

            Data<F, Types.List<A>> ManyV() => F.Or(SomeV(), F.Pure<Types.List<A>>([]));
            Data<F, Types.List<A>> SomeV() => F.LiftA2(cons, a, ManyV());

            return ManyV();
        }
    }

    public interface MonadPlus<M> where M : Monad<M>, Alternative<M>
    {
        public static virtual Data<M, A> MZero<A>() =>
            M.Empty<A>();

        public static virtual Data<M, A> MPlus<A>(Data<M, A> a, Data<M, A> b) =>
            M.Or(a, b);
    }

    /// <summary>
    /// 'foldr', applied to a binary operator, a starting value (typically the right-identity of the operator), and a list, reduces the list
    /// using the binary operator, from right to left
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="f"></param>
    /// <param name="z"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    public static B FoldR<A, B>(Func<A, B, B> f, B z, Types.List<A> @as) =>
        @as switch
        {
        [] => z,
        [var x, .. var xs] => f(x, FoldR(f, z, xs))
        };

    public static Func<A, Types.List<A>, Types.List<A>> Cons<A>() =>
                (a, list) => Types.List<A>.Combine([a], list);

    /// <summary>
    /// A list producer that can be fused with 'foldr'.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="g"></param>
    /// <returns></returns>
    public static Types.List<A> Build<A>(Func<Func<A, Types.List<A>, Types.List<A>>, Types.List<A>, Types.List<A>> g)
        => g(Cons<A>(), []);

    /// <summary>
    /// A list producer that can be fused with 'foldr'.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="g"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    public static Types.List<A> Augment<A>(Func<Func<A, Types.List<A>, Types.List<A>>, Types.List<A>, Types.List<A>> g, Types.List<A> @as)
        => g(Cons<A>(), @as);

    /// <summary>
    /// Map(F, as) is the list obtained by applying f to each element of as
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="f"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    public static Types.List<B> Map<A, B>(Func<A, B> f, Types.List<A> @as) =>
        @as switch
        {
        [] => [],
        [var x, .. var xs] => [f(x), .. Map(f, xs)]
        };


    public static Func<A, Lst, Lst> MapFB<A, B, Elt, Lst>(Func<Elt, Lst, Lst> c, Func<A, Elt> f) =>
        (x, ys) => c(f(x), ys);

    // TODO : implement when C# "extension everything" => c# 13 -ish
    // instance (Monoid a, Monoid b) => Monoid (a,b) where
    //         mempty = (mempty, mempty)

    // -- | @since 4.9.0.0
    // instance (Semigroup a, Semigroup b, Semigroup c) => Semigroup (a, b, c) where
    //         (a,b,c) <> (a',b',c') = (a<>a',b<>b',c<>c')
    //         stimes n (a,b,c) = (stimes n a, stimes n b, stimes n c)

    // -- | @since 2.01
    // instance (Monoid a, Monoid b, Monoid c) => Monoid (a,b,c) where
    //         mempty = (mempty, mempty, mempty)

    // -- | @since 4.9.0.0
    // instance (Semigroup a, Semigroup b, Semigroup c, Semigroup d)
    //          => Semigroup (a, b, c, d) where
    //         (a,b,c,d) <> (a',b',c',d') = (a<>a',b<>b',c<>c',d<>d')
    //         stimes n (a,b,c,d) = (stimes n a, stimes n b, stimes n c, stimes n d)

    // -- | @since 2.01
    // instance (Monoid a, Monoid b, Monoid c, Monoid d) => Monoid (a,b,c,d) where
    //         mempty = (mempty, mempty, mempty, mempty)

    // -- | @since 4.9.0.0
    // instance (Semigroup a, Semigroup b, Semigroup c, Semigroup d, Semigroup e)
    //          => Semigroup (a, b, c, d, e) where
    //         (a,b,c,d,e) <> (a',b',c',d',e') = (a<>a',b<>b',c<>c',d<>d',e<>e')
    //         stimes n (a,b,c,d,e) =
    //             (stimes n a, stimes n b, stimes n c, stimes n d, stimes n e)

    // -- | @since 2.01
    // instance (Monoid a, Monoid b, Monoid c, Monoid d, Monoid e) =>
    //                 Monoid (a,b,c,d,e) where
    //         mempty = (mempty, mempty, mempty, mempty, mempty)


    // TODO : need to implement when C# "extension everything" => c# 13 -ish

    // instance Monoid a => Applicative ((,) a) where
    // pure x = (mempty, x)
    // (u, f) <*> (v, x) = (u <> v, f x)
    // liftA2 f (u, x) (v, y) = (u <> v, f x y)

    // instance Monoid a => Monad ((,) a) where
    // (u, a) >>= k = case k a of (v, b) -> (u <> v, b)


    // TODO : figure out how to do this in c#
    //     unsafeChr :: Int -> Char
    //     unsafeChr (I# i#) = C# (chr# i#)

    // -- | The 'Prelude.fromEnum' method restricted to the type 'Data.Char.Char'.
    //     ord :: Char -> Int
    // ord (C# c#) = I# (ord# c#)

    public static Bool EqString(String x, String y) =>
        (x, y) switch
        {
            ([], []) => new True(),
            ([var c1, .. var cs1], [var c2, .. var cs2]) =>
                (c1 == c2) switch
                {
                    true => EqString((String)cs1, (String)cs2),
                    false => new False()
                },
            (_, _) => new False()
        };

    public static Func<A, A> Id<A>() => a => a;

    /// <summary>
    /// Assertion function.  This simply ignores its boolean argument
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <returns></returns>
    public static Func<Bool, A, A> Assert<A>() =>
        (_, a) => a;

    public static Func<A, A> BreakPoint<A>() =>
        a => a;

    public static Func<Bool, A, A> BreakPointCond<A>() =>
        (_, a) => a;

    /// <summary>
    /// The const function is used to replace the second component of a pair, or the second argument to a function, with its first argument.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="a"></param>
    /// <param name="_"></param>
    /// <returns></returns>
    public static Func<A, Func<B, A>> Const<A, B>() => a => _ => a;

    public static Func<A, C> Compose<A, B, C>(Func<B, C> g, Func<A, B> f) => a => g(f(a));

    /// <summary>
    /// Flip(f) takes its (first) two arguments in the reverse order of f.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <param name="f"></param>
    /// <returns></returns>
    public static Func<B, Func<A, C>> Flip<A, B, C>(Func<A, Func<B, C>> f) => b => a => f(a)(b);

    /// <summary>
    /// Flip as a function delegate
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <typeparam name="C"></typeparam>
    /// <returns></returns>
    public static Func<Func<A, Func<B, C>>, Func<B, Func<A, C>>> Flip<A, B, C>() => f => b => a => f(a)(b);

    /// <summary>
    /// Function application
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    /// <param name="f"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static B Apply<A, B>(Func<A, B> f, A a) =>
        f(a);

    // -- | Strict (call-by-value) application operator. It takes a function and an
    // -- argument, evaluates the argument to weak head normal form (WHNF), then calls
    // -- the function with that value.

    // ($!) :: forall r a (b :: TYPE r). (a -> b) -> a -> b
    // {-# INLINE ($!) #-}
    // f $! x = let !vx = x in f vx  -- see #2273

    /// <summary>
    /// Until(p, f) yields the result of applying f until p holds.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="p"></param>
    /// <param name="f"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Func<A, A> Until<A>(Func<A, Bool> p, Func<A, A> f)
    {
        A Go(A x) => p(x) switch
        {
            True => x,
            False => Go(f(x)),
            _ => throw new NotImplementedException()
        };

        return Go;
    }

    /// <summary>
    /// A type-restricted version of Const. Its typing forces its first argument to have the same type as the second.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Func<A, Func<A, A>> AsTypeOf<A>(A a) =>
        Const<A, A>();

    public static IO<A> ReturnIO<A>(A x) =>
        new IO<A>(s => (s, x));

    public static IO<B> BindIO<A, B>(IO<A> m, Func<A, IO<B>> k) =>
        new IO<B>(s =>
        {
            var (new_s, a) = UnIO(m)(s);
            return UnIO(k(a))(new_s);
        });


    public static IO<B> ThenIO<A, B>(IO<A> a, IO<B> b) =>
        BindIO(a, _ => b);

    public static IO<A> FailIO<A>(String s) =>
        new IO<A>(_ => throw new Exception(s));

    public static Func<WorldState, (WorldState, A)> UnIO<A>(IO<A> a) =>
        a switch
        {
            IO<A>(var action) => action,
            _ => throw new NotImplementedException()
        };

    /// <summary>
    /// This GetTag method takes a value x of type object and returns its hash code, which can be used as a stand-in for a tag. 
    /// This is similar to the behavior of the getTag function in Haskell.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static int GetTag<A>(A a) =>
        a!.GetHashCode();

    /// <summary>
    /// A method that performs integer division of a by b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static Int QuotInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return new Int(a / b);
    }

    /// <summary>
    /// A method that calculates the remainder of integer division of a by b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static Int RemInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return new Int(a % b);
    }

    /// <summary>
    /// A method that performs integer division of a by b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static Int DivInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return new Int(a / b);
    }

    /// <summary>
    /// A method that calculates the modulus of integer division of a by b. 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static Int ModInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return new Int(a % b);
    }

    /// <summary>
    /// A method that performs integer division of a by b and calculates the remainder, returning both results as a tuple.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static (Int, Int) QuotRemInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return (new Int(a / b), new Int(a % b));
    }

    /// <summary>
    /// A method that performs integer division of a by b and calculates the modulus, returning both results as a tuple. 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    public static (Int, Int) DivModInt(Int a, Int b)
    {
        if (b.Value == 0)
            throw new DivideByZeroException();
        else
            return (new Int(a.Value / b.Value), new Int(a.Value % b.Value));
    }

    /// <summary>
    /// This ShiftMask method takes two int values m and b, and returns an int. 
    /// It uses the - operator and the < operator to perform its calculations. 
    /// This is similar to the behavior of the shift_mask function in Haskell.
    /// </summary>
    /// <param name="m"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Int ShiftMask(Int m, int b) =>
        -(b < m ? 1 : 0);

    /// <summary>
    /// This ShiftL method takes a uint value a and an int value b, performs a left shift operation on a by b bits, and then applies a mask to the result. 
    /// It uses the << and & operators to perform its calculations. This is similar to the behavior of the shiftL# function in Haskell.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Word ShiftL(Word a, int b) =>
        a << b & (uint)ShiftMask(sizeof(uint) * 8, b);

    /// <summary>
    /// This ShiftRL method takes a uint value a and an int value b, performs a right logical shift operation on a by b bits, and then applies a mask to the result. 
    /// It uses the >> and & operators to perform its calculations. 
    /// This is similar to the behavior of the shiftRL# function in Haskell.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Word ShiftRL(Word a, Int b) =>
        a >> b & (uint)ShiftMask(sizeof(uint) * 8, b);

    /// <summary>
    /// This IShiftL method takes an int value a and an int value b, performs a left shift operation on a by b bits, and then applies a mask to the result. 
    /// It uses the << and & operators to perform its calculations. 
    /// This is similar to the behavior of the iShiftL# function in Haskell.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Int IShiftL(Int a, Int b) =>
        a << b & ShiftMask(sizeof(int) * 8, b);

    /// <summary>
    /// This IShiftRA method takes an int value a and an int value b, checks if b is greater than or equal to the word size in bits, and returns either -1 or 0 depending on the sign of a if it is. Otherwise, it performs a right arithmetic shift operation on a by b bits. 
    /// It uses the >=, <, and >> operators to perform its calculations. 
    /// This is similar to the behavior of the iShiftRA# function in Haskell.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Int IShiftRA(Int a, Int b)
    {
        if (b >= sizeof(int) * 8)
            return a < 0 ? -1 : 0;
        else
            return a >> b;
    }

    /// <summary>
    /// This IShiftRL method takes an int value a and an int value b, performs a right logical shift operation on a by b bits, and then applies a mask to the result. 
    /// It uses the >> and & operators to perform its calculations. 
    /// This is similar to the behavior of the iShiftRL# function in Haskell.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Word IShiftRL(Int a, Int b) =>
        (uint)a >> b & (uint)ShiftMask(sizeof(int) * 8, b);

}
