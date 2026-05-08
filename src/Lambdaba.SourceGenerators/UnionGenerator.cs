using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdaba.SourceGenerators;

[Generator]
public sealed class UnionGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor Lmbd001 = new(
        id: "LMBD001",
        title: "User-declared member shadows generated union helper",
        messageFormat: "User-declared '{0}' on '{1}' shadows the generated union helper; the generated helper has been suppressed",
        category: "Lambdaba.SourceGenerators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var unions = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "System.Runtime.CompilerServices.UnionAttribute",
                predicate: static (n, _) => n is ClassDeclarationSyntax cds
                    && cds.Modifiers.Any(SyntaxKind.PartialKeyword),
                transform: static (ctx, ct) => ToModel(ctx, ct))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(unions!, static (spc, m) => Emit(spc, m!));
    }

    // ------------------------------------------------------------------ model

    private sealed record TypeParameterModel(string Name, string Constraints);

    private sealed record CaseModel(string FullyQualifiedType, string SimpleName);

    private sealed record BrandInfo(
        string FullyQualifiedTypeName,
        string SimpleName,
        string Namespace,
        string DataInterfaceFqn,
        ImmutableArray<string> BrandTyParamNames);

    private sealed record UnionModel(
        string Namespace,
        string TypeName,
        string FullyQualifiedTypeName,
        ImmutableArray<TypeParameterModel> TyParams,
        string ConstraintClauses,
        ImmutableArray<CaseModel> Cases,
        BrandInfo? Brand,
        ImmutableArray<string> ExistingMembers);

    // ------------------------------------------------------------------ pipeline transform

    private static UnionModel? ToModel(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        ct.ThrowIfCancellationRequested();

        // Collect type parameters
        var tyParams = typeSymbol.TypeParameters
            .Select(tp =>
            {
                var constraints = BuildConstraintClause(tp);
                return new TypeParameterModel(tp.Name, constraints);
            })
            .ToImmutableArray();

        // Build constraint clauses string (where T : ...)
        var constraintClauses = BuildAllConstraintClauses(typeSymbol.TypeParameters);

        // Discover cases: public/internal single-param ctors, not delegating
        var cases = DiscoverCases(typeSymbol);
        if (cases.IsEmpty)
            return null;

        // Discover HKT brand
        var brand = DiscoverBrand(typeSymbol);

        // Collect existing member names for conflict detection
        var existingMembers = typeSymbol
            .GetMembers()
            .Select(m => m.Name)
            .Distinct()
            .ToImmutableArray();

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : typeSymbol.ContainingNamespace.ToDisplayString();

        var fqTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new UnionModel(
            Namespace: ns,
            TypeName: typeSymbol.Name,
            FullyQualifiedTypeName: fqTypeName,
            TyParams: tyParams,
            ConstraintClauses: constraintClauses,
            Cases: cases,
            Brand: brand,
            ExistingMembers: existingMembers);
    }

    // ------------------------------------------------------------------ case discovery

    private static ImmutableArray<CaseModel> DiscoverCases(INamedTypeSymbol typeSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<CaseModel>();

        foreach (var ctor in typeSymbol.Constructors)
        {
            if (ctor.Parameters.Length != 1)
                continue;
            if (ctor.DeclaredAccessibility == Accessibility.Private)
                continue;

            var param = ctor.Parameters[0];
            if (param.RefKind is RefKind.Out or RefKind.Ref)
                continue;

            // Skip delegating ctors (: this(...)) — we can't detect these from symbols alone,
            // but we can check the syntax for an initializer.
            if (ctor.DeclaringSyntaxReferences.Length > 0)
            {
                var syntaxRef = ctor.DeclaringSyntaxReferences[0];
                if (syntaxRef.GetSyntax() is ConstructorDeclarationSyntax ctorSyntax
                    && ctorSyntax.Initializer is { ThisOrBaseKeyword.RawKind: (int)SyntaxKind.ThisKeyword })
                {
                    continue;
                }
            }

            var caseType = param.Type;
            var fqType = caseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            // Roslyn's Name strips generic arity: Just<A>.Name == "Just", Nothing.Name == "Nothing"
            var simpleName = caseType.Name;

            builder.Add(new CaseModel(fqType, simpleName));
        }

        return builder.ToImmutable();
    }

    // ------------------------------------------------------------------ brand discovery

    private static BrandInfo? DiscoverBrand(INamedTypeSymbol typeSymbol)
    {
        // Find the Data<F, A> interface the union type implements — its FQN gives us the correct namespace for Data
        var dataIfaceFqn = FindDataInterfaceFqn(typeSymbol);
        if (dataIfaceFqn is null)
            return null;

        var baseType = typeSymbol.BaseType;

        while (baseType is not null
               && baseType.SpecialType != SpecialType.System_Object)
        {
            foreach (var iface in baseType.AllInterfaces)
            {
                if (iface.TypeArguments.Length != 1)
                    continue;

                var originalDef = iface.OriginalDefinition;
                if (!IsHktTypeClassInterface(originalDef))
                    continue;

                var typeArg = iface.TypeArguments[0];
                // The brand is detected when the sole type-arg's OriginalDefinition equals the base type's OriginalDefinition
                if (!SymbolEqualityComparer.Default.Equals(
                        typeArg.OriginalDefinition,
                        baseType.OriginalDefinition))
                    continue;

                // Found a brand match on baseType
                var brandNs = baseType.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : baseType.ContainingNamespace.ToDisplayString();

                var brandTyParamNames = baseType.TypeParameters
                    .Select(tp => tp.Name)
                    .ToImmutableArray();

                return new BrandInfo(
                    FullyQualifiedTypeName: baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    SimpleName: baseType.Name,
                    Namespace: brandNs,
                    DataInterfaceFqn: dataIfaceFqn,
                    BrandTyParamNames: brandTyParamNames);
            }

            baseType = baseType.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Finds the fully-qualified name of the Data&lt;F, A&gt; interface the union type implements,
    /// with A replaced by a type-parameter name token that can be used in emitted generic code.
    /// Returns something like "global::Lambdaba.Data" (the container namespace path only).
    /// </summary>
    private static string? FindDataInterfaceFqn(INamedTypeSymbol typeSymbol)
    {
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (iface.TypeArguments.Length != 2)
                continue;
            if (iface.Name != "Data")
                continue;
            // Return the namespace-qualified original definition path (strips type args)
            return iface.OriginalDefinition.ContainingNamespace.IsGlobalNamespace
                ? iface.Name
                : $"global::{iface.OriginalDefinition.ContainingNamespace.ToDisplayString()}.{iface.Name}";
        }
        return null;
    }

    private static bool IsHktTypeClassInterface(INamedTypeSymbol originalDef)
    {
        // The interface must be nested inside a type (Base) in the Lambdaba namespace
        if (originalDef.ContainingType is null)
            return false;
        if (originalDef.ContainingType.Name != "Base")
            return false;
        if (originalDef.ContainingNamespace.ToDisplayString() != "Lambdaba")
            return false;

        return originalDef.Name is "Functor" or "Applicative" or "Monad" or "Alternative" or "MonadPlus";
    }

    // ------------------------------------------------------------------ constraint clause helpers

    private static string BuildConstraintClause(ITypeParameterSymbol tp)
    {
        var parts = new List<string>();

        if (tp.HasReferenceTypeConstraint) parts.Add("class");
        if (tp.HasValueTypeConstraint) parts.Add("struct");
        if (tp.HasNotNullConstraint) parts.Add("notnull");
        if (tp.HasUnmanagedTypeConstraint) parts.Add("unmanaged");

        foreach (var ct in tp.ConstraintTypes)
            parts.Add(ct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        if (tp.HasConstructorConstraint) parts.Add("new()");

        return string.Join(", ", parts);
    }

    private static string BuildAllConstraintClauses(
        ImmutableArray<ITypeParameterSymbol> typeParams)
    {
        var sb = new StringBuilder();
        foreach (var tp in typeParams)
        {
            var constraints = BuildConstraintClause(tp);
            if (!string.IsNullOrEmpty(constraints))
                sb.AppendLine($"    where {tp.Name} : {constraints}");
        }
        return sb.ToString().TrimEnd();
    }

    // ------------------------------------------------------------------ type-param rendering helpers

    private static string TyParamsOpen(ImmutableArray<TypeParameterModel> tyParams)
        => tyParams.IsEmpty ? string.Empty : $"<{string.Join(", ", tyParams.Select(p => p.Name))}>";

    // ------------------------------------------------------------------ emission

    private static void Emit(SourceProductionContext spc, UnionModel m)
    {
        EmitUnionType(spc, m);

        if (m.Brand is not null)
            EmitBrandHelper(spc, m);
    }

    // ------------------------------------------------------------------ emission #1: augmented union type

    private static void EmitUnionType(SourceProductionContext spc, UnionModel m)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(m.Namespace))
        {
            sb.AppendLine($"namespace {m.Namespace};");
            sb.AppendLine();
        }

        var tyParamsStr = TyParamsOpen(m.TyParams);
        var constraints = string.IsNullOrEmpty(m.ConstraintClauses)
            ? string.Empty
            : $"\n{m.ConstraintClauses}";

        sb.AppendLine($"partial class {m.TypeName}{tyParamsStr}{constraints}");
        sb.AppendLine("{");

        // Match<R>
        EmitMatchMethod(spc, sb, m);

        // HasValue
        EmitHasValue(spc, sb, m);

        // Per-case members
        foreach (var c in m.Cases)
        {
            EmitIsProperty(spc, sb, m, c);
            EmitTryGetValue(spc, sb, m, c);
        }

        sb.AppendLine("}");

        var hintName = BuildHintName(m.FullyQualifiedTypeName) + ".Union.g.cs";
        spc.AddSource(hintName, sb.ToString());
    }

    private static void EmitMatchMethod(SourceProductionContext spc, StringBuilder sb, UnionModel m)
    {
        const string memberName = "Match";

        if (MemberAlreadyExists(m, memberName))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Lmbd001,
                Location.None,
                memberName,
                m.TypeName));
            return;
        }

        // Use TResult as return type param to avoid shadowing any class-level type parameter
        // that the union type itself may already declare (e.g. Either<L, R> has "R" at class level).
        const string returnTyParam = "TResult";

        // Func<Case1, TResult> onCase1, Func<Case2, TResult> onCase2, ...
        var paramList = string.Join(", ", m.Cases.Select(c =>
            $"global::System.Func<{c.FullyQualifiedType}, {returnTyParam}> on{c.SimpleName}"));

        // Case1 v0 => onCase1(v0), ...
        var switchArms = string.Join("\n            ", m.Cases.Select((c, i) =>
            $"{c.FullyQualifiedType} v_{i} => on{c.SimpleName}(v_{i}),"));

        sb.AppendLine($"    public {returnTyParam} Match<{returnTyParam}>({paramList}) =>");
        sb.AppendLine($"        Value switch");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            {switchArms}");
        sb.AppendLine($"            null => throw new global::System.InvalidOperationException(\"Union value is null.\"),");
        sb.AppendLine($"            _ => throw new global::System.InvalidOperationException(\"Union value is in an unexpected case.\")");
        sb.AppendLine($"        }};");
        sb.AppendLine();
    }

    private static void EmitHasValue(SourceProductionContext spc, StringBuilder sb, UnionModel m)
    {
        const string memberName = "HasValue";

        if (MemberAlreadyExists(m, memberName))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Lmbd001,
                Location.None,
                memberName,
                m.TypeName));
            return;
        }

        sb.AppendLine($"    public bool HasValue => Value is not null;");
        sb.AppendLine();
    }

    private static void EmitIsProperty(SourceProductionContext spc, StringBuilder sb, UnionModel m, CaseModel c)
    {
        var memberName = $"Is{c.SimpleName}";

        if (MemberAlreadyExists(m, memberName))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Lmbd001,
                Location.None,
                memberName,
                m.TypeName));
            return;
        }

        sb.AppendLine($"    public bool {memberName} => Value is {c.FullyQualifiedType};");
        sb.AppendLine();
    }

    private static void EmitTryGetValue(SourceProductionContext spc, StringBuilder sb, UnionModel m, CaseModel c)
    {
        // TryGetValue is overloaded by the out parameter type; we use a combined name check
        // with a per-case discriminator. We check for any existing TryGetValue member.
        // Per conflict policy: if ANY TryGetValue exists, we suppress this overload and warn.
        const string memberName = "TryGetValue";

        if (MemberAlreadyExists(m, memberName))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Lmbd001,
                Location.None,
                $"{memberName}(out {c.SimpleName}?)",
                m.TypeName));
            return;
        }

        sb.AppendLine($"    public bool {memberName}([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {c.FullyQualifiedType}? value)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        value = Value as {c.FullyQualifiedType};");
        sb.AppendLine($"        return value is not null;");
        sb.AppendLine($"    }}");
        sb.AppendLine();
    }

    // ------------------------------------------------------------------ emission #2: brand helper

    private static void EmitBrandHelper(SourceProductionContext spc, UnionModel m)
    {
        var brand = m.Brand!;
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(brand.Namespace))
        {
            sb.AppendLine($"namespace {brand.Namespace};");
            sb.AppendLine();
        }

        // The brand helper is generic over (brand type params) + A + R.
        // Convention: the union's last type parameter is the HKT value slot (A).
        // For Maybe<A>  (1 param, brand Maybe     has 0 brand params): Match<A, R>(Data<Maybe, A>, ...)
        // For Either<L,R> (2 params, brand Either<L> has 1 brand param L): Match<L, A, R>(Data<Either<L>, A>, ...)
        var aTyParam = m.TyParams.Length > 0 ? m.TyParams[m.TyParams.Length - 1].Name : "A";

        // Brand type params (e.g. "L" for Either<L>) are class-level on the brand partial class.
        // They must NOT be re-declared as method-level type params — they are already in scope.
        var brandTyParams = brand.BrandTyParamNames;

        // All union type params in order (e.g. "L, R" for Either<L, R>).
        var allUnionTyParams = string.Join(", ", m.TyParams.Select(tp => tp.Name));

        // Brand class rendered with its own type params (e.g. "Either<L>").
        var brandWithTyParams = brandTyParams.IsEmpty
            ? brand.SimpleName
            : $"{brand.SimpleName}<{string.Join(", ", brandTyParams)}>";

        // Use TResult as return type to avoid collision with union type param names (e.g. Either<L, R>
        // already uses "R" as a type param, so we cannot also use "R" as the return type variable).
        const string returnTyParam = "TResult";

        var paramList = string.Join(", ", m.Cases.Select(c =>
            $"global::System.Func<{c.FullyQualifiedType}, {returnTyParam}> on{c.SimpleName}"));

        var unionTypeName = m.TypeName;
        var castArgs = string.Join(", ", m.Cases.Select(c => $"on{c.SimpleName}"));

        // The cast target uses all union type params, e.g. "Either<L, R>" for Either<L, R>.
        var castTarget = $"{m.Namespace}.{unionTypeName}<{allUnionTyParams}>";

        // The partial class declaration mirrors the brand class type params (e.g. "Either<L>").
        // Method-level type params are only: the value slot (aTyParam) and the return type (TResult).
        sb.AppendLine($"partial class {brandWithTyParams}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static {returnTyParam} Match<{aTyParam}, {returnTyParam}>({brand.DataInterfaceFqn}<{brandWithTyParams}, {aTyParam}> data,");
        sb.AppendLine($"        {paramList}) =>");
        sb.AppendLine($"        (({castTarget})data).Match({castArgs});");
        sb.AppendLine("}");

        var hintName = BuildHintName(brand.FullyQualifiedTypeName) + ".Brand.g.cs";
        spc.AddSource(hintName, sb.ToString());
    }

    // ------------------------------------------------------------------ helpers

    private static string RenderCaseTypeWithTyParam(
        string fullyQualifiedCaseType,
        UnionModel m,
        string aTyParam)
    {
        // If the case type is generic (e.g. global::Lambdaba.Just<global::Lambdaba.Maybe.A>),
        // we need the simple form with the actual type parameter name.
        // Strategy: if the case type ends with ">", replace the inner type-arg with aTyParam.
        // We do this by checking if the case type contains a "<" — if so, take the base name and add <A>.
        // The fully qualified type for Just<A> is something like "global::Lambdaba.Just<A>".
        // We strip the global:: prefix for namespace resolution and keep the structure.
        // Simpler: check if the original case model has a generic arity.
        // fullyQualifiedCaseType for Just<A> during analysis is the bound type symbol display.
        // In the analysis context the type arg "A" is actually a type parameter symbol named "A".
        // Roslyn displays it as e.g. "global::Lambdaba.Just<A>" where "A" is the tp name.
        // So the display string already uses the parameter name. We can use it directly.
        return fullyQualifiedCaseType;
    }

    private static bool MemberAlreadyExists(UnionModel m, string memberName)
        => m.ExistingMembers.Contains(memberName);

    private static string BuildHintName(string fullyQualifiedTypeName)
        => fullyQualifiedTypeName
            .Replace("global::", string.Empty)
            .Replace(".", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", string.Empty);
}
