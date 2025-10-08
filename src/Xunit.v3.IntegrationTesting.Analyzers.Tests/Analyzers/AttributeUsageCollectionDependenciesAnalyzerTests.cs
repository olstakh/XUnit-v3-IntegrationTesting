using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageCollectionDependenciesAnalyzerTests
{
    [Fact]
    public async Task Validate_DependsOnCollections_ValidUsage_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [{|XIT0009:DependsOnCollections|}]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOnCollections_MissingDisableParallelization_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"")]
            [{|XIT0010:DependsOnCollections|}]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOnCollections_DisableParallelizationSetToFalse_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"", DisableParallelization = false)]
            [{|XIT0010:DependsOnCollections|}]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOnCollections_DisableParallelizationSetToTrue_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"", DisableParallelization = true)]
            [DependsOnCollections]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpAnalyzerTest<AttributeUsageCollectionDependenciesAnalyzer, DefaultVerifier> GetAnalyzer(string source) =>
        new CSharpAnalyzerTest<AttributeUsageCollectionDependenciesAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            TestState =
            {
                AdditionalReferences = { typeof(FactDependsOnAttribute).Assembly }
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(new PackageIdentity[]
            {
                new PackageIdentity("xunit.v3", "3.0.0")
            }.ToImmutableArray()),
        };
}