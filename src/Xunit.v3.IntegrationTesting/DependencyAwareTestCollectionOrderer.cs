using System.Globalization;
using Xunit.Sdk;
using Xunit.v3.IntegrationTesting.Extensions;
using Xunit.v3.IntegrationTesting.Exceptions;

namespace Xunit.v3.IntegrationTesting;

public class DependencyAwareTestCollectionOrderer : ITestCollectionOrderer
{
    public IEnumerable<TTestCollection> OrderTestCollections<TTestCollection>(IEnumerable<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
    {
        try
        {
            var graph = testCollections.CollectionsToOrientedGraph(out var issues);
            var orderedCollections = graph.TopologicalSort();

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

    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
        where TTestCollection : ITestCollection
    {
        return OrderTestCollections(testCollections.AsEnumerable()).Cast<TTestCollection>().ToList();
    }
}