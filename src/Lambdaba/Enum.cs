global using ShowS = System.Func<Lambdaba.Base.String, Lambdaba.Base.String>;
using static Lambdaba.Base;
using static Lambdaba.Types;
using Char = Lambdaba.Base.Char;
using String = Lambdaba.Base.String;

namespace Lambdaba;

/// <summary>
/// Is used to name the upper and lower limits of a type. 'Ord' is not a base type of 'Bounded' since types that are not 
/// totally ordered may also have upper and lower bounds.
/// </summary>
/// <typeparam name="A"></typeparam>
public interface Bounded<out A>
    where A : Bounded<A>
{
    static abstract A MinBound { get; }
    static abstract A MaxBound { get; }
}

/// <summary>
/// Defines operations on sequentially ordered types.
/// </summary>
/// <typeparam name="A"></typeparam>
public interface Enum<A>
    where A : Enum<A>
{
    static Func<int, A> ToEnum() => x => A.ToEnum(x);
    static abstract A ToEnum(Int x);

    /// <summary>
    /// Takes a value of some type a that is an instance of the Enum type class, and returns an Int.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    static abstract Int FromEnum(A x);

    /// <summary>
    /// Returns the successor of a value. For numeric types, succ adds 1.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    static virtual A Succ(A x) => A.ToEnum(A.FromEnum(x) + 1);

    /// <summary>
    /// Returns the predecessor of a value. For numeric types, pred subtracts 1.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    static virtual A Pred(A x) => A.ToEnum(A.FromEnum(x) - 1);

    /// <summary>
    /// Function takes a value of some type A that is an instance of the Enum type class, and returns an infinite list of values of type A. 
    /// The list starts from the input value and increments by 1 for each subsequent element.
    /// IMPORTANT: Use LINQ methods like Take to get a finite number of elements from the sequence.
    /// In C# lists are not lazy, so you will get an infinite loop if you try to iterate over the entire list.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Types.List<A> EnumFrom(A x) =>
        Map(ToEnum(), [.. Infinite(A.FromEnum(x))]);

    /// <summary>
    /// The enumFromThen function is used to generate a list of elements from an enumerated type, 
    /// starting with the specified element and incrementing by one until the second specified element is reached.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Types.List<A> EnumFromThen(A x, A y) =>
        Map(ToEnum(), [A.FromEnum(x), .. Infinite(A.FromEnum(y))]);

    /// <summary>
    /// Generates an infinite sequence of integers starting from the given value
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    static IEnumerable<Int> Infinite(Int start)
    {
        Int current = start;
        while (Int.MaxBound > current)
        {
            yield return current;
            current++;
        }
    }

    /// <summary>
    /// Is a function that generates an inclusive list of elements from an enumerated type, 
    /// starting with the first specified element and ending with the second specified element
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Types.List<A> EnumFromTo(A x, A y) =>
        Map(ToEnum(), [.. Enumerable.Range(A.FromEnum(x), A.FromEnum(y))]);

    /// <summary>
    /// Similar to the enumFromThen function, but it takes an additional argument that specifies the maximum number of elements to generate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Types.List<A> EnumFromThenTo(A x, A y, A z) =>
        Map(ToEnum(), [A.FromEnum(x), .. Enumerable.Range(A.FromEnum(y), A.FromEnum(z))]);
}

public static class Enum
{
    /// <summary>
    /// Works similarly to the EnumFromTo function. 
    /// However, the BoundedEnumFrom function does not require the user to specify the last element of the list because it knows the last element of the list from the Bounded class
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Types.List<A> BoundedEnumFrom<A>(A x) where A : Enum<A>, Bounded<A> =>
        Map(Enum<A>.ToEnum(), [.. Enumerable.Range(A.FromEnum(x), A.FromEnum(A.MaxBound))]);

    /// <summary>
    /// Is a function that generates an inclusive list of elements from an enumerated type, 
    /// starting with the specified element and ending with the last element of the type.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Types.List<A> BoundedEnumFromThen<A>(A x, A y) 
        where A : Enum<A>, Bounded<A> =>
        (x, y) switch
        {
            (A a, A b) when A.FromEnum(a) >= A.FromEnum(b) => 
                Map(Enum<A>.ToEnum(), [A.FromEnum(a), A.FromEnum(b), .. Enumerable.Range(A.FromEnum(x), A.FromEnum(A.MaxBound))]),
            _ => Map(Enum<A>.ToEnum(), [A.FromEnum(x), A.FromEnum(y), .. Enumerable.Range(A.FromEnum(x), A.FromEnum(A.MinBound))])
        };

