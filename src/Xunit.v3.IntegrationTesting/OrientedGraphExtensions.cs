using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting;

public static class OrientedGraphExtensions
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
}