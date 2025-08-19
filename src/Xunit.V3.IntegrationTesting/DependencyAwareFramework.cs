using Xunit.v3;
using System.Reflection;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareFramework : XunitTestFramework
{
    private ITestFrameworkDiscoverer? _discoverer;

    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
    {
        _discoverer = new DependencyAwareTestDiscoverer(base.CreateDiscoverer(assembly));
        return _discoverer;
    }

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        return new DependencyAwareFrameworkExecutor(new XunitTestAssembly(assembly, null, assembly.GetName().Version), _discoverer);
    }
}