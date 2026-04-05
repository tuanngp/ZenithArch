using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RynorArch.Generator.Models;

namespace RynorArch.Generator.Pipeline;

/// <summary>
/// Transforms semantic model data into <see cref="EntityModel"/> instances.
/// All methods are static and allocation-conscious for hot-path performance.
/// No LINQ is used — explicit loops only.
/// </summary>
internal static class EntityTransformer
{
    private const string EntityAttributeFqn = "RynorArch.Abstractions.Attributes.EntityAttribute";
    private const string AggregateRootAttributeFqn = "RynorArch.Abstractions.Attributes.AggregateRootAttribute";
    private const string QueryFilterAttributeFqn = "RynorArch.Abstractions.Attributes.QueryFilterAttribute";
    private const string MapToAttributeFqn = "RynorArch.Abstractions.Attributes.MapToAttribute";
    private const string ArchitectureAttributeFqn = "RynorArch.Abstractions.Attributes.ArchitectureAttribute";

    /// <summary>
    /// Transforms a <see cref="GeneratorAttributeSyntaxContext"/> into an <see cref="EntityModel"/>.
    /// Returns null if the target symbol is not valid.
    /// </summary>
    public static EntityModel? TransformEntity(
        GeneratorAttributeSyntaxContext context,
        System.Threading.CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        string name = typeSymbol.Name;
        string ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : typeSymbol.ContainingNamespace.ToDisplayString();

        // Check for AggregateRoot
        bool isAggregateRoot = HasAttribute(typeSymbol, AggregateRootAttributeFqn);

        // Extract MapTo target
        string? mapToTypeName = null;
        string? mapToTypeNamespace = null;
        ExtractMapToTarget(typeSymbol, ref mapToTypeName, ref mapToTypeNamespace);

        // Extract properties — no LINQ, explicit loop
        var members = typeSymbol.GetMembers();
        var properties = new List<PropertyModel>();
        var filterProperties = new List<PropertyModel>();

        for (int i = 0; i < members.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (members[i] is not IPropertySymbol propSymbol)
                continue;

            // Skip compiler-generated, static, or indexer properties
            if (propSymbol.IsImplicitlyDeclared || propSymbol.IsStatic || propSymbol.IsIndexer)
                continue;

            // Skip Id property (comes from EntityBase)
            if (propSymbol.Name == "Id")
                continue;

            var propModel = ExtractProperty(propSymbol);
            properties.Add(propModel);

            if (HasAttribute(propSymbol, QueryFilterAttributeFqn))
            {
                filterProperties.Add(propModel);
            }
        }

        return new EntityModel(
            name: name,
            @namespace: ns,
            properties: new EquatableArray<PropertyModel>(properties),
            filterProperties: new EquatableArray<PropertyModel>(filterProperties),
            isAggregateRoot: isAggregateRoot,
            mapToTypeName: mapToTypeName,
            mapToTypeNamespace: mapToTypeNamespace);
    }

