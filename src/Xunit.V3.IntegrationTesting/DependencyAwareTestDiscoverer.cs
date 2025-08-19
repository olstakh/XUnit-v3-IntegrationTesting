using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareTestDiscoverer : ITestFrameworkDiscoverer
{
    private readonly ITestFrameworkDiscoverer _innerDiscoverer;

    public DependencyAwareTestDiscoverer(ITestFrameworkDiscoverer innerDiscoverer)
    {
        this._innerDiscoverer = innerDiscoverer;
    }

    public ITestAssembly TestAssembly => _innerDiscoverer.TestAssembly;

    public ValueTask Find(Func<ITestCase, ValueTask<bool>> callback, ITestFrameworkDiscoveryOptions discoveryOptions, Type[]? types = null, CancellationToken? cancellationToken = null)
    {
        return _innerDiscoverer.Find((testCase) =>
        {
            return callback(testCase);
        }, discoveryOptions, types, cancellationToken);
    }
}