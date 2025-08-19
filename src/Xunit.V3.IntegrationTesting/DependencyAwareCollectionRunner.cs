using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareCollectionRunner : XunitTestCollectionRunnerBase<XunitTestCollectionRunnerContext, IXunitTestCollection, IXunitTestClass, IXunitTestCase>
{
    protected override ValueTask<RunSummary> RunTestClass(XunitTestCollectionRunnerContext ctxt, IXunitTestClass? testClass, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        throw new NotImplementedException();
    }
}