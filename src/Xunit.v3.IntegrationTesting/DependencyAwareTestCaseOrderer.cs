using System.Globalization;
using Xunit.Sdk;
using Xunit.v3.IntegrationTesting.Exceptions;
using Xunit.v3.IntegrationTesting.Extensions;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Orders test cases based on dependencies declared via <see cref="DependsOnAttributeBase"/>.
/// Subclass and override <see cref="CompareTestCases"/> to provide secondary ordering
/// (e.g., by priority) for test cases at the same dependency level.
/// </summary>
public class DependencyAwareTestCaseOrderer : ITestCaseOrderer
{
    /// <summary>
    /// Compares two test cases for ordering within the same dependency level.
    /// Override to provide secondary ordering (e.g., by priority) among tests
    /// that have no dependency relationship.
    /// <para>
    /// Return a negative value if <paramref name="x"/> should run before <paramref name="y"/>,
    /// zero if no preference, or a positive value if <paramref name="y"/> should run first.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This comparison only affects ordering of tests that are at the same "level" in the
    /// dependency graph. It cannot override dependency constraints: if test A depends on
    /// test B, B will always run before A regardless of this comparison.
    /// </remarks>
    protected virtual int CompareTestCases(ITestCase x, ITestCase y) => 0;

    /// <inheritdoc />
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
        => OrderTestCasesCore(testCases);

    /// <inheritdoc />
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
        => OrderTestCasesCore(testCases).ToList();

    private IEnumerable<TTestCase> OrderTestCasesCore<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : notnull, ITestCase
    {
        try
        {
            var graph = testCases.ToOrientedGraph(out var issues);
            var orderedCases = graph.TopologicalSort((x, y) => CompareTestCases(x, y));

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

            return testCases.ToArray();
        }
        catch (Exception ex)
        {
            var message = "An unexpected error occurred while ordering test cases: " + ex.Message;
            TestContext.Current.SendDiagnosticMessage("[TEST CASE ORDERER ERROR] " + message);

            return testCases.ToArray();
        }
    }
}