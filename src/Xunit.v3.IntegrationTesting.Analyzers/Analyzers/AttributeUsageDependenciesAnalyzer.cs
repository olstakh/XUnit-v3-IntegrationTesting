using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageDependenciesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create([
            AttributeUsageDescriptors.DependsOnMissingMethod,
        ]);

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

        var dependsOnAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.FactDependsOnAttribute");
        var factAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.FactAttribute");

        if (dependsOnAttributeSymbol == null || factAttributeSymbol == null)
            return;

        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
        if (methodSymbol == null)
            return;            

        // Check if method has [FactDependsOn]
        AttributeSyntax? dependsOnAttrSyntax = null;
        AttributeSyntax? factAttributeSyntax = null;
        foreach (var attrList in methodDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrType = semanticModel.GetTypeInfo(attr).Type;
                if (SymbolEqualityComparer.Default.Equals(attrType, dependsOnAttributeSymbol))
                {
                    dependsOnAttrSyntax = attr;
                    break;
                }
                if (SymbolEqualityComparer.Default.Equals(attrType, factAttributeSymbol))
                {
                    factAttributeSyntax = attr;
                    break;
                }
            }
            if (dependsOnAttrSyntax != null)
                break;
        }

        // Get dependencies property from attribute syntax (support string literals and nameof)
        var dependencies = new List<string>();

        if (dependsOnAttrSyntax?.ArgumentList != null)
        {
            foreach (var arg in dependsOnAttrSyntax.ArgumentList.Arguments)
            {
                if (arg.NameEquals?.Name.Identifier.Text == "Dependencies")
                {
                    var expr = arg.Expression;
                    // Handles: Dependencies = new[] { ... }
                    if (expr is ImplicitArrayCreationExpressionSyntax implicitArray && implicitArray.Initializer != null)
                    {
                        foreach (var element in implicitArray.Initializer.Expressions)
                        {
                            var c = semanticModel.GetConstantValue(element);
                            if (c.HasValue && c.Value is string s)
                            {
                                dependencies.Add(s);
                            }
                        }
                    }
                    // Handles: Dependencies = new string[] { ... }
                    else if (expr is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
                    {
                        foreach (var element in arrayCreation.Initializer.Expressions)
                        {
                            var c = semanticModel.GetConstantValue(element);
                            if (c.HasValue && c.Value is string s)
                            {
                                dependencies.Add(s);
                            }
                        }
                    }
                    // Handles: Dependencies = [...]
                    else if (expr is CollectionExpressionSyntax collectionExpression)
                    {
                        foreach (var element in collectionExpression.Elements.OfType<ExpressionElementSyntax>())
                        {
                            var c = semanticModel.GetConstantValue(element.Expression);
                            if (c.HasValue && c.Value is string s)
                            {
                                dependencies.Add(s);
                            }
                        }
                    }
                }
            }
        }

        if (dependencies.Count == 0)
            return;

        // Get all methods in the class
        var classDecl = methodDecl.Parent as ClassDeclarationSyntax;
        if (classDecl == null)
            return;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol == null)
            return;

        var methodsByName = classSymbol.GetMembers().OfType<IMethodSymbol>().ToDictionary(m => m.Name);

        foreach (var depName in dependencies)
        {
            if (!methodsByName.TryGetValue(depName, out var depMethod))
            {
                // No method with such name
                context.ReportDiagnostic(Diagnostic.Create(AttributeUsageDescriptors.DependsOnMissingMethod, methodDecl.Identifier.GetLocation(), methodSymbol.Name, depName));
            }
        }
    }

    private static string? TryGetStringFromExpression(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }
        else if (expr is InvocationExpressionSyntax invocation && invocation.Expression is IdentifierNameSyntax idName && idName.Identifier.Text == "nameof")
        {
            if (invocation.ArgumentList.Arguments.Count == 1)
            {
                var nameofArg = invocation.ArgumentList.Arguments[0].Expression;
                if (nameofArg is IdentifierNameSyntax nameofId)
                {
                    return nameofId.Identifier.Text;
                }
            }
        }
        return null;
    }
}
