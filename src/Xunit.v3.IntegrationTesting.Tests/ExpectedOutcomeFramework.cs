using Xunit.Sdk;
using Xunit.v3;
using Xunit.v3.IntegrationTesting;

namespace Xunit.v3.IntegrationTesting.Tests;

/// <summary>
/// Test-only framework that extends <see cref="DependencyAwareFramework"/>
/// with <see cref="ExpectedToFailAttribute"/> / <see cref="ExpectedToBeSkippedAttribute"/>
/// support via a message sink wrapper.
/// </summary>
internal class ExpectedOutcomeFramework : DependencyAwareFramework
{
    public ExpectedOutcomeFramework() : base() { }
    public ExpectedOutcomeFramework(string? configFile) : base(configFile) { }

    protected override DependencyAwareFrameworkExecutor CreateExecutor(IXunitTestAssembly testAssembly)
    {
        return new ExpectedOutcomeFrameworkExecutor(testAssembly);
    }
}

internal class ExpectedOutcomeFrameworkExecutor(IXunitTestAssembly testAssembly)
    : DependencyAwareFrameworkExecutor(testAssembly)
{
    protected override IMessageSink WrapMessageSink(IMessageSink messageSink, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        return new ExpectedOutcomeMessageSinkWrapper(messageSink, testCases);
    }
}
