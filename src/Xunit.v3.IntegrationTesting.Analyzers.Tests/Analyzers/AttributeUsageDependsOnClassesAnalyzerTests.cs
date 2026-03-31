using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.v3.IntegrationTesting.Analyzers.Tests;

public class AttributeUsageDependsOnClassesAnalyzerTests
{
    [Fact]
    public async Task Validate_DependencyWithCollectionAttribute_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [{|XIT0013:typeof(ClassB)|}], Name = ""CollA"")]
            public class ClassA;

            [Collection(""MyCollection"")]
            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyWithCollectionDefinitionAttribute_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [{|XIT0013:typeof(ClassB)|}], Name = ""CollA"")]
            public class ClassA;

            [CollectionDefinition(""MyCollection"")]
            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyPlainClass_CollectionPerAssembly_DiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

            [DependsOnClasses(Dependencies = [{|XIT0014:typeof(ClassB)|}], Name = ""CollA"")]
            public class ClassA;

            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyPlainClass_DefaultBehavior_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [typeof(ClassB)], Name = ""CollA"")]
            public class ClassA;

            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyPlainClass_CollectionPerClass_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]

            [DependsOnClasses(Dependencies = [typeof(ClassB)], Name = ""CollA"")]
            public class ClassA;

            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyWithDependsOnClasses_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [typeof(ClassB)], Name = ""CollA"")]
            public class ClassA;

            [DependsOnClasses(Name = ""CollB"")]
            public class ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_DependencyInheritsFromDependsOnClasses_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Dependencies = [typeof(ClassC)], Name = ""CollA"")]
            public class ClassA;

            [DependsOnClasses(Name = ""CollB"")]
            public class ClassB;

            public class ClassC : ClassB;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_NoDependencies_NoDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [DependsOnClasses(Name = ""CollA"")]
            public class ClassA;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Validate_MultipleDependencies_MixedDiagnosticAsync()
    {
        var source = /* lang=c#-test */ @"
            using Xunit;
            using Xunit.v3.IntegrationTesting;

            [assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

            [DependsOnClasses(Dependencies = [typeof(ClassB), {|XIT0014:typeof(ClassC)|}, {|XIT0013:typeof(ClassD)|}], Name = ""CollA"")]
            public class ClassA;

            [DependsOnClasses(Name = ""CollB"")]
            public class ClassB;

            public class ClassC;

            [Collection(""SomeCollection"")]
            public class ClassD;
        ";

        var analyzer = GetAnalyzer(source);
        await analyzer.RunAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpAnalyzerTest<AttributeUsageDependsOnClassesAnalyzer, DefaultVerifier> GetAnalyzer(string source) =>
        new CSharpAnalyzerTest<AttributeUsageDependsOnClassesAnalyzer, DefaultVerifier>
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
