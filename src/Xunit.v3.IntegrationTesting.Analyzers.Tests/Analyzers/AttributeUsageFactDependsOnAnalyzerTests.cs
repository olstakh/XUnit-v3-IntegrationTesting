using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageFactDependsOnAnalyzerTests
{
    [Fact]
    public async Task Validate_FactInClassWithFactDependsOn_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }

                [FactDependsOn]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactAttributeWithProperties_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [{|XIT0008:FactAttribute(Skip = ""reason"")|}]
                public void Test1() { }

                [FactDependsOn]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_MultipleFactMethods_AllFlagged_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }

                [{|XIT0008:Fact|}]
                public void Test2() { }

                [FactDependsOn]
                public void Test3() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_AllFactDependsOn_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn]
                public void Test1() { }

                [FactDependsOn]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_AllFact_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [Fact]
                public void Test1() { }

                [Fact]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpAnalyzerTest<AttributeUsageFactDependsOnAnalyzer, DefaultVerifier> GetAnalyzer(string source) =>
        new CSharpAnalyzerTest<AttributeUsageFactDependsOnAnalyzer, DefaultVerifier>
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
