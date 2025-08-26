using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.v3.IntegrationTesting.Analyzers.Fixers;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests.Fixers;

public class FactDependsOnAttributeFixerTests
{
    [Fact]
    public async Task Replaces_Fact_With_FactDependsOnAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }

                [{|XIT0008:FactAttribute(Skip = ""reason"")|}]
                public void Test2() { }
            }
        ";

        var fixedSource = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            public class MyTests
            {
                [FactDependsOn]
                public void Test1() { }

                [FactDependsOn(Skip = ""reason"")]
                public void Test2() { }
            }
        ";

        var analyzer = GetAnalyzer(source, fixedSource);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CodeFixTest<DefaultVerifier> GetAnalyzer(string source, string fixedCode) => new CSharpCodeFixTest<AttributeUsageDependenciesAnalyzer, FactDependsOnAttributeFixer, DefaultVerifier>
    {
        TestCode = source,
        FixedCode = fixedCode,
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