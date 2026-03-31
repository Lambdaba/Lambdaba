namespace System.Runtime.CompilerServices;

/// <summary>
/// Marks a type as a union type for compiler support of union conversions,
/// pattern matching, and exhaustiveness checks.
/// Required for .NET 11 Preview 2 (not yet included in the runtime).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute;

/// <summary>
/// Implemented by all union types. Provides access to the wrapped value.
/// </summary>
public interface IUnion
{
    object? Value { get; }
}
