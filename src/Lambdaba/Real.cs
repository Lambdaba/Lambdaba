namespace Lambdaba;

public record Ratio<A>
{
    public A Numerator { get; }
    public A Denominator { get; }

    public Ratio(A numerator, A denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }
}

public record Rational : Ratio<Integer>
{
    public Rational(Integer numerator, Integer denominator) : base(numerator, denominator)
    {
    }
}

public interface Real<A> : Num<A>, Prelude.Ord<A>
    where A : Real<A>
{
    public static abstract Rational ToRational(A x);
}

public interface Integral<A> : Real<A>, Enum<A>
    where A : Integral<A>
{
    public static abstract Integer ToInteger(A x);
    public static virtual A Quot(A n, A d)
    {
        var (q, _) = A.QuotRem(n, d);
        return q;
    }

    public static virtual A Rem(A n, A d)
    {
        var (_, r) = A.QuotRem(n, d);
        return r;
    }

    public static virtual A Div(A n, A d)
    {
        var (q, _) = A.DivMod(n, d);
        return q;
    }
    public static virtual A Mod(A n, A d)
    {
        var (_, r) = A.DivMod(n, d);
        return r;
    }

    public static abstract (A, A) QuotRem(A n, A d);
    public static virtual (A, A) DivMod(A n, A d)
    {
        var (q, r) = A.QuotRem(n, d);
        if(A.Signum(r).Equals(A.Negate(A.Signum(d))))
        {
            return (A.Add(q, A.Negate(A.FromInteger(1))), A.Add(r, d));
        }
        return (q, r);
    }
    
}