using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageDependenciesAnalyzerTests
{
    [Theory]
    [InlineData("[NonExistentMethodConst]")]
    [InlineData("[\"NonExistentMethod\"]")]
    [InlineData("new [] { NonExistentMethodConst }")]
    [InlineData("new [] {\"NonExistentMethod\" }")]
    [InlineData("new string [] { NonExistentMethodConst }")]
    [InlineData("new string [] {\"NonExistentMethod\" }")]
    public async Task Validate_DependsOn_MissingMethod_DiagnosticAsync(string dependencyDeclaration)
    {
        var source = /* lang=c#-test */ @"
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                const string NonExistentMethodConst = ""NonExistentMethod"";

                [FactDependsOn(Dependencies = PLACEHOLDER)]
                public void {|XIT0004:Test1|}() { }
            }
        ";

        var analyzer = GetAnalyzer(source.Replace("PLACEHOLDER", dependencyDeclaration));

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("[InvalidMethodConst]")]
    [InlineData("[\"InvalidMethod\"]")]
    [InlineData("[nameof(InvalidMethod)]")]
    [InlineData("new [] { InvalidMethodConst }")]
    [InlineData("new [] {\"InvalidMethod\" }")]
    [InlineData("new [] {nameof(InvalidMethod) }")]
    [InlineData("new string [] { InvalidMethodConst }")]
    [InlineData("new string [] {\"InvalidMethod\" }")]
    [InlineData("new string [] {nameof(InvalidMethod) }")]
    public async Task Validate_DependsOn_InvalidMethod_DiagnosticAsync(string dependencyDeclaration)
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                const string InvalidMethodConst = nameof(InvalidMethod);

                [FactDependsOn(Dependencies = PLACEHOLDER)]
                public void {|XIT0005:Test1|}() { }

                [Fact]
                public void InvalidMethod() { }
            }
        ";

        var analyzer = GetAnalyzer(source.Replace("PLACEHOLDER", dependencyDeclaration));

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependsOn_ValidMethod_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn(Dependencies = [""Test2""])]
                public void Test1() { }

                [FactDependsOn]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source);

        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static AnalyzerTest<DefaultVerifier> GetAnalyzer(string source) => new CSharpAnalyzerTest<AttributeUsageDependenciesAnalyzer, DefaultVerifier>
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