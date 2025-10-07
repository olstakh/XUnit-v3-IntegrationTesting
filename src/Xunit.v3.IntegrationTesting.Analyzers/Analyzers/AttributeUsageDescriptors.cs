using Microsoft.CodeAnalysis;

namespace Xunit.v3.IntegrationTesting.Analyzers;

internal static class AttributeUsageDescriptors
{
    public static readonly DiagnosticDescriptor NotSupportedClassLevelTestCaseOrderer = new DiagnosticDescriptor(
        "XIT0001",
        "FactDependsOn attribute requires DependencyAwareTestCaseOrderer to respect test dependencies",
        "Method '{0}' uses [FactDependsOn] attribute, but class-level test orderer is not DependencyAwareTestCaseOrderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [FactDependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor NotSupportedAssemblyLevelTestCaseOrderer = new DiagnosticDescriptor(
        "XIT0002",
        "FactDependsOn attribute requires DependencyAwareTestCaseOrderer to respect test dependencies",
        "Method '{0}' uses [FactDependsOn] attribute, but assembly-level test orderer is not DependencyAwareTestCaseOrderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [FactDependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor MissingTestCaseAndCollectionOrderer = new DiagnosticDescriptor(
        "XIT0003",
        "FactDependsOn attribute requires DependencyAwareTestCaseOrderer or DependencyAwareTestCollectionOrderer to respect test dependencies",
        "Method '{0}' uses [FactDependsOn] attribute, but neither DependencyAwareTestCaseOrderer nor DependencyAwareTestCollectionOrderer are set as class-level or assembly-level test case orderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [FactDependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor DependsOnMissingMethod = new DiagnosticDescriptor(
        "XIT0004",
        "Missing test dependency",
        "Method '{0}' depends on method '{1}' but it is missing from the class",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "[FactDependsOn] attribute requires all listed dependencies to be present in the class.");

    public static readonly DiagnosticDescriptor MissingTestFrameworkAttribute = new DiagnosticDescriptor(
        "XIT0006",
        "Missing TestFramework assembly attribute",
        "Assembly is missing [assembly: TestFramework(typeof(DependencyAwareFramework))]. This can affect filtered test runs (filtering test cases in command line or selecting subset of tests in UI).",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: "Assemblies using FactDependsOn attribute on tests should declare [assembly: TestFramework(typeof(DependencyAwareFramework))] to support full test discovery during filtered test execution.");

    public static readonly DiagnosticDescriptor NotSupportedTestFrameworkAttribute = new DiagnosticDescriptor(
        "XIT0007",
        "Not supported TestFramework assembly attribute",
        "Assembly has existing [assembly: TestFramework(typeof(...))] attribute. Consider using DependencyAwareFramework instead. Otherwise filtered test runs might be affected.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd,
        description: "Assemblies using FactDependsOn attribute on tests should declare [assembly: TestFramework(typeof(DependencyAwareFramework))] to support full test discovery during filtered test execution.");

    public static readonly DiagnosticDescriptor UseFactDependsOnAttribute = new DiagnosticDescriptor(
        "XIT0008",
        "Use FactDependsOn attribute",
        "Method '{0}' should use [FactDependsOn] attribute to react to test dependencies",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "All test methods should use [FactDependsOn] attribute when corresponding collection definition has dependencies.");

    public static readonly DiagnosticDescriptor InvalidDependsOnCollectionsAttributeUsage = new DiagnosticDescriptor(
        "XIT0009",
        "Apply DependsOnCollections attribute to collection definitions",
        "Class '{0}' is not a collection definition, it cannot use [DependsOnCollections] attribute",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "DependsOnCollections attribute should apply only to collection definitions.");

    public static readonly DiagnosticDescriptor CollectionDefinitionMissingDisableParallelization = new DiagnosticDescriptor(
        "XIT0010",
        "CollectionDefinition with DependsOnCollections must have DisableParallelization set to true",
        "Collection definition '{0}' has DependsOnCollections attribute, but DisableParallelization is not set to true. This means that collections execution order won't be guaranteed.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Collection definitions with DependsOnCollections attribute must have DisableParallelization set to true to ensure sequential execution order according to declared dependencies.");

    public static readonly DiagnosticDescriptor MissingTestCollectionOrderer = new DiagnosticDescriptor(
        "XIT0011",
        "DependsOnCollections attribute requires assembly-level TestCollectionOrderer",
        "[DependsOnCollections] attribute requires assembly-level test collection orderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Usage of  [DependsOnCollections] requires DependencyAwareTestCollectionOrderer set as TestCollectionOrderer, in order for test collections to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor NotSupportedTestCollectionOrderer = new DiagnosticDescriptor(
        "XIT0012",
        "DependsOnCollections attribute requires assembly-level TestCollectionOrderer to be DependencyAwareTestCollectionOrderer to respect test dependencies",
        "[DependsOnCollections] attribute requires assembly-level test collection orderer to be DependencyAwareTestCollectionOrderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Usage of  [DependsOnCollections] requires DependencyAwareTestCollectionOrderer set as TestCollectionOrderer, in order for test collections to be ordered according to defined dependencies.");
}