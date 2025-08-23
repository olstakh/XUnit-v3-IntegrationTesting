using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.v3.IntegrationTesting.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageTestFrameworkAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(AttributeUsageDescriptors.MissingTestFrameworkAttribute);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilation = context.Compilation;
        var testFrameworkAttributeSymbol = compilation.GetTypeByMetadataName("Xunit.TestFrameworkAttribute");
        var dependencyAwareFrameworkSymbol = compilation.GetTypeByMetadataName("Xunit.v3.IntegrationTesting.DependencyAwareFramework");
        if (testFrameworkAttributeSymbol == null || dependencyAwareFrameworkSymbol == null)
            return;

        bool found = false;
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testFrameworkAttributeSymbol))
            {
                if (attr.ConstructorArguments.Length == 1)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Kind == TypedConstantKind.Type && SymbolEqualityComparer.Default.Equals(arg.Value as INamedTypeSymbol, dependencyAwareFrameworkSymbol))
                    {
                        found = true;
                        break;
                    }
                }
            }
        }
        if (!found)
        {
            context.ReportDiagnostic(Diagnostic.Create(AttributeUsageDescriptors.MissingTestFrameworkAttribute, Location.None));
        }
    }
}
