using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
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
    private const string SoftDeleteInterfaceFqn = "RynorArch.Abstractions.Interfaces.ISoftDelete";
    private const string AuditableInterfaceFqn = "RynorArch.Abstractions.Interfaces.IAuditable";
    private const string MaxLengthAttributeFqn = "RynorArch.Abstractions.Attributes.MaxLengthAttribute";
    private const string MinLengthAttributeFqn = "RynorArch.Abstractions.Attributes.MinLengthAttribute";
    private const string EmailAttributeFqn = "RynorArch.Abstractions.Attributes.EmailAttribute";
    private const string RequiredAttributeFqn = "RynorArch.Abstractions.Attributes.RequiredAttribute";

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

        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration
            || !IsPartialDeclaration(classDeclaration))
        {
            return null;
        }

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

        // Check for Interfaces (SoftDelete, Auditable)
        bool isSoftDelete = HasInterface(typeSymbol, SoftDeleteInterfaceFqn);
        bool isAuditable = HasInterface(typeSymbol, AuditableInterfaceFqn);

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

            if (HasAttribute(propSymbol, QueryFilterAttributeFqn)
                && IsSupportedQueryFilterType(propSymbol.Type))
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
            mapToTypeNamespace: mapToTypeNamespace,
            isSoftDelete: isSoftDelete,
            isAuditable: isAuditable);
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
            bool enableExperimentalEndpoints = false;
            bool generateDtos = false;
            bool generateEfConfigurations = false;
            bool generateCachingDecorators = false;
            bool generatePagination = false;
            string cqrsDbContextTypeName = ArchitectureConfig.DefaultCqrsDbContextTypeName;
            int cqrsSaveMode = 0;
            int profile = 0;

            var namedArgs = attr.NamedArguments;

            // First pass: resolve profile to establish low-touch defaults.
            for (int j = 0; j < namedArgs.Length; j++)
            {
                var arg = namedArgs[j];
                if (arg.Key == "Profile" && arg.Value.Value is int configuredProfile)
                {
                    profile = configuredProfile;
                    break;
                }
            }

            ApplyProfileDefaults(
                profile,
                ref pattern,
                ref useSpecification,
                ref useUnitOfWork,
                ref enableValidation,
                ref generateDependencyInjection,
                ref generateEndpoints,
                ref enableExperimentalEndpoints,
                ref generateDtos,
                ref generateEfConfigurations,
                ref generateCachingDecorators,
                ref generatePagination,
                ref cqrsSaveMode);

            // Second pass: explicit flags always override profile defaults.
            for (int j = 0; j < namedArgs.Length; j++)
            {
                var arg = namedArgs[j];
                switch (arg.Key)
                {
                    case "Profile":
                        break;
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
                    case "EnableExperimentalEndpoints":
                        enableExperimentalEndpoints = (bool)arg.Value.Value!;
                        break;
                    case "CqrsSaveMode":
                        if (arg.Value.Value is int configuredSaveMode)
                        {
                            cqrsSaveMode = configuredSaveMode;
                        }
                        break;
                    case "DbContextType":
                        if (arg.Value.Value is INamedTypeSymbol dbContextSymbol)
                        {
                            cqrsDbContextTypeName = dbContextSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        }
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
                enableExperimentalEndpoints,
                generateDtos,
                generateEfConfigurations,
                generateCachingDecorators,
                generatePagination,
                cqrsDbContextTypeName,
                cqrsSaveMode);
        }

        return ArchitectureConfig.Default;
    }

    private static void ApplyProfileDefaults(
        int profile,
        ref int pattern,
        ref bool useSpecification,
        ref bool useUnitOfWork,
        ref bool enableValidation,
        ref bool generateDependencyInjection,
        ref bool generateEndpoints,
        ref bool enableExperimentalEndpoints,
        ref bool generateDtos,
        ref bool generateEfConfigurations,
        ref bool generateCachingDecorators,
        ref bool generatePagination,
        ref int cqrsSaveMode)
    {
        // 0 = Custom (no preset defaults)
        if (profile == 0)
        {
            return;
        }

        // 1 = CqrsQuickStart
        if (profile == 1)
        {
            pattern = 0;
            useSpecification = true;
            enableValidation = true;
            generateDependencyInjection = true;
            return;
        }

        // 2 = RepositoryQuickStart
        if (profile == 2)
        {
            pattern = 1;
            useSpecification = true;
            useUnitOfWork = true;
            generateDependencyInjection = true;
            return;
        }

        // 3 = FullStackQuickStart
        if (profile == 3)
        {
            pattern = 2;
            useSpecification = true;
            useUnitOfWork = true;
            enableValidation = true;
            generateDependencyInjection = true;
            generateDtos = true;
            generateEfConfigurations = true;
            generatePagination = true;
            cqrsSaveMode = 1;
            return;
        }

        // Unknown profile: leave defaults untouched.
        _ = generateEndpoints;
        _ = enableExperimentalEndpoints;
        _ = generateCachingDecorators;
    }

    public static bool HasArchitectureConfiguration(Compilation compilation, System.Threading.CancellationToken cancellationToken)
    {
        var assemblyAttributes = compilation.Assembly.GetAttributes();

        for (int i = 0; i < assemblyAttributes.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attr = assemblyAttributes[i];
            if (attr.AttributeClass is null)
                continue;

            if (string.Equals(attr.AttributeClass.ToDisplayString(), ArchitectureAttributeFqn, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPartialDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var modifiers = classDeclaration.Modifiers;
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].IsKind(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsSupportedQueryFilterType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String
            || type.SpecialType == SpecialType.System_Boolean
            || type.SpecialType == SpecialType.System_DateTime
            || type.SpecialType == SpecialType.System_Byte
            || type.SpecialType == SpecialType.System_SByte
            || type.SpecialType == SpecialType.System_Int16
            || type.SpecialType == SpecialType.System_UInt16
            || type.SpecialType == SpecialType.System_Int32
            || type.SpecialType == SpecialType.System_UInt32
            || type.SpecialType == SpecialType.System_Int64
            || type.SpecialType == SpecialType.System_UInt64
            || type.SpecialType == SpecialType.System_Single
            || type.SpecialType == SpecialType.System_Double
            || type.SpecialType == SpecialType.System_Decimal)
        {
            return true;
        }

        if (type.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                && namedType.TypeArguments.Length == 1)
            {
                return IsSupportedQueryFilterType(namedType.TypeArguments[0]);
            }

            return string.Equals(namedType.ToDisplayString(), "System.Guid", System.StringComparison.Ordinal);
        }

        return false;
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

        ValidationRules? validation = ExtractValidation(propSymbol);

        return new PropertyModel(
            name: propSymbol.Name,
            typeName: typeName,
            isNullable: isNullable,
            isCollection: isCollection,
            collectionElementType: collectionElementType,
            validation: validation);
    }

    private static ValidationRules? ExtractValidation(IPropertySymbol propSymbol)
    {
        ValidationRules? rules = null;
        var attributes = propSymbol.GetAttributes();

        for (int i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if (attr.AttributeClass is null) continue;

            string fqn = attr.AttributeClass.ToDisplayString();
            
            if (fqn == MaxLengthAttributeFqn || fqn == MinLengthAttributeFqn || fqn == EmailAttributeFqn || fqn == RequiredAttributeFqn)
            {
                rules ??= new ValidationRules();
                
                if (fqn == MaxLengthAttributeFqn) rules.MaxLength = (int)attr.ConstructorArguments[0].Value!;
                if (fqn == MinLengthAttributeFqn) rules.MinLength = (int)attr.ConstructorArguments[0].Value!;
                if (fqn == EmailAttributeFqn) rules.IsEmail = true;
                if (fqn == RequiredAttributeFqn) rules.IsRequired = true;
            }
        }

        return rules;
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

    public static string? GetEntityNameForUnsupportedQueryFilter(
        GeneratorAttributeSyntaxContext context,
        System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetSymbol is not IPropertySymbol propertySymbol)
            return null;

        return HasAttribute(propertySymbol, QueryFilterAttributeFqn)
            && !IsSupportedQueryFilterType(propertySymbol.Type)
            ? propertySymbol.ContainingType?.Name
            : null;
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

    private static bool HasInterface(INamedTypeSymbol typeSymbol, string interfaceFqn)
    {
        var interfaces = typeSymbol.AllInterfaces;
        for (int i = 0; i < interfaces.Length; i++)
        {
            if (string.Equals(interfaces[i].ToDisplayString(), interfaceFqn, System.StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }
}