    public static B ToEnumError<A, B>(String inst_ty, Int x, (A, A) bnds) 
        where A : Show<A> => 
            throw new Exception($"Enum.toEnum{inst_ty}: tag ({x}) is outside of bounds {A.ShowList([bnds.Item1, bnds.Item2])}");

    public static B FromEnumError<A, B>(String inst_ty, A x) 
        where A : Show<A> => 
            throw new Exception($"Enum.fromEnum{inst_ty}: value ({A.show(x)}) is outside of Int's bounds ({Int.MinBound}, {Int.MaxBound})");

    public static B SuccError<A, B>(String inst_ty, A x) 
        where A : Show<A> =>
            throw new Exception($"Enum.succ{inst_ty}: tried to take `succ' of maxBound");

    public static B PredError<A, B>(String inst_ty, A x) 
        where A : Show<A> =>
            throw new Exception($"Enum.pred{inst_ty}: tried to take `pred' of minBound");

    public static A EftCharFB<A>(Func<Char, A, A> c, A n, Int x0, Int y) 
        where A : Enum<A> =>
            x0 > y 
                ? n 
                : c(Lambdaba.Data.Char.Chr(x0), EftCharFB(c, n, x0 + 1, y));

    public static String EftChar(Int x, Int y)=>
        x > y 
            ? [] 
            : [Lambdaba.Data.Char.Chr(x), .. EftChar(x + 1, y)];


    public static A EfdCharFB<A>(Func<Char, A, A> c, A n, Int x1, Int x2) 
        where A : Enum<A>
        {
            var delta = x2 - x1;
            return delta >= 0 
                ? GoUpCharFB(c, n, x1, delta, Lambdaba.Data.Char.Ord(Char.MaxBound)) 
                : GoDnCharFB(c, n, x1, delta, Lambdaba.Data.Char.Ord(Char.MinBound));
        }

    public static String EfdtChar(Int x1, Int x2, Int lim)
    {
        var delta = x2 - x1;
        return delta >= 0 
            ? GoUpCharList(x1, delta, lim) 
            : GoDnCharList(x1, delta, lim);
    }
       

    public static String EfdChar(Int x, Int y)
    {
        var delta = y - x;
        return (delta >= 0) switch
        {
            true => GoUpCharList(x, delta, Lambdaba.Data.Char.Ord(Char.MaxBound)),
            false => GoDnCharList(x, delta, Lambdaba.Data.Char.Ord(Char.MinBound))
        };
    } 

    private static A GoUpCharFB<A>(Func<Char, A, A> c, A n, Int x1, Int delta, Int lim) 
        where A : Enum<A> => 
            x1 > lim 
                ? n 
                : c(Lambdaba.Data.Char.Chr(x1), GoUpCharFB(c, n, x1 + delta, delta, lim));

    private static A GoDnCharFB<A>(Func<Char, A, A> c, A n, Int x1, Int delta, Int lim)
        where A : Enum<A> =>
            x1 < lim 
                ? n 
                : c(Lambdaba.Data.Char.Chr(x1), GoDnCharFB(c, n, x1 + delta, delta, lim));
    

    public static String GoUpCharList(Int x0, Int delta, Int lim) =>
        x0 > lim 
            ? [] 
            : [Lambdaba.Data.Char.Chr(x0), .. GoUpCharList(x0 + delta, delta, lim)];

    public static String GoDnCharList(Int x0, Int delta, Int lim) =>
        x0 < lim 
            ? [] 
            : [Lambdaba.Data.Char.Chr(x0), .. GoDnCharList(x0 + delta, delta, lim)];
    public static Types.List<Int> EftInt(Int x0, Int y)
    {
        return x0 > y 
            ? [] 
            : Go(x0);

        Types.List<Int> Go(Int x) =>
            x > y 
                ? [] 
                : [x, .. Go(x + 1)];

    }

    public static R EftIntFB<R>(Func<Int, R, R> c, R n, Int x0, Int y)
    {
        return x0 > y 
            ? n 
            : Go(x0);

        R Go(Int x) =>
            x > y 
                ? n 
                : c(x, Go(x + 1));
    }

    public static Types.List<Int> EfdInt(Int x1, Int x2) => 
        x2 >= x1 
            ? EfdtIntUp(x1, x2, Int.MaxBound)
            : EfdtIntDn(x1, x2, Int.MinBound);

