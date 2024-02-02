namespace Lambdaba;

public interface Num<A>
    where A : Num<A>
{
    public static abstract A operator -(A x, A y);

    public static abstract A operator *(A x, A y);

    public static abstract A operator -(A x);

    public static abstract A operator +(A x);

    public static abstract A Add(A x, A y);

    public static virtual A Negate(A x) =>
        A.Add(A.FromInteger(0), A.Negate(x));

    public static abstract A Abs(A a);
    public static abstract A Signum(A a);
    public static abstract A FromInteger(int a);
}
