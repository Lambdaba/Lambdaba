/// <summary>
/// Primitive ad-hoc union types for arities 2–8, declared with the C# 15 <c>union</c> keyword.
/// </summary>
/// <remarks>
/// <para>
/// The C# 15 compiler lowers each <c>union</c> declaration into a struct that implements
/// <see cref="System.Runtime.CompilerServices.IUnion"/>. Ctors, implicit conversions from each
/// case type, and the <c>Value</c> property are all compiler-emitted — no generator or
/// hand-written boilerplate is needed here.
/// </para>
/// <para>
/// <strong>Union&lt;X, X&gt; instantiation collision.</strong>
/// When all type arguments are the same concrete type (e.g. <c>Union&lt;int, int&gt;</c>),
/// the lowered constructor signatures collapse: both <c>Union(T1)</c> and <c>Union(T2)</c>
/// become identical <c>Union(int)</c> overloads, and the implicit conversions become ambiguous.
/// The C# 15 specification provides no mitigation for this at runtime; it is a compile-time
/// constraint. <c>Union&lt;X, X&gt;</c> (and by extension any arity where two or more type
/// arguments resolve to the same type) cannot be instantiated. This limitation is recorded in
/// ADR-0001.
/// </para>
/// </remarks>

namespace Lambdaba;

/// <summary>A union of two distinct types.</summary>
public union Union<T1, T2>(T1, T2);

/// <summary>A union of three distinct types.</summary>
public union Union<T1, T2, T3>(T1, T2, T3);

/// <summary>A union of four distinct types.</summary>
public union Union<T1, T2, T3, T4>(T1, T2, T3, T4);

/// <summary>A union of five distinct types.</summary>
public union Union<T1, T2, T3, T4, T5>(T1, T2, T3, T4, T5);

/// <summary>A union of six distinct types.</summary>
public union Union<T1, T2, T3, T4, T5, T6>(T1, T2, T3, T4, T5, T6);

/// <summary>A union of seven distinct types.</summary>
public union Union<T1, T2, T3, T4, T5, T6, T7>(T1, T2, T3, T4, T5, T6, T7);

/// <summary>A union of eight distinct types.</summary>
public union Union<T1, T2, T3, T4, T5, T6, T7, T8>(T1, T2, T3, T4, T5, T6, T7, T8);
