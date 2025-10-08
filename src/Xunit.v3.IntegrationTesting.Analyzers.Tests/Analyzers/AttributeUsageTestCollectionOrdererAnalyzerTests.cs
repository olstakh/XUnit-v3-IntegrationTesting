using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageTestCollectionOrdererAnalyzerTests
{
    [Fact]
    public async Task Validate_DependsOn_MissingTestCollectionOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            [{|XIT0011:DependsOnCollections|}]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_NotSupportedCollectionOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3;
            using Xunit.v3.IntegrationTesting;
            using System.Collections.Generic;

            [assembly: TestCollectionOrderer(typeof(DefaultCollectionOrderer))]

            [{|XIT0012:DependsOnCollections|}]
            public sealed class MyCollection;

            public class DefaultCollectionOrderer : ITestCollectionOrderer
            {
                public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
                    where TTestCollection : ITestCollection
                {
                    return testCollections;
                }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_CorrectUsage_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]

            [DependsOnCollections]
            public sealed class MyCollection;
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static AnalyzerTest<DefaultVerifier> GetAnalyzer(string source) => new CSharpAnalyzerTest<AttributeUsageTestCollectionOrdererAnalyzer, DefaultVerifier>
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