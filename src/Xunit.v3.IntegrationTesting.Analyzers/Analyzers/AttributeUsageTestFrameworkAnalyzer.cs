using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.v3.IntegrationTesting.Analyzers;

/// <summary>
/// Checks that assemblies using [FactDependsOn] have the correct [assembly: TestFramework(typeof(DependencyAwareFramework))] attribute.
/// Only fires when the compilation actually contains [FactDependsOn] usage.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageTestFrameworkAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create([
            AttributeUsageDescriptors.MissingTestFrameworkAttribute,
            AttributeUsageDescriptors.NotSupportedTestFrameworkAttribute
        ]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        var factDependsOnSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.FactDependsOnAttribute");
        var testFrameworkAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TestFrameworkAttribute");
        var dependencyAwareFrameworkSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependencyAwareFramework");

        if (factDependsOnSymbol == null || testFrameworkAttributeSymbol == null || dependencyAwareFrameworkSymbol == null)
            return;

        bool hasFactDependsOnUsage = false;

        context.RegisterSymbolAction(symbolContext =>
        {
            if (hasFactDependsOnUsage)
                return;

            var method = (IMethodSymbol)symbolContext.Symbol;
            foreach (var attr in method.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, factDependsOnSymbol))
                {
                    hasFactDependsOnUsage = true;
                    return;
                }
            }
        }, SymbolKind.Method);

        context.RegisterCompilationEndAction(endContext =>
        {
            if (!hasFactDependsOnUsage)
                return;

            AnalyzeCompilation(endContext, testFrameworkAttributeSymbol, dependencyAwareFrameworkSymbol);
        });
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context, INamedTypeSymbol testFrameworkAttributeSymbol, INamedTypeSymbol dependencyAwareFrameworkSymbol)
    {
        var compilation = context.Compilation;

        bool found = false;
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testFrameworkAttributeSymbol))
            {
                found = true;
                if (attr.ConstructorArguments.Length == 1)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Kind == TypedConstantKind.Type && SymbolEqualityComparer.Default.Equals(arg.Value as INamedTypeSymbol, dependencyAwareFrameworkSymbol))
                    {
                        break;
                    }
                }
                context.ReportDiagnostic(Diagnostic.Create(AttributeUsageDescriptors.NotSupportedTestFrameworkAttribute, Location.None));
            }
        }

        if (found)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(AttributeUsageDescriptors.MissingTestFrameworkAttribute, Location.None));
    }
}
