using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyTestCaseOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        try
        {
            var orderedCases = DependencyResolver.OrderTestsByDependencies(testCases.Cast<IXunitTestCase>());
            return orderedCases.Cast<TTestCase>();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circular dependency"))
        {
            throw new Exception($"Test ordering failed: {ex.Message}");
        }
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToList();
    }
}