using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.v3.IntegrationTesting.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "XIT001";
    private static readonly LocalizableString Title = "DependsOn attribute requires DependencyAwareTestCaseOrderer";
    private static readonly LocalizableString MessageFormat = "Method '{0}' uses [DependsOn] but no [assembly: TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))] is defined";
    private static readonly LocalizableString Description = "Any method with [DependsOn] must have DependencyAwareTestCaseOrderer set as assembly TestCaseOrderer, in order for test to be ordered according to defined dependencies.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        var dependsOnAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependsOnAttribute");
        var testCaseOrdererAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TestCaseOrdererAttribute");
        var dependencyAwareOrdererSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependencyAwareTestCaseOrderer");

        if (dependsOnAttributeSymbol == null || testCaseOrdererAttributeSymbol == null || dependencyAwareOrdererSymbol == null)
        {
            return;
        }

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

        bool hasAssemblyOrderer = false;
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseOrdererAttributeSymbol))
            {
                if (attr.ConstructorArguments.Length == 1)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Kind == TypedConstantKind.Type && SymbolEqualityComparer.Default.Equals(arg.Value as INamedTypeSymbol, dependencyAwareOrdererSymbol))
                    {
                        hasAssemblyOrderer = true;
                        break;
                    }
                }
            }
        }

        if (!hasAssemblyOrderer)
        {
            var methodName = methodDecl.Identifier.Text;
            context.ReportDiagnostic(Diagnostic.Create(Rule, methodDecl.Identifier.GetLocation(), methodName));
        }
    }
}