    public static Types.List<Int> EfdtInt(Int x1, Int x2, Int y) =>
        x2 >= x1 
            ? EfdtIntUp(x1, x2, y) 
            : EfdtIntDn(x1, x2, y); 

    public static Types.List<Int> EfdtIntUp(Int x1, Int x2, Int y)
    {
        Int delta = x2 - x1;
        Int yPrime = y - delta;

        return y < x2 
            ? y < x1 
                ? [] 
                : [x1] 
            : [x1, .. GoUp(x1 + delta)];

        Types.List<Int> GoUp(Int x) =>
            x > yPrime 
                ? [x] 
                : [x, .. GoUp(x + delta)];
    }

    public static R EfdtIntUpFB<R>(Func<Int, R, R> c, R n, Int x1, Int x2, Int y)
    {
        Int delta = x2 - x1;
        Int yPrime = y - delta;

        return y < x2 
            ? y < x1 
                ? n 
                : c(x1, n) 
            : c(x1, GoUp(x1 + delta));

        R GoUp(Int x) =>
            x > yPrime 
                ? n 
                : c(x, GoUp(x + delta));
    }

    public static Types.List<Int> EfdtIntDn(Int x1, Int x2, Int y)
    {
        Int delta = x2 - x1;
        Int yPrime = y - delta;

        return y > x2 
            ? y > x1 
                ? [] 
                : [x1] 
            : [x1, .. GoDn(x1 + delta)];

        Types.List<Int> GoDn(Int x) =>
            x < yPrime 
                ? [x] 
                : [x, .. GoDn(x + delta)];
    }

    public static R EfdtIntDnFB<R>(Func<Int, R, R> c, R n, Int x1, Int x2, Int y)
    {
        Int delta = x2 - x1;
        Int yPrime = y - delta;

        return y > x2 
            ? y > x1 
                ? n 
                : c(x1, n) 
            : c(x1, GoDn(x1 + delta));

        R GoDn(Int x) =>
            x < yPrime 
                ? n 
                : c(x, GoDn(x + delta));
    }

    public static Types.List<Word> EftWord(Word x0, Word y)
    {
        return x0 > y 
            ? [] 
            : Go(x0);

        Types.List<Word> Go(Word x) =>
            x == y 
                ? [] 
                : [x, .. Go(x + 1)];
    }

    public static Types.List<Word> EfdWord(Word x1, Word x2) => 
        x2 >= x1
            ? EfdtWordUp(x1, x2, Word.MaxBound)
            : EfdtWordDn(x1, x2, Word.MinBound);

    public static Types.List<Word> EfdtWord(Word x1, Word x2, Word y) => 
        x2 >= x1
            ? EfdtWordUp(x1, x2, y) 
            : EfdtWordDn(x1, x2, y);

    public static R EfdtWordFB<R>(Func<Word, R, R> c, R n, Word x1, Word x2, Word y) =>
        x2 >= x1
            ? EfdtWordUpFB(c, n, x1, x2, y) 
            : EfdtWordDnFB(c, n, x1, x2, y);

    public static R EfdtWordUpFB<R>(Func<Word, R, R> c, R? n, Word x1, Word x2, Word y)
    {
        var delta = x2 - x1;
        var yPrime = y - delta;

        return y < x2 
            ? y < x1 
                ? n 
                : c(x1, n) 
            : c(x1, GoUp(x1 + delta));

        R GoUp(Word x) =>
            x > yPrime 
                ? n 
                : c(x, GoUp(x + delta));
    }
    
    public static R EfdtWordDnFB<R>(Func<Word, R, R> c, R? n, Word x1, Word x2, Word y)
    {
        var delta = x2 - x1;
        var yPrime = y - delta;

        return y > x2 
            ? y > x1 
                ? n 
                : c(x1, n) 
            : c(x1, GoDn(x1 + delta));

        R GoDn(Word x) =>
            x < yPrime 
                ? n 
                : c(x, GoDn(x + delta));
    }
        
    public static Word MaxIntWord => 
        (Word)Int.MaxBound;

    public static Types.List<Word> EfdtWordDn(Word x1, Word x2, Word minBound)
    {
        var delta = x2 - x1;
        var yPrime = minBound - delta;

        return x1 > minBound 
            ? x1 > x2 
                ? [] 
                : [x1] 
            : [x1, .. GoDn(x1 + delta)];

        Types.List<Word> GoDn(Word x) =>
            x < yPrime 
                ? [x] 
                : [x, .. GoDn(x + delta)];
    }

