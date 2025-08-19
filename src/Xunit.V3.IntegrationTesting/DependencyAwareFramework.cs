using Xunit.v3;
using System.Reflection;

namespace Xunit.V3.IntegrationTesting;

public class DependencyAwareFramework : XunitTestFramework
{
    private readonly string? _configFile;
    private ITestFrameworkDiscoverer? _discoverer;

    public DependencyAwareFramework()
        : base()
    {
    }

    public DependencyAwareFramework(string? configFile)
        : base(configFile)
    {
        _configFile = configFile;
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
    {
        _discoverer = new DependencyAwareTestDiscoverer(base.CreateDiscoverer(assembly));
        return _discoverer;
    }

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        return new DependencyAwareFrameworkExecutor(new XunitTestAssembly(assembly, _configFile, assembly.GetName().Version), _discoverer);
    }
}