using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting;

public class DependencyAwareTestCaseOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
    {
        var graph = testCases.ToOrientedGraph(out var issues);
        var cycle = graph.FindCycle();

        if (cycle.Count > 0)
        {
            var circularDependencyMessage = string.Format(
                CultureInfo.CurrentCulture,
                "There's a circular dependency involving the following tests: {0}",
                string.Join(" -> ", cycle.Select(tc => $"{tc.TestClassName}.{tc.TestMethodName}")));

            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " +
                circularDependencyMessage);

            throw new TestPipelineException(
                circularDependencyMessage);
        }

        var orderedCases = graph.TopologicalSort();
        foreach (var issue in issues)
        {
            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER WARNING] " + issue);
        }

        return orderedCases.Cast<TTestCase>();
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToList();
    }
}