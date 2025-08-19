using System.Collections.Generic;
using System.Globalization;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyTestCaseOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
    {
        try
        {
            var orderedCases = testCases.ToOrientedGraph(out var issues).TopologicalSort();
            foreach (var issue in issues)
            {
                TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER WARNING] " + issue);
            }

            return orderedCases.Cast<TTestCase>();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circular dependency"))
        {
            throw new TestPipelineException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    // Produce a list of circular dependency chain in the exception
                     $"There's a circular dependency involving the following tests: TBD"));
        }
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToList();
    }
}