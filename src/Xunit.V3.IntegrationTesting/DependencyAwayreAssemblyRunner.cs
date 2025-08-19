using System.Runtime.CompilerServices;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareAssemblyRunner : XunitTestAssemblyRunnerBase<XunitTestAssemblyRunnerContext, IXunitTestAssembly, IXunitTestCollection, IXunitTestCase>
{
    protected override ValueTask<RunSummary> RunTestCollection(XunitTestAssemblyRunnerContext ctxt, IXunitTestCollection testCollection, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        return base.RunTestCollection(ctxt, testCollection, testCases);
    }
}

public class DependencyAwareXunitTestAssemblyRunnerBaseContext : XunitTestAssemblyRunnerBaseContext<IXunitTestAssembly, IXunitTestCase>
{
    public DependencyAwareXunitTestAssemblyRunnerBaseContext(IXunitTestAssembly testAssembly, IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken)
        : base(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
    {
    }
}