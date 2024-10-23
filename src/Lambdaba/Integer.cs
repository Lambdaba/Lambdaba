using System;
using System.Numerics;

namespace Lambdaba;

public abstract record Integer :
    Data<Integer>,
    Enum<Integer>
{
    public static Base.Int FromEnum(Integer x) => 
        x switch
        {
            IS(var value) => value,
            IP(var value) => (Base.Int)value,
            IN(var value) => (Base.Int)value,
            _ => throw new NotSupportedException("Unlikely")
        };
       
    public static Integer ToEnum(Base.Int x) => throw new NotImplementedException();

    public static Integer One => new IS(1);

    public static implicit operator Integer(int v)
     => new IS(v);
}
   

public record IS(int Value) : Integer;

public record IP(BigInteger Value) : Integer;

public record IN(BigInteger Value) : Integer;
