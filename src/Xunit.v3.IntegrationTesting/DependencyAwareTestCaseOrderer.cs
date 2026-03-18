using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;
using Xunit.v3.IntegrationTesting.Extensions;
using Xunit.v3.IntegrationTesting.Exceptions;

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

            // Return tests in original order instead of throwing, which causes
            // a catastrophic failure in xUnit. The diagnostic message above is
            // sufficient to surface the problem.
            return testCases.ToArray();
        }
        catch (Exception ex)
        {
            var message = "An unexpected error occurred while ordering test cases: " + ex.Message;
            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " + message);

            // Return tests in original order instead of throwing.
            return testCases.ToArray();
        }
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToList();
    }
}