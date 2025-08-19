using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public static class OrientedGraphExtensions
{
    public static OrientedGraph<TTestCase> ToOrientedGraph<TTestCase>(this IEnumerable<TTestCase> testCases, out List<string> issues)
        where TTestCase : notnull, ITestCase
    {
        var graph = new OrientedGraph<TTestCase>(TestCaseComparer<TTestCase>.Instance);
        issues = new List<string>();

        foreach (var tc in testCases)
        {
            if (tc is not IXunitTestCase testCase)
            {
                graph.AddNode(tc);
                continue; // Skip non-Xunit test cases
            }

            var dependsOnAttrs = testCase.TestMethod.Method.GetCustomAttribute<DependsOnAttribute>(false) ?? new(); // send diagnostic if null?
            foreach (var dependency in dependsOnAttrs.Dependencies)
            {
                var dependentTest = testCases.SingleOrDefault(tc => TestClassComparer.Instance.Equals(tc.TestMethod?.TestClass, testCase.TestClass) && tc.TestMethodName == dependency);
                if (dependentTest != null)
                {
                    graph.AddEdge(tc, dependentTest);
                }
                else
                {
                    issues.Add(
                        $"Dependency '{dependency}' for test '{testCase.TestClassName}.{testCase.TestMethodName}' not found.");
                }
            }
        }

        graph.ValidateNoCycles();

        return graph;
    }
}