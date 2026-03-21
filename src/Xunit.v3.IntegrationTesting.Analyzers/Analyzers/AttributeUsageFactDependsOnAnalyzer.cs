using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Analyzer that flags [Fact] methods in classes belonging to a collection with
/// [DependsOnCollections] dependencies. Without [FactDependsOn], the skip logic
/// for upstream collection failures will not run.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageFactDependsOnAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        AttributeUsageDescriptors.UseFactDependsOnAttribute
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;

        var factAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.FactAttribute");
        var theoryAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TheoryAttribute");
        if (factAttributeSymbol == null && theoryAttributeSymbol == null)
            return;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol == null)
            return;

        if (!ClassBelongsToCollectionWithDependencies(classSymbol, compilation))
            return;

        // Report diagnostic on methods with [Fact] or [Theory]
        foreach (var member in classDecl.Members)
        {
            if (member is not MethodDeclarationSyntax method)
                continue;

            var matchedAttr = (factAttributeSymbol != null ? FindAttribute(method, factAttributeSymbol, semanticModel) : null)
                ?? (theoryAttributeSymbol != null ? FindAttribute(method, theoryAttributeSymbol, semanticModel) : null);
            if (matchedAttr != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.UseFactDependsOnAttribute,
                    matchedAttr.GetLocation(),
                    method.Identifier.Text));
            }
        }
    }

    private static bool ClassBelongsToCollectionWithDependencies(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var dependsOnClassesSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnClassesAttribute");
        var collectionAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionAttribute");
        var collectionDefinitionSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionDefinitionAttribute");
        var dependsOnCollectionsSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnCollectionsAttribute");

        // Check 1: class has [DependsOnClasses] with non-empty Dependencies
        if (dependsOnClassesSymbol != null)
        {
            foreach (var attr in classSymbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, dependsOnClassesSymbol))
                {
                    var depsArg = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Dependencies");
                    if (!depsArg.Value.IsNull && depsArg.Value.Values.Length > 0)
                    {
                        return true;
                    }
                }
            }
        }

        // Check 2: class has [Collection(typeof(X))] where X has [DependsOnCollections(...)]
        // Check 3: class has [Collection("name")] where matching [CollectionDefinition("name")] has [DependsOnCollections(...)]
        if (collectionAttributeSymbol != null && dependsOnCollectionsSymbol != null)
        {
            foreach (var attr in classSymbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionAttributeSymbol) && attr.ConstructorArguments.Length >= 1)
                {
                    var arg = attr.ConstructorArguments[0];

                    INamedTypeSymbol? collectionDefType = null;

                    if (arg.Kind == TypedConstantKind.Type && arg.Value is INamedTypeSymbol typeArg)
                    {
                        collectionDefType = typeArg;
                    }
                    else if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string collectionName && collectionDefinitionSymbol != null)
                    {
                        collectionDefType = FindCollectionDefinitionByName(compilation.GlobalNamespace, collectionName, collectionDefinitionSymbol);
                    }

                    if (collectionDefType != null && HasDependsOnCollections(collectionDefType, dependsOnCollectionsSymbol))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool HasDependsOnCollections(INamedTypeSymbol typeSymbol, INamedTypeSymbol dependsOnCollectionsSymbol)
    {
        foreach (var attr in typeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, dependsOnCollectionsSymbol))
            {
                return attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Values.Length > 0;
            }
        }
        return false;
    }

    private static INamedTypeSymbol? FindCollectionDefinitionByName(INamespaceSymbol ns, string collectionName, INamedTypeSymbol collectionDefinitionSymbol)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            foreach (var attr in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionDefinitionSymbol))
                {
                    string? name = null;
                    if (attr.ConstructorArguments.Length >= 1 && attr.ConstructorArguments[0].Value is string ctorName)
                    {
                        name = ctorName;
                    }

                    if (name == collectionName)
                    {
                        return type;
                    }
                }
            }
        }

        foreach (var childNs in ns.GetNamespaceMembers())
        {
            var result = FindCollectionDefinitionByName(childNs, collectionName, collectionDefinitionSymbol);
            if (result != null) return result;
        }

        return null;
    }

    private static AttributeSyntax? FindAttribute(MethodDeclarationSyntax method, INamedTypeSymbol attributeSymbol, SemanticModel semanticModel)
    {
        foreach (var attrList in method.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (SymbolEqualityComparer.Default.Equals(attrType, attributeSymbol))
                    return attr;
            }
        }
        return null;
    }
}
