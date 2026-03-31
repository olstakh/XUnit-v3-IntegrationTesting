using Xunit.Sdk;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Executor that extends <see cref="DependencyAwareFrameworkExecutor"/> by using
/// <see cref="DependencySkippingAssemblyRunner"/> instead of the default runner.
/// This enables automatic collection-level skipping for all test attributes.
/// </summary>
public class DependencySkippingExecutor(IXunitTestAssembly testAssembly)
    : DependencyAwareFrameworkExecutor(testAssembly)
{
    /// <inheritdoc />
    protected override async ValueTask RunAssembly(
        IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        await DependencySkippingAssemblyRunner.Instance.Run(
            TestAssembly,
            testCases,
            executionMessageSink,
            executionOptions,
            cancellationToken);
    }
}
