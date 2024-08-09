/// <summary>
/// The namespace for the Lambdaba library.
/// </summary>
namespace Lambdaba;

/// <summary>
/// Represents a data type with a type constructor.
/// </summary>
/// <typeparam name="TypeConstructor">The type constructor.</typeparam>
public interface Data<TypeConstructor>;

/// <summary>
/// Represents a data type with a type constructor and one type parameter.
/// </summary>
/// <typeparam name="TypeConstructor">The type constructor.</typeparam>
/// <typeparam name="A">The first type parameter.</typeparam>
public interface Data<TypeConstructor, A>;

/// <summary>
/// Represents a data type with a type constructor and two type parameters.
/// </summary>
/// <typeparam name="TypeConstructor">The type constructor.</typeparam>
/// <typeparam name="A">The first type parameter.</typeparam>
/// <typeparam name="B">The second type parameter.</typeparam>
public interface Data<TypeConstructor, A, B>;

/// <summary>
/// Represents a data type with a type constructor and three type parameters.
/// </summary>
/// <typeparam name="TypeConstructor"></typeparam>
/// <typeparam name="A">The first type parameter.</typeparam>
/// <typeparam name="B">The second type parameter.</typeparam>
/// <typeparam name="C">The third type parameter.</typeparam>
public interface Data<TypeConstructor, A, B, C>;
