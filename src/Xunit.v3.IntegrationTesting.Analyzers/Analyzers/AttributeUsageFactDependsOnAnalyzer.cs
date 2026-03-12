using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Analyzer that flags [Fact] methods in classes where sibling methods use [FactDependsOn].
/// All test methods in such classes should use [FactDependsOn] to ensure dependency tracking works correctly.
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
        var factDependsOnSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.FactDependsOnAttribute");

        if (factAttributeSymbol == null || factDependsOnSymbol == null)
            return;

        // First pass: check if any method in the class uses [FactDependsOn]
        bool hasFactDependsOn = false;
        foreach (var member in classDecl.Members)
        {
            if (member is MethodDeclarationSyntax method && HasAttribute(method, factDependsOnSymbol, semanticModel))
            {
                hasFactDependsOn = true;
                break;
            }
        }

        if (!hasFactDependsOn)
            return;

        // Second pass: report diagnostic on methods with [Fact]
        foreach (var member in classDecl.Members)
        {
            if (member is not MethodDeclarationSyntax method)
                continue;

            var factAttr = FindAttribute(method, factAttributeSymbol, semanticModel);
            if (factAttr != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.UseFactDependsOnAttribute,
                    factAttr.GetLocation(),
                    method.Identifier.Text));
            }
        }
    }

    private static bool HasAttribute(MethodDeclarationSyntax method, INamedTypeSymbol attributeSymbol, SemanticModel semanticModel)
    {
        foreach (var attrList in method.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (SymbolEqualityComparer.Default.Equals(attrType, attributeSymbol))
                    return true;
            }
        }
        return false;
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
