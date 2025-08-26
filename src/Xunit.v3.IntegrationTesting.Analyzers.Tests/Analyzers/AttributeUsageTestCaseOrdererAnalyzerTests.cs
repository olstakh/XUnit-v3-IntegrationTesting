using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageTestCaseOrdererAnalyzerTests
{
    [Fact]
    public async Task Validate_DependsOn_NotSupportedClassLevelTestCaseOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using System.Collections.Generic;
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3;
            using Xunit.v3.IntegrationTesting;

            [TestCaseOrderer(typeof(CustomOrderer))]
            public class MyTests
            {
                [FactDependsOn]
                public void {|XIT0001:Test1|}() { }
            }

            public class CustomOrderer : ITestCaseOrderer
            {
                public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
                    where TTestCase : notnull, ITestCase
                {
                    return testCases;
                }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_NotSupportedAssemblyLevelTestCaseOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using System.Collections.Generic;
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3;
            using Xunit.v3.IntegrationTesting;

            [assembly: TestCaseOrderer(typeof(CustomOrderer))]
            public class MyTests
            {
                [FactDependsOn]
                public void {|XIT0002:Test1|}() { }
            }

            public class CustomOrderer : ITestCaseOrderer
            {
                public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
                    where TTestCase : notnull, ITestCase
                {
                    return testCases;
                }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_MissingTestCaseOrderer_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn]
                public void {|XIT0003:Test1|}() { }
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
                [FactDependsOn]
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
                [FactDependsOn]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_NoDependsOn_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.Sdk;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }    

    private static AnalyzerTest<DefaultVerifier> GetAnalyzer(string source) => new CSharpAnalyzerTest<AttributeUsageTestCaseOrdererAnalyzer, DefaultVerifier>
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