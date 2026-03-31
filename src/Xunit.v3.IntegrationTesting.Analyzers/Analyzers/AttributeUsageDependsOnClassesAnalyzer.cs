using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Reports when a dependency type in [DependsOnClasses] already belongs to an explicit
/// collection (via [Collection] or [CollectionDefinition]), or when it is a plain class
/// that doesn't belong to any named collection at all.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageDependsOnClassesAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        AttributeUsageDescriptors.DependsOnClassesDependencyAlreadyInCollection,
        AttributeUsageDescriptors.DependsOnClassesDependencyNotInCollection,
    ];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attributeSyntax)
            return;

        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;

        var dependsOnClassesSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnClassesAttribute");
        if (dependsOnClassesSymbol == null)
            return;

        var attributeType = semanticModel.GetTypeInfo(attributeSyntax).Type;
        if (!SymbolEqualityComparer.Default.Equals(attributeType, dependsOnClassesSymbol))
            return;

        var collectionAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionAttribute");
        var collectionDefinitionAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionDefinitionAttribute");

        if (collectionAttributeSymbol == null || collectionDefinitionAttributeSymbol == null)
            return;

        // Find the Dependencies argument
        if (attributeSyntax.ArgumentList == null)
            return;

        var isCollectionPerAssembly = IsCollectionPerAssembly(compilation);

        foreach (var arg in attributeSyntax.ArgumentList.Arguments)
        {
            if (arg.NameEquals?.Name.Identifier.Text != "Dependencies")
                continue;

            // Dependencies = [typeof(X), typeof(Y)]
            if (arg.Expression is not CollectionExpressionSyntax collectionExpression)
                continue;

            foreach (var element in collectionExpression.Elements)
            {
                if (element is not ExpressionElementSyntax exprElement)
                    continue;

                if (exprElement.Expression is not TypeOfExpressionSyntax typeOfExpr)
                    continue;

                var typeInfo = semanticModel.GetTypeInfo(typeOfExpr.Type);
                if (typeInfo.Type is not INamedTypeSymbol dependencyType)
                    continue;

                if (HasExplicitCollectionAttribute(dependencyType, collectionAttributeSymbol, collectionDefinitionAttributeSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AttributeUsageDescriptors.DependsOnClassesDependencyAlreadyInCollection,
                        typeOfExpr.GetLocation(),
                        dependencyType.Name));
                }
                else if (isCollectionPerAssembly && !HasDependsOnClassesAttribute(dependencyType, dependsOnClassesSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AttributeUsageDescriptors.DependsOnClassesDependencyNotInCollection,
                        typeOfExpr.GetLocation(),
                        dependencyType.Name));
                }
            }
        }
    }

    /// <summary>
    /// Checks if [assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)] is set.
    /// When CollectionPerAssembly is active, plain classes share a single assembly-wide collection
    /// whose name won't match GetCollectionNameForType, breaking dependency resolution.
    /// Default (no attribute or CollectionPerClass) is safe.
    /// </summary>
    private static bool IsCollectionPerAssembly(Compilation compilation)
    {
        var collectionBehaviorAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionBehaviorAttribute");
        if (collectionBehaviorAttributeSymbol == null)
            return false;

        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (attr.AttributeClass == null)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionBehaviorAttributeSymbol))
                continue;

            // CollectionBehavior enum: CollectionPerAssembly = 0, CollectionPerClass = 1
            // Constructor: CollectionBehaviorAttribute(CollectionBehavior collectionBehavior)
            if (attr.ConstructorArguments.Length >= 1 &&
                attr.ConstructorArguments[0].Value is int behaviorValue &&
                behaviorValue == 0) // CollectionPerAssembly
            {
                return true;
            }

            // Parameterless constructor defaults to CollectionPerClass (safe)
            return false;
        }

        // No attribute = default = CollectionPerClass (safe)
        return false;
    }

    private static bool HasExplicitCollectionAttribute(
        INamedTypeSymbol type,
        INamedTypeSymbol collectionAttributeSymbol,
        INamedTypeSymbol collectionDefinitionAttributeSymbol)
    {
        foreach (var attr in type.GetAttributes())
        {
            if (attr.AttributeClass == null)
                continue;

            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionAttributeSymbol))
                return true;

            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionDefinitionAttributeSymbol))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the type (or any of its base types) has [DependsOnClasses].
    /// This mirrors the runtime behavior where GetCustomAttributes(true) includes inherited attributes.
    /// </summary>
    private static bool HasDependsOnClassesAttribute(INamedTypeSymbol type, INamedTypeSymbol dependsOnClassesSymbol)
    {
        var current = type;
        while (current != null)
        {
            foreach (var attr in current.GetAttributes())
            {
                if (attr.AttributeClass != null &&
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, dependsOnClassesSymbol))
                    return true;
            }
            current = current.BaseType;
        }

        return false;
    }
}
