using System.Security.Cryptography;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly ITestFrameworkDiscoverer _discoverer;
    private readonly List<ITestCase> _testCases = new List<ITestCase>();

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

        await _discoverer.Find(
            async testCase =>
            {
                _testCases.Add(testCase);
                // Custom logic to handle dependencies can be added here
                return await Task.FromResult(true);
            },
            new DiscoveryOptions(),
            cancellationToken: cancellationToken);

        // TODO: filter out unnecessary tests
        await base.RunTestCases(_testCases.OfType<IXunitTestCase>().ToList(), executionMessageSink, executionOptions, cancellationToken);
    }
}

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