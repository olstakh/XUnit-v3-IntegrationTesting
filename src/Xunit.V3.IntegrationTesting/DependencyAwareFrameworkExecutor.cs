using System.Security.Cryptography;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly ITestFrameworkDiscoverer _discoverer;

    public DependencyAwareFrameworkExecutor(IXunitTestAssembly testAssembly, ITestFrameworkDiscoverer? discoverer)
        : base(testAssembly)
    {
        _discoverer = discoverer ?? new DependencyAwareTestDiscoverer(base.CreateDiscoverer());
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer()
    {
        return _discoverer;
    }

    public override async ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken)
    {
        executionMessageSink.OnMessage(new DiagnosticMessage("Running test cases using custom dependency-aware executor..."));

        List<IXunitTestCase> allTestCases = new();

        await _discoverer.Find(
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

        HashSet<IXunitTestCase> necessaryTests = new();
        foreach (var testCase in testCases)
        {
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

        // TODO: filter out unnecessary tests
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