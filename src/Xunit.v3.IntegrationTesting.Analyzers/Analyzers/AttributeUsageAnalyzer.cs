using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        AttributeUsageDescriptors.NotSupportedClassLevelTestCaseOrderer,
        AttributeUsageDescriptors.NotSupportedAssemblyLevelTestCaseOrderer,
        AttributeUsageDescriptors.MissingTestCaseOrderer
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;
        var methodName = methodDecl.Identifier.Text;
        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;

        var dependsOnAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnAttribute");
        var testCaseOrdererAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TestCaseOrdererAttribute");
        var dependencyAwareOrdererSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependencyAwareTestCaseOrderer");

        if (dependsOnAttributeSymbol == null || testCaseOrdererAttributeSymbol == null || dependencyAwareOrdererSymbol == null)
        {
            return;
        }

        // Check for DependsOnAttribute on a method
        bool hasDependsOn = false;
        foreach (var attrList in methodDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (SymbolEqualityComparer.Default.Equals(attrType, dependsOnAttributeSymbol))
                {
                    hasDependsOn = true;
                    break;
                }
            }
            if (hasDependsOn)
            {
                break;
            }
        }

        if (!hasDependsOn)
        {
            return;
        }

        // Check for TestCaseOrdererAttribute on a containing class
        bool hasClassOrderer = false;
        var classDecl = methodDecl.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDecl != null)
        {
            foreach (var attrList in classDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrType = semanticModel.GetTypeInfo(attr).Type;
                    if (SymbolEqualityComparer.Default.Equals(attrType, testCaseOrdererAttributeSymbol))
                    {
                        hasClassOrderer = true;
                        // Check if the argument is typeof(DependencyAwareTestCaseOrderer)
                        if (attr.ArgumentList != null && attr.ArgumentList.Arguments.Count == 1 &&
                            attr.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExpr)
                        {
                            var typeSymbol = semanticModel.GetTypeInfo(typeOfExpr.Type).Type;
                            if (SymbolEqualityComparer.Default.Equals(typeSymbol, dependencyAwareOrdererSymbol))
                            {
                                break;
                            }
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            AttributeUsageDescriptors.NotSupportedClassLevelTestCaseOrderer, methodDecl.Identifier.GetLocation(), methodName));

                        break;
                    }
                }

                if (hasClassOrderer)
                {
                    break;
                }
            }
        }

        if (hasClassOrderer)
        {
            return;
        }

        // Check for TestCaseOrdererAttribute on assembly
        bool hasAssemblyOrderer = false;
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseOrdererAttributeSymbol))
            {
                hasAssemblyOrderer = true;
                if (attr.ConstructorArguments.Length == 1)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Kind == TypedConstantKind.Type && SymbolEqualityComparer.Default.Equals(arg.Value as INamedTypeSymbol, dependencyAwareOrdererSymbol))
                    {
                        break;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    AttributeUsageDescriptors.NotSupportedAssemblyLevelTestCaseOrderer, methodDecl.Identifier.GetLocation(), methodName));
                break;
            }
        }

        if (hasAssemblyOrderer)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            AttributeUsageDescriptors.MissingTestCaseOrderer, methodDecl.Identifier.GetLocation(), methodName));
    }
}
