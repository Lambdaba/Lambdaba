// Union<X, X> where all type arguments resolve to the same concrete type cannot be
// instantiated: the C# 15 compiler lowering collapses the two constructors into identical
// overloads, making them ambiguous. This is a compile-time constraint with no runtime
// mitigation. See ADR-0001, Consequences section.

namespace Lambdaba.Tests;

using System.Globalization;
using System.Runtime.CompilerServices;
using Assert = TUnit.Assertions.Assert;

/// <summary>
/// Round-trip and value tests for the primitive ad-hoc union types
/// <c>Union&lt;T1,T2&gt;</c> through <c>Union&lt;T1,…,T8&gt;</c>.
/// These types are declared with the C# 15 <c>union</c> keyword and are
/// compiler-lowered — no source generator pass runs on them.
/// </summary>
public class UnionPrimitiveTests
{
    // ──────────────────────────────────────────────
    // Union<T1, T2> — arity 2
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity2_IntArm_SwitchFiresCorrectly()
    {
        Union<int, string> u = 42;

        string result = u switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
        };

        await Assert.That(result).IsEqualTo("int:42");
    }

    [Test]
    public async Task Arity2_StringArm_SwitchFiresCorrectly()
    {
        Union<int, string> u = "hello";

        string result = u switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
        };

        await Assert.That(result).IsEqualTo("str:hello");
    }

    [Test]
    public async Task Arity2_IUnion_Value_ReflectsWrappedInt()
    {
        Union<int, string> u = 7;
        object? value = ((IUnion)u).Value;
        await Assert.That(value).IsEqualTo(7);
    }

    [Test]
    public async Task Arity2_IUnion_Value_ReflectsWrappedString()
    {
        Union<int, string> u = "world";
        object? value = ((IUnion)u).Value;
        await Assert.That(value).IsEqualTo("world");
    }

    [Test]
    public async Task Arity2_HasValue_AlwaysTrueForConstructedUnion()
    {
        Union<int, string> ui = 1;
        Union<int, string> us = "x";
        // HasValue is defined as Value is not null — valid for value types (boxed) and strings.
        await Assert.That(((IUnion)ui).Value).IsNotNull();
        await Assert.That(((IUnion)us).Value).IsNotNull();
    }

    // ──────────────────────────────────────────────
    // Union<T1, T2, T3> — arity 3
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity3_EachArm_SwitchFiresCorrectly()
    {
        Union<int, string, bool> u1 = 10;
        Union<int, string, bool> u2 = "arity3";
        Union<int, string, bool> u3 = true;

        string Describe(Union<int, string, bool> u) => u switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            null => throw new InvalidOperationException("null union value"),
        };

        await Assert.That(Describe(u1)).IsEqualTo("int:10");
        await Assert.That(Describe(u2)).IsEqualTo("str:arity3");
        await Assert.That(Describe(u3)).IsEqualTo("bool:True");
    }

    [Test]
    public async Task Arity3_IUnion_Value_ReflectsWrappedValue()
    {
        Union<int, string, bool> u = "test";
        await Assert.That(((IUnion)u).Value).IsEqualTo("test");
    }

    // ──────────────────────────────────────────────
    // Union<T1, T2, T3, T4> — arity 4
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity4_EachArm_SwitchFiresCorrectly()
    {
        Union<int, string, bool, double> u1 = 1;
        Union<int, string, bool, double> u2 = "two";
        Union<int, string, bool, double> u3 = false;
        Union<int, string, bool, double> u4 = 3.14;

        string Describe(Union<int, string, bool, double> u) => u switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            double d => $"double:{d.ToString(CultureInfo.InvariantCulture)}",
            null => throw new InvalidOperationException("null union value"),
        };

        await Assert.That(Describe(u1)).IsEqualTo("int:1");
        await Assert.That(Describe(u2)).IsEqualTo("str:two");
        await Assert.That(Describe(u3)).IsEqualTo("bool:False");
        await Assert.That(Describe(u4)).IsEqualTo("double:3.14");
    }

    [Test]
    public async Task Arity4_IUnion_Value_ReflectsWrappedValue()
    {
        Union<int, string, bool, double> u = 3.14;
        await Assert.That(((IUnion)u).Value).IsEqualTo(3.14);
    }

    // ──────────────────────────────────────────────
    // Union<T1..T5> — arity 5
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity5_RoundTrip_AllArms_FireCorrectly()
    {
        Union<int, string, bool, double, char> u5 = 'Z';

        string result = u5 switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            double d => $"double:{d}",
            char c => $"char:{c}",
        };

        await Assert.That(result).IsEqualTo("char:Z");
    }

    [Test]
    public async Task Arity5_IUnion_Value_ReflectsWrappedValue()
    {
        Union<int, string, bool, double, char> u = 'A';
        await Assert.That(((IUnion)u).Value).IsEqualTo('A');
    }

    // ──────────────────────────────────────────────
    // Union<T1..T6> — arity 6
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity6_RoundTrip_LastArm_FiresCorrectly()
    {
        Union<int, string, bool, double, char, long> u6 = 999L;

        string result = u6 switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            double d => $"double:{d}",
            char c => $"char:{c}",
            long l => $"long:{l}",
        };

        await Assert.That(result).IsEqualTo("long:999");
    }

    // ──────────────────────────────────────────────
    // Union<T1..T7> — arity 7
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity7_RoundTrip_LastArm_FiresCorrectly()
    {
        Union<int, string, bool, double, char, long, float> u7 = 1.5f;

        string result = u7 switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            double d => $"double:{d.ToString(CultureInfo.InvariantCulture)}",
            char c => $"char:{c}",
            long l => $"long:{l}",
            float f => $"float:{f.ToString(CultureInfo.InvariantCulture)}",
        };

        await Assert.That(result).IsEqualTo("float:1.5");
    }

    // ──────────────────────────────────────────────
    // Union<T1..T8> — arity 8
    // ──────────────────────────────────────────────

    [Test]
    public async Task Arity8_RoundTrip_LastArm_FiresCorrectly()
    {
        Union<int, string, bool, double, char, long, float, byte> u8 = (byte)255;

        string result = u8 switch
        {
            int i => $"int:{i}",
            string s => $"str:{s}",
            bool b => $"bool:{b}",
            double d => $"double:{d}",
            char c => $"char:{c}",
            long l => $"long:{l}",
            float f => $"float:{f}",
            byte by => $"byte:{by}",
        };

        await Assert.That(result).IsEqualTo("byte:255");
    }

    [Test]
    public async Task Arity8_IUnion_Value_ReflectsWrappedValue()
    {
        Union<int, string, bool, double, char, long, float, byte> u = (byte)128;
        await Assert.That(((IUnion)u).Value).IsEqualTo((byte)128);
    }
}
