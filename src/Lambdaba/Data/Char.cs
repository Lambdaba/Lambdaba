﻿using static Lambdaba.Base;

namespace Lambdaba.Data;

public static class Char
{
    public static Int Ord(Base.Char c) => 
        (int)c;

    public static Base.Char Chr(Int i) => 
        (char)(int)i;
}
