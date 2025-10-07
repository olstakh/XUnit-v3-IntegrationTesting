using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices.ComTypes;

namespace Xunit.v3.IntegrationTesting.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageCollectionDependenciesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        AttributeUsageDescriptors.InvalidDependsOnCollectionsAttributeUsage,
        AttributeUsageDescriptors.CollectionDefinitionMissingDisableParallelization,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.Attribute);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
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

        var collectionDefinitionAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.CollectionDefinitionAttribute");
        if (collectionDefinitionAttributeSymbol == null)
            return;

        // Get class declaration
        var classDecl = attributeSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDecl == null)
            return;

        // Check if class has [CollectionDefinition]
        var classAttributes = classDecl.AttributeLists.SelectMany(a => a.Attributes);
        var collectionDefinitionAttribute = classAttributes.FirstOrDefault(attr =>
        {
            var attrType = semanticModel.GetTypeInfo(attr).Type;
            if (attrType == null)
                return false;

            return SymbolEqualityComparer.Default.Equals(attrType, collectionDefinitionAttributeSymbol);
        });

        // We shouldn't apply DependsOnCollections to non-collection definition classes
        if (collectionDefinitionAttribute == null)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol != null)
            {
                var diagnostic = Diagnostic.Create(
                    AttributeUsageDescriptors.InvalidDependsOnCollectionsAttributeUsage,
                    attributeSyntax.GetLocation(),
                    classSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Check if CollectionDefinition has DisableParallelization argument set to true
        bool parallelizationDisabled = false;
        if (collectionDefinitionAttribute.ArgumentList != null)
        {
            foreach (var arg in collectionDefinitionAttribute.ArgumentList.Arguments)
            {
                if (arg.NameEquals?.Name.Identifier.Text == "DisableParallelization")
                {
                    var constantValue = semanticModel.GetConstantValue(arg.Expression);
                    if (constantValue.HasValue && constantValue.Value is bool boolValue && boolValue)
                    {
                        parallelizationDisabled = true;
                        break;
                    }
                }
            }
        }

        if (!parallelizationDisabled)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol != null)
            {
                var diagnostic = Diagnostic.Create(
                    AttributeUsageDescriptors.CollectionDefinitionMissingDisableParallelization,
                    attributeSyntax.GetLocation(),
                    classSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}