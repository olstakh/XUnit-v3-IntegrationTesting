using System.Globalization;

namespace Xunit.v3.IntegrationTesting.Analyzers;

internal static class DiagnosticIds
{
    public const string NotSupportedClassLevelTestCaseOrderer = "XIT0001";
    public const string NotSupportedAssemblyLevelTestCaseOrderer = "XIT0002";
    public const string MissingTestCaseAndCollectionOrderer = "XIT0003";
    public const string DependsOnMissingMethod = "XIT0004";
    public const string MissingTestFrameworkAttribute = "XIT0006";
    public const string NotSupportedTestFrameworkAttribute = "XIT0007";
    public const string UseFactDependsOnAttribute = "XIT0008";
    public const string InvalidDependsOnCollectionsAttributeUsage = "XIT0009";
    public const string CollectionDefinitionMissingDisableParallelization = "XIT0010";
    public const string MissingTestCollectionOrderer = "XIT0011";
    public const string NotSupportedTestCollectionOrderer = "XIT0012";
    public const string DependsOnClassesDependencyAlreadyInCollection = "XIT0013";
    public const string DependsOnClassesDependencyNotInCollection = "XIT0014";
    public const string MultipleDependsOnAttributes = "XIT0015";
    public const string DependsOnWithOtherFactAttributes = "XIT0016";
    public static string CreateLink(string id) =>
        string.Format(CultureInfo.InvariantCulture, "https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/docs/Rules/{0}.md", id);
}
