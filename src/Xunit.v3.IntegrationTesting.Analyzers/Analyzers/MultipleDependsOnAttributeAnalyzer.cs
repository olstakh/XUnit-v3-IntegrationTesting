using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.v3.IntegrationTesting.Analyzers.Helpers;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Reports when a method has multiple attributes derived from <c>DependsOnAttributeBase</c>
/// (e.g. both [FactDependsOn] and [TheoryDependsOn]), or when a method has a
/// <c>DependsOnAttributeBase</c> combined with another <c>IFactAttribute</c> (e.g. [Fact]).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleDependsOnAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        AttributeUsageDescriptors.MultipleDependsOnAttributes,
        AttributeUsageDescriptors.DependsOnWithOtherFactAttributes,
    ];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;

        var dependsOnBaseSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnAttributeBase");
        if (dependsOnBaseSymbol == null)
            return;

        var iFactAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IFactAttribute");

        int dependsOnCount = 0;
        bool hasOtherFactAttribute = false;
        foreach (var attrList in methodDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (TypeHierarchyHelper.IsOrDerivesFrom(attrType, dependsOnBaseSymbol))
                    dependsOnCount++;
                else if (iFactAttributeSymbol != null && attrType is INamedTypeSymbol namedType
                      && namedType.AllInterfaces.Contains(iFactAttributeSymbol, SymbolEqualityComparer.Default))
                    hasOtherFactAttribute = true;
            }
        }

        if (dependsOnCount > 1)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.MultipleDependsOnAttributes,
                    methodDecl.Identifier.GetLocation(),
                    methodSymbol.Name));
            }
        }
        else if (dependsOnCount >= 1 && hasOtherFactAttribute)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.DependsOnWithOtherFactAttributes,
                    methodDecl.Identifier.GetLocation(),
                    methodSymbol.Name));
            }
        }
    }
}
