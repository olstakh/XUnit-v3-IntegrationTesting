using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.v3.IntegrationTesting;
using Xunit.v3.IntegrationTesting.Analyzers;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageTestFrameworkAnalyzerTests
{
    [Fact]
    public async Task Validate_MissingTestFrameworkAttribute_DiagnosticAsync()
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
        analyzer.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning(AttributeUsageDescriptors.MissingTestFrameworkAttribute.Id).WithNoLocation());

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_NotSupportedTestFrameworkAttribute_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3;

            [assembly: TestFramework(typeof(CustomTestFramework))]

            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }

            public class CustomTestFramework : XunitTestFramework {}
        ";

        var analyzer = GetAnalyzer(source);
        analyzer.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning(AttributeUsageDescriptors.NotSupportedTestFrameworkAttribute.Id).WithNoLocation());

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }    

    [Fact]
    public async Task Validate_TestFrameworkAttribute_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3;
            using Xunit.v3.IntegrationTesting;

            [assembly: TestFramework(typeof(DependencyAwareFramework))]

            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static AnalyzerTest<DefaultVerifier> GetAnalyzer(string source) => new CSharpAnalyzerTest<AttributeUsageTestFrameworkAnalyzer, DefaultVerifier>
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