using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageDependsOnAttributeAnalyzerTests
{
    [Fact]
    public async Task Validate_FactInClassWithDependsOnClasses_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [typeof(OtherClass)], Name = ""MyCollection"")]
            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }
            }

            public class OtherClass;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactInClassWithCollectionTypeRefAndDependencies_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"", DisableParallelization = true)]
            [DependsOnCollections(typeof(OtherCollectionDef))]
            public sealed class MyCollectionDef;

            [CollectionDefinition(""OtherCollection"")]
            public sealed class OtherCollectionDef;

            [Collection(typeof(MyCollectionDef))]
            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactInClassWithCollectionNameRefAndDependencies_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"", DisableParallelization = true)]
            [DependsOnCollections(typeof(OtherCollectionDef))]
            public sealed class MyCollectionDef;

            [CollectionDefinition(""OtherCollection"")]
            public sealed class OtherCollectionDef;

            [Collection(""MyCollection"")]
            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }
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

            [DependsOnClasses(Dependencies = [typeof(OtherClass)], Name = ""MyCollection"")]
            public class MyTests
            {
                [{|XIT0008:Fact|}]
                public void Test1() { }

                [{|XIT0008:Fact|}]
                public void Test2() { }

                [FactDependsOn]
                public void Test3() { }
            }

            public class OtherClass;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactDependsOnInClassWithDependencies_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [typeof(OtherClass)], Name = ""MyCollection"")]
            public class MyTests
            {
                [FactDependsOn]
                public void Test1() { }

                [FactDependsOn]
                public void Test2() { }
            }

            public class OtherClass;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactInClassWithDependsOnClassesNoDeps_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Name = ""MyCollection"")]
            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_FactInClassWithoutCollectionDeps_NoDiagnosticAsync()
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

    [Fact]
    public async Task Validate_FactInClassWithCollectionNoDependencies_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [CollectionDefinition(""MyCollection"")]
            public sealed class MyCollectionDef;

            [Collection(typeof(MyCollectionDef))]
            public class MyTests
            {
                [Fact]
                public void Test1() { }
            }
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpAnalyzerTest<AttributeUsageDependsOnAttributeAnalyzer, DefaultVerifier> GetAnalyzer(string source) =>
        new CSharpAnalyzerTest<AttributeUsageDependsOnAttributeAnalyzer, DefaultVerifier>
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
