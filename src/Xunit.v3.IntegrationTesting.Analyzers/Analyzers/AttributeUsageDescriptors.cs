using Microsoft.CodeAnalysis;

namespace Xunit.v3.IntegrationTesting.Analyzers;

internal static class AttributeUsageDescriptors
{
    public static readonly DiagnosticDescriptor NotSupportedClassLevelTestCaseOrderer = new DiagnosticDescriptor(
        "XIT0001",
        "DependsOn attribute requires DependencyAwareTestCaseOrderer to respect test dependencies",
        "Method '{0}' uses [DependsOn] attribute, but class-level test orderer is not DependencyAwareTestCaseOrderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [DependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor NotSupportedAssemblyLevelTestCaseOrderer = new DiagnosticDescriptor(
        "XIT0002",
        "DependsOn attribute requires DependencyAwareTestCaseOrderer to respect test dependencies",
        "Method '{0}' uses [DependsOn] attribute, but assembly-level test orderer is not DependencyAwareTestCaseOrderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [DependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor MissingTestCaseOrderer = new DiagnosticDescriptor(
        "XIT0003",
        "DependsOn attribute requires DependencyAwareTestCaseOrderer to respect test dependencies",
        "Method '{0}' uses [DependsOn] attribute, but DependencyAwareTestCaseOrderer is not set as class-level or assembly-level test case orderer",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Any method with [DependsOn] must have DependencyAwareTestCaseOrderer set TestCaseOrderer, in order for test to be ordered according to defined dependencies.");

    public static readonly DiagnosticDescriptor DependsOnMissingMethod = new DiagnosticDescriptor(
        "XIT0004",
        "Missing test dependency",
        "Method '{0}' depends on method '{1}' but it is missing from the class",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "[DependsOn] attribute requires all listed dependencies to be present in the class.");

    public static readonly DiagnosticDescriptor DependsOnInvalidMethod = new DiagnosticDescriptor(
        "XIT0005",
        "Invalid test dependency",
        "Method '{0}' depends on method '{1}', which should be decorated with [DependsOn] attribute",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All test dependencies should be decorated with [DependsOn] attribute.");
}