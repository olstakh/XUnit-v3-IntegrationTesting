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
        try
        {
            var graph = testCases.ToOrientedGraph(out var issues);
            var orderedCases = graph.TopologicalSort();

            foreach (var issue in issues)
            {
                TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER WARNING] " + issue);
            }

            return orderedCases.Cast<TTestCase>();
        }
        catch (CircularDependencyException<IXunitTestCase> ex)
        {
            var circularDependencyMessage = string.Format(
                CultureInfo.CurrentCulture,
                "There's a circular dependency involving the following tests: {0}",
                string.Join(" -> ", ex.DependencyCycle.Select(tc => $"{tc.TestClassName}.{tc.TestMethodName}")));

            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " +
                circularDependencyMessage);

            throw new TestPipelineException(
                circularDependencyMessage, ex);
        }
        catch (Exception ex)
        {
            var message = "An unexpected error occurred while ordering test cases: " + ex.Message;
            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " + message);
            throw new TestPipelineException(message, ex);
        }
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToList();
    }
}