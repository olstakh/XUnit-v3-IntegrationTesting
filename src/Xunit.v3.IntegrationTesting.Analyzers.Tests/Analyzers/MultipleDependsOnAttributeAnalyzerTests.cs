using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class MultipleDependsOnAttributeAnalyzerTests
{
    [Fact]
    public async Task Validate_MultipleDependsOnAttributes_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn]
                [TheoryDependsOn]
                public void {|XIT0013:Test1|}() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_MultipleDependsOnAttributes_WithDependencies_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn(Dependencies = [""Test2""])]
                [TheoryDependsOn]
                public void {|XIT0013:Test1|}() { }

                [FactDependsOn]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_SingleFactDependsOn_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_SingleTheoryDependsOn_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [TheoryDependsOn]
                [InlineData(1)]
                public void Test1(int x) { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_NoDependsOnAttributes_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;

            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpAnalyzerTest<MultipleDependsOnAttributeAnalyzer, DefaultVerifier> GetAnalyzer(string source) =>
        new CSharpAnalyzerTest<MultipleDependsOnAttributeAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            TestState =
            {
                AdditionalReferences = { typeof(FactDependsOnAttribute).Assembly }
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100.AddPackages(new PackageIdentity[]
            {
                new PackageIdentity("xunit.v3", "3.0.0")
            }.ToImmutableArray()),
        };
}
