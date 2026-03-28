using System.Globalization;
using Xunit.Sdk;
using Xunit.v3.IntegrationTesting.Exceptions;
using Xunit.v3.IntegrationTesting.Extensions;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Orders test collections based on dependencies declared via <see cref="DependsOnCollectionsAttribute"/>.
/// Subclass and override <see cref="CompareTestCollections"/> to provide secondary ordering
/// (e.g., by priority) for collections at the same dependency level.
/// </summary>
public class DependencyAwareTestCollectionOrderer : ITestCollectionOrderer
{
    /// <summary>
    /// Compares two test collections for ordering within the same dependency level.
    /// Override to provide secondary ordering (e.g., by priority) among collections
    /// that have no dependency relationship.
    /// <para>
    /// Return a negative value if <paramref name="x"/> should run before <paramref name="y"/>,
    /// zero if no preference, or a positive value if <paramref name="y"/> should run first.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This comparison only affects ordering of collections that are at the same "level" in the
    /// dependency graph. It cannot override dependency constraints.
    /// </remarks>
    protected virtual int CompareTestCollections(ITestCollection x, ITestCollection y) => 0;

    /// <inheritdoc />
    public IEnumerable<TTestCollection> OrderTestCollections<TTestCollection>(IEnumerable<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
        => OrderTestCollectionsCore(testCollections);

    /// <inheritdoc />
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
        => OrderTestCollectionsCore(testCollections).Cast<TTestCollection>().ToList();

    private IEnumerable<TTestCollection> OrderTestCollectionsCore<TTestCollection>(IEnumerable<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
    {
        try
        {
            var graph = testCollections.CollectionsToOrientedGraph(out var issues);
            var orderedCollections = graph.TopologicalSort((x, y) => CompareTestCollections(x, y));

            foreach (var issue in issues)
            {
                TestContext.Current.SendDiagnosticMessage("[TEST COLLECTION ORDERER WARNING] " + issue);
            }

            return orderedCollections;
        }
        catch (CircularDependencyException<IXunitTestCollection> ex)
        {
            var circularDependencyMessage = string.Format(
                CultureInfo.CurrentCulture,
                "There's a circular dependency involving the following test collections: {0}",
                string.Join(" -> ", ex.DependencyCycle.Select(tc => tc.TestCollectionDisplayName)));

            TestContext.Current.SendDiagnosticMessage("[TEST COLLECTION ORDERER ERROR] " +
                circularDependencyMessage);

            throw new TestPipelineException(
                circularDependencyMessage, ex);
        }
        catch (Exception ex)
        {
            var message = "An unexpected error occurred while ordering test collections: " + ex.Message;
            TestContext.Current.SendDiagnosticMessage("[TEST COLLECTION ORDERER ERROR] " + message);
            throw new TestPipelineException(message, ex);
        }
    }
}