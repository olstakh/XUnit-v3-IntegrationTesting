using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.v3.IntegrationTesting.Analyzers.Helpers;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Reports when a method has multiple attributes derived from <c>DependsOnAttributeBase</c>
/// (e.g. both [FactDependsOn] and [TheoryDependsOn]).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleDependsOnAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        AttributeUsageDescriptors.MultipleDependsOnAttributes,
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

        int dependsOnCount = 0;
        foreach (var attrList in methodDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (TypeHierarchyHelper.IsOrDerivesFrom(attrType, dependsOnBaseSymbol))
                    dependsOnCount++;
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
    }
}
