using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

public class AttributeUsageTestCollectionOrdererAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        AttributeUsageDescriptors.MissingTestCollectionOrderer,
        AttributeUsageDescriptors.NotSupportedTestCollectionOrderer,
    ];

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
        var dependsOnCollectionsAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnCollectionsAttribute");

        if (dependsOnCollectionsAttributeSymbol == null)
            return;

        var attributeType = semanticModel.GetTypeInfo(attributeSyntax).Type;
        if (!SymbolEqualityComparer.Default.Equals(attributeType, dependsOnCollectionsAttributeSymbol))
            return;

        // Check if TestCollectionOrdererAttribute is set on assembly level
        var collectionOrdererAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TestCollectionOrdererAttribute");
        var dependencyAwareCollectionOrdererSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependencyAwareTestCollectionOrderer");
        if (collectionOrdererAttributeSymbol == null || dependencyAwareCollectionOrdererSymbol == null)
        {
            return;
        }

        // Check for TestCollectionOrdererAttribute on assembly
        bool hasAssemblyOrderer = false;
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, collectionOrdererAttributeSymbol))
            {
                hasAssemblyOrderer = true;
                if (attr.ConstructorArguments.Length == 1)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Kind == TypedConstantKind.Type && SymbolEqualityComparer.Default.Equals(arg.Value as INamedTypeSymbol, dependencyAwareCollectionOrdererSymbol))
                    {
                        break;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.NotSupportedTestCollectionOrderer, attributeSyntax.GetLocation()));
                break;
            }
        }

        if (hasAssemblyOrderer)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AttributeUsageDescriptors.MissingTestCollectionOrderer, attributeSyntax.GetLocation()));
    }    
}