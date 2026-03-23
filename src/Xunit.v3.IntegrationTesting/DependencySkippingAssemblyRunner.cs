using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;
using Xunit.v3.IntegrationTesting.Extensions;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Custom assembly runner that extends <see cref="XunitTestAssemblyRunner"/> with
/// automatic collection-level dependency skipping. Before running a collection,
/// checks whether all upstream collection dependencies (declared via
/// <see cref="DependsOnCollectionsAttribute"/>) have passed. If any dependency
/// failed or did not run, all tests in the collection are skipped.
/// <para>
/// This allows tests to use plain [Fact]/[Theory] attributes while still getting
/// dependency-aware collection-level skipping — no [FactDependsOn] required.
/// </para>
/// </summary>
public class DependencySkippingAssemblyRunner : XunitTestAssemblyRunner
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="DependencySkippingAssemblyRunner"/>.
    /// </summary>
    public static new DependencySkippingAssemblyRunner Instance { get; } = new();

    private const string SkipReason = "One or more collection dependencies were skipped or had failed.";

    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTestCollection(
        XunitTestAssemblyRunnerContext ctxt,
        IXunitTestCollection testCollection,
        IReadOnlyCollection<IXunitTestCase> testCases)
    {
        if (ShouldSkipCollection(testCollection))
        {
            var summary = XunitRunnerHelper.SkipTestCases(
                ctxt.MessageBus,
                ctxt.CancellationTokenSource,
                testCases,
                SkipReason,
                sendTestCollectionMessages: true,
                sendTestClassMessages: true,
                sendTestMethodMessages: true,
                sendTestCaseMessages: true,
                sendTestMessages: true);

            return new ValueTask<RunSummary>(summary);
        }

        return base.RunTestCollection(ctxt, testCollection, testCases);
    }

    private static bool ShouldSkipCollection(IXunitTestCollection testCollection)
    {
        var dependsOn = testCollection.CollectionDefinition?.GetCustomAttribute<DependsOnCollectionsAttribute>(false);

        if (dependsOn == null)
            return false;

        var dependencies = dependsOn.Dependencies;
        if (dependencies == null || dependencies.Length == 0)
            return false;

        foreach (var dependency in dependencies)
        {
            var collectionName = dependency.GetCollectionDefinitionName();

            var collectionResults = TestContext.Current.KeyValueStorage.Keys
                .Where(k => k.StartsWith($"{collectionName}.", StringComparison.Ordinal))
                .Select(k => TestContext.Current.KeyValueStorage[k])
                .OfType<string>()
                .Select(r => Enum.TryParse<TestResult>(r, out var tr) ? tr : (TestResult?)null)
                .Where(r => r.HasValue)
                .ToList();

            if (collectionResults.Count == 0 || collectionResults.Any(r => r != TestResult.Passed))
            {
                return true;
            }
        }

        return false;
    }
}
