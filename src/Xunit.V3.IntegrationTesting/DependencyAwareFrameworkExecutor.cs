using System.Security.Cryptography;
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
                    executionMessageSink.OnMessage(new DiagnosticMessage($"Skipping non-Xunit test case: {testCase.TestClassName}.{testCase.TestMethodName}"));
                }
                return await Task.FromResult(true);
            },
            new DiscoveryOptions(),
            cancellationToken: cancellationToken);

        // TODO: filter out unnecessary tests
        await base.RunTestCases(allTestCases.OfType<IXunitTestCase>().ToList(), executionMessageSink, executionOptions, cancellationToken);
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