    public static Types.List<Word> EfdtWordUp(Word x1, Word x2, Word maxBound)
    {
        var delta = x2 - x1;
        var yPrime = maxBound - delta;

        return x1 < maxBound 
            ? x1 < x2 
                ? [] 
                : [x1] 
            : [x1, .. GoUp(x1 + delta)];

        Types.List<Word> GoUp(Word x) =>
            x > yPrime 
                ? [x] 
                : [x, .. GoUp(x + delta)];
    }


    // TODO:
    //         ------------------------------------------------------------------------
    // -- Tuples
    // ------------------------------------------------------------------------

    // -- | @since 2.01
    // deriving instance Bounded ()

    // -- | @since 2.01
    // instance Enum () where
    //     succ _      = errorWithoutStackTrace "Prelude.Enum.().succ: bad argument"
    //     pred _      = errorWithoutStackTrace "Prelude.Enum.().pred: bad argument"

    //     toEnum x | x == 0    = ()
    //              | otherwise = errorWithoutStackTrace "Prelude.Enum.().toEnum: bad argument"

    //     fromEnum () = 0
    //     enumFrom ()         = [()]
    //     enumFromThen () ()  = let many = ():many in many
    //     enumFromTo () ()    = [()]
    //     enumFromThenTo () () () = let many = ():many in many

    // instance Enum a => Enum (Solo a) where
    //     succ (MkSolo a) = MkSolo (succ a)
    //     pred (MkSolo a) = MkSolo (pred a)

    //     toEnum x = MkSolo (toEnum x)

    //     fromEnum (MkSolo x) = fromEnum x
    //     enumFrom (MkSolo x) = [MkSolo a | a <- enumFrom x]
    //     enumFromThen (MkSolo x) (MkSolo y) =
    //       [MkSolo a | a <- enumFromThen x y]
    //     enumFromTo (MkSolo x) (MkSolo y) =
    //       [MkSolo a | a <- enumFromTo x y]
    //     enumFromThenTo (MkSolo x) (MkSolo y) (MkSolo z) =
    //       [MkSolo a | a <- enumFromThenTo x y z]

    // deriving instance Bounded a => Bounded (Solo a)
    // -- Report requires instances up to 15
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b)
    //         => Bounded (a,b)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c)
    //         => Bounded (a,b,c)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d)
    //         => Bounded (a,b,c,d)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e)
    //         => Bounded (a,b,c,d,e)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f)
    //         => Bounded (a,b,c,d,e,f)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g)
    //         => Bounded (a,b,c,d,e,f,g)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h)
    //         => Bounded (a,b,c,d,e,f,g,h)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i)
    //         => Bounded (a,b,c,d,e,f,g,h,i)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j, Bounded k)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j,k)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j, Bounded k,
    //           Bounded l)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j,k,l)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j, Bounded k,
    //           Bounded l, Bounded m)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j,k,l,m)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j, Bounded k,
    //           Bounded l, Bounded m, Bounded n)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j,k,l,m,n)
    // -- | @since 2.01
    // deriving instance (Bounded a, Bounded b, Bounded c, Bounded d, Bounded e,
    //           Bounded f, Bounded g, Bounded h, Bounded i, Bounded j, Bounded k,
    //           Bounded l, Bounded m, Bounded n, Bounded o)
    //         => Bounded (a,b,c,d,e,f,g,h,i,j,k,l,m,n,o)
    // }  


}

public static class Show
{
    public static ShowS ShowS<A>(A a) where A : Show<A> => 
        A.ShowPrec(0, a);

    public static ShowS ShowList__<A>(Func<A, ShowS> showX, Types.List<A> xs) where A : Show<A> =>
        s => 
        {
            String ShowL(Types.List<A> ys) =>
                ys switch
                {
                    [] => ']' + s,
                    [var head, .. var tail] => 
                        ',' + showX(head)(ShowL(tail)),
                    _ => throw new NotSupportedException()
                };

            return xs switch
            {
                [] => "[]" + s,
                [var head, .. var tail] => 
                    '[' + showX(head)(ShowL(tail)),
                _ => throw new NotSupportedException()
            }; 
        };
            

}

public interface Show<A> 
        where A : Show<A>
{
    public static virtual ShowS ShowPrec(Int p, A x) =>
        s =>
            (String)(A.show(x) + s);

    public static virtual String show(A a) =>
        Show.ShowS(a)("");


    public static virtual ShowS ShowList(Types.List<A> xs) =>
        s => 
            Show.ShowList__(Show.ShowS, xs)(s);
}