    /// <summary>
    /// Extracts <see cref="ArchitectureConfig"/> from the compilation's assembly attributes.
    /// </summary>
    public static ArchitectureConfig ExtractConfig(Compilation compilation, System.Threading.CancellationToken cancellationToken)
    {
        var assemblyAttributes = compilation.Assembly.GetAttributes();

        for (int i = 0; i < assemblyAttributes.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attr = assemblyAttributes[i];
            if (attr.AttributeClass is null)
                continue;

            string fqn = attr.AttributeClass.ToDisplayString();
            if (!string.Equals(fqn, ArchitectureAttributeFqn, System.StringComparison.Ordinal))
                continue;

            int pattern = 0;
            bool useSpecification = false;
            bool useUnitOfWork = false;
            bool enableValidation = false;
            bool generateDependencyInjection = false;
            bool generateEndpoints = false;
            bool generateDtos = false;
            bool generateEfConfigurations = false;
            bool generateCachingDecorators = false;
            bool generatePagination = false;

            var namedArgs = attr.NamedArguments;
            for (int j = 0; j < namedArgs.Length; j++)
            {
                var arg = namedArgs[j];
                switch (arg.Key)
                {
                    case "Pattern":
                        pattern = (int)arg.Value.Value!;
                        break;
                    case "UseSpecification":
                        useSpecification = (bool)arg.Value.Value!;
                        break;
                    case "UseUnitOfWork":
                        useUnitOfWork = (bool)arg.Value.Value!;
                        break;
                    case "EnableValidation":
                        enableValidation = (bool)arg.Value.Value!;
                        break;
                    case "GenerateDependencyInjection":
                        generateDependencyInjection = (bool)arg.Value.Value!;
                        break;
                    case "GenerateEndpoints":
                        generateEndpoints = (bool)arg.Value.Value!;
                        break;
                    case "GenerateDtos":
                        generateDtos = (bool)arg.Value.Value!;
                        break;
                    case "GenerateEfConfigurations":
                        generateEfConfigurations = (bool)arg.Value.Value!;
                        break;
                    case "GenerateCachingDecorators":
                        generateCachingDecorators = (bool)arg.Value.Value!;
                        break;
                    case "GeneratePagination":
                        generatePagination = (bool)arg.Value.Value!;
                        break;
                }
            }

            return new ArchitectureConfig(
                pattern, 
                useSpecification, 
                useUnitOfWork, 
                enableValidation,
                generateDependencyInjection,
                generateEndpoints,
                generateDtos,
                generateEfConfigurations,
                generateCachingDecorators,
                generatePagination);
        }

        return ArchitectureConfig.Default;
    }

    private static PropertyModel ExtractProperty(IPropertySymbol propSymbol)
    {
        var type = propSymbol.Type;
        bool isNullable = type.NullableAnnotation == NullableAnnotation.Annotated;
        bool isCollection = false;
        string? collectionElementType = null;

        // Unwrap Nullable<T> for value types
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                && namedType.TypeArguments.Length > 0)
            {
                type = namedType.TypeArguments[0];
                isNullable = true;
            }

            // Check for collection types (IEnumerable<T>, ICollection<T>, IList<T>, List<T>)
            if (IsCollectionType(namedType))
            {
                isCollection = true;
                if (namedType.TypeArguments.Length > 0)
                {
                    collectionElementType = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }
        }

        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new PropertyModel(
            name: propSymbol.Name,
            typeName: typeName,
            isNullable: isNullable,
            isCollection: isCollection,
            collectionElementType: collectionElementType);
    }

    private static bool IsCollectionType(INamedTypeSymbol type)
    {
        if (type.TypeArguments.Length == 0) return false;

        string name = type.OriginalDefinition.ToDisplayString();
        return name == "System.Collections.Generic.IEnumerable<T>"
            || name == "System.Collections.Generic.ICollection<T>"
            || name == "System.Collections.Generic.IList<T>"
            || name == "System.Collections.Generic.List<T>"
            || name == "System.Collections.Generic.IReadOnlyList<T>"
            || name == "System.Collections.Generic.IReadOnlyCollection<T>";
    }

    private static bool HasAttribute(ISymbol symbol, string attributeFqn)
    {
        var attributes = symbol.GetAttributes();
        for (int i = 0; i < attributes.Length; i++)
        {
            if (attributes[i].AttributeClass is { } attrClass
                && string.Equals(attrClass.ToDisplayString(), attributeFqn, System.StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static void ExtractMapToTarget(INamedTypeSymbol typeSymbol, ref string? typeName, ref string? typeNamespace)
    {
        var attributes = typeSymbol.GetAttributes();
        for (int i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if (attr.AttributeClass is null)
                continue;

            if (!string.Equals(attr.AttributeClass.ToDisplayString(), MapToAttributeFqn, System.StringComparison.Ordinal))
                continue;

            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is INamedTypeSymbol targetType)
            {
                typeName = targetType.Name;
                typeNamespace = targetType.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : targetType.ContainingNamespace.ToDisplayString();
                return;
            }
        }
    }
}
