using System.Globalization;
using System.Security.Cryptography;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting;

public class DependencyAwareFrameworkExecutor(IXunitTestAssembly testAssembly) : XunitTestFrameworkExecutor(testAssembly)
{
    public override async ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken)
    {
        executionMessageSink.OnMessage(new DiagnosticMessage("Running test cases using custom dependency-aware executor..."));

        List<IXunitTestCase> allTestCases = new();

        var discoverer = CreateDiscoverer();

        await discoverer.Find(
            async testCase =>
            {
                if (testCase is IXunitTestCase xunitTestCase)
                {
                    allTestCases.Add(xunitTestCase);
                }
                else
                {
                    executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE DISCOVERER WARNING] Skipping non-Xunit test case: {testCase.TestClassName}.{testCase.TestMethodName}"));
                }
                return await Task.FromResult(true);
            },
            new DiscoveryOptions(),
            cancellationToken: cancellationToken);

        var graph = allTestCases.ToOrientedGraph(out var issues);
        foreach (var issue in issues)
        {
            executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE EXECUTOR WARNING] {issue}"));
        }

        var collectionGraph = allTestCases.Select(x => x.TestCollection).Distinct().CollectionsToOrientedGraph(out var collectionIssues);
        foreach (var issue in collectionIssues)
        {
            executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE EXECUTOR WARNING] {issue}"));
        }

        HashSet<IXunitTestCase> necessaryTests = new(TestCaseComparer<IXunitTestCase>.Instance);
        foreach (var testCase in testCases)
        {
            if (collectionGraph.ContainsNode(testCase.TestCollection))
            {
                var collectionsToAdd = collectionGraph.GetSubTree(testCase.TestCollection);
                necessaryTests.AddRange(allTestCases.Where(tc => collectionsToAdd.Contains(tc.TestCollection)));
            }
            else
            {
                executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE EXECUTOR WARNING] Test collection {testCase.TestCollection.TestCollectionDisplayName} was not discovered"));
            }

            if (graph.ContainsNode(testCase))
            {
                necessaryTests.AddRange(graph.GetSubTree(testCase));
            }
            else
            {
                executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE EXECUTOR WARNING] Test case {testCase.TestClassName}.{testCase.TestMethodName} was not discovered"));
                necessaryTests.Add(testCase);
            }
        }

        if (testCases.Count < necessaryTests.Count)
        {
            executionMessageSink.OnMessage(new DiagnosticMessage($"[TEST CASE EXECUTOR INFO] {testCases.Count} tests were requested - {necessaryTests.Count} will be executed due to dependencies between tests"));
        }

        await base.RunTestCases(necessaryTests, executionMessageSink, executionOptions, cancellationToken);
    }
}

// How to get it properly? Does it matter for executor when we want to load all tests?
class DiscoveryOptions : ITestFrameworkDiscoveryOptions
{
    public TValue? GetValue<TValue>(string name)
    {
        return default(TValue);
    }

    public void SetValue<TValue>(string name, TValue value)
    {
    }

    public string ToJson()
    {
        return "{}";
    }
}