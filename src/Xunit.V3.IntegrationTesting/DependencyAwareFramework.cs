using Xunit.v3;
using System.Reflection;

namespace Xunit.v3.IntegrationTesting;

public class DependencyAwareFramework : XunitTestFramework
{
    private readonly string? _configFile;

    public DependencyAwareFramework()
        : base()
    {
    }

    public DependencyAwareFramework(string? configFile)
        : base(configFile)
    {
        _configFile = configFile;
    }

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        return new DependencyAwareFrameworkExecutor(new XunitTestAssembly(assembly, _configFile, assembly.GetName().Version), new DependencyAwareTestDiscoverer(base.CreateDiscoverer(assembly)));
    }
}