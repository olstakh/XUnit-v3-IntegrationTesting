using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;
using Xunit.v3.IntegrationTesting.Comparers;

namespace Xunit.v3.IntegrationTesting.Extensions;

internal static class OrientedGraphExtensions
{
    public static OrientedGraph<TTestCase> ToOrientedGraph<TTestCase>(this IEnumerable<TTestCase> testCases, out List<string> issues)
        where TTestCase : notnull, ITestCase
    {
        var graph = new OrientedGraph<TTestCase>(TestCaseComparer<TTestCase>.Instance);
        issues = new List<string>();

        foreach (var tc in testCases)
        {
            graph.AddNode(tc);
            if (tc is not IXunitTestCase testCase)
            {
                continue; // Skip non-Xunit test cases
            }

            var dependsOnAttrs = testCase.TestMethod.Method.GetCustomAttribute<FactDependsOnAttribute>(false) ?? new(); // send diagnostic if null?
            foreach (var dependency in dependsOnAttrs.Dependencies)
            {
                var dependentTest = testCases.Where(tc => TestClassComparer.Instance.Equals(tc.TestMethod?.TestClass, testCase.TestClass) && tc.TestMethodName == dependency).ToList();
                if (dependentTest.Count > 1)
                {
                    throw new Exception(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Multiple tests found with the same name '{0}' in class '{1}'. Total test cases: '{2}'. This is not allowed.",
                            dependency,
                            testCase.TestClassName,
                            dependentTest.Count()));
                }
                if (dependentTest.Count > 0)
                {
                    graph.AddEdge(tc, dependentTest.Single());
                }
                else
                {
                    issues.Add(
                        $"Dependency '{dependency}' for test '{testCase.TestClassName}.{testCase.TestMethodName}' not found.");
                }
            }
        }

        return graph;
    }

    public static OrientedGraph<TTestCollection> CollectionsToOrientedGraph<TTestCollection>(this IEnumerable<TTestCollection> testCollections, out List<string> issues)
        where TTestCollection : ITestCollection
    {
        var graph = new OrientedGraph<TTestCollection>(TestCollectionComparerLocal<TTestCollection>.Instance);
        issues = new List<string>();
        
        HashSet<IXunitTestCollection> collectionsWithParallelism = new();

        foreach (var tc in testCollections)
        {
            graph.AddNode(tc);
            if (tc is not IXunitTestCollection testCollection)
            {
                continue; // Skip non-Xunit test collections
            }
            var collectionDefinition = testCollection.CollectionDefinition;
            if (collectionDefinition == null)
            {
                continue; // Dependencies can be declared only on collection definitions
            }

            bool hasAtLeastOneDependency = false;
            var dependsOnAttrs = collectionDefinition.GetCustomAttribute<DependsOnCollectionsAttribute>(false);
            if (dependsOnAttrs == null)
            {
                continue; // No dependencies
            }

            foreach (var dependency in dependsOnAttrs.Dependencies)
            {
                var dependencyName = dependency.GetCollectionDefinitionName();

                var dependentCollections = testCollections.Where(t => t is IXunitTestCollection tc && tc.TestCollectionDisplayName == dependencyName).ToList();
                if (dependentCollections.Count > 1)
                {
                    throw new Exception(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Multiple test collections found with the same name '{0}'. Total collections: '{1}'. This is not allowed.",
                            dependency,
                            dependentCollections.Count()));
                }
                if (dependentCollections.Count > 0)
                {
                    var dependentCollection = dependentCollections.Single();
                    if (dependentCollection as IXunitTestCollection is { DisableParallelization: false } y)
                    {
                        collectionsWithParallelism.Add(y);
                    }
                    hasAtLeastOneDependency = true;
                    graph.AddEdge(tc, dependentCollection);
                }
                else
                {
                    issues.Add(
                        $"Dependency '{dependency.Name}' for test collection '{tc.TestCollectionDisplayName}' not found.");
                }
            }

            if (hasAtLeastOneDependency && testCollection is IXunitTestCollection { DisableParallelization: false } x)
            {
                collectionsWithParallelism.Add(x);
            }
        }

        foreach (var c in collectionsWithParallelism)
        {
            issues.Add(
                $"Test collection '{c.TestCollectionDisplayName}' has dependencies (or is dependent on) and does not have parallelization disabled. " +
                "This means its order is not guaranteed. To fix this, add [CollectionDefinition(DisableParallelization = true)] to the collection definition class.");
        }

        return graph;
    }
}
