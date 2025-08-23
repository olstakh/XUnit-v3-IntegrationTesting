using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageAnalyzerTests
{
    [Fact]
    public async Task Validate_DependsOn_NoTestCaseOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [DependsOn]
                public void [|Test1|]() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_AssemblyTestCaseOrderer_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            [assembly: TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
            public class MyTests
            {
                [DependsOn]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_ClassTestCaseOrderer_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            [TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
            public class MyTests
            {
                [DependsOn]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static AnalyzerTest<DefaultVerifier> GetAnalyzer(string source) => new CSharpAnalyzerTest<AttributeUsageAnalyzer, DefaultVerifier>
    {
        TestCode = source,
        TestState =
        {
            AdditionalReferences = { typeof(DependsOnAttribute).Assembly }
        },
        ReferenceAssemblies = ReferenceAssemblies.Net.Net90.AddPackages(new PackageIdentity[]
        {
            new PackageIdentity("xunit.v3", "3.0.0")
        }.ToImmutableArray()),
    };
}