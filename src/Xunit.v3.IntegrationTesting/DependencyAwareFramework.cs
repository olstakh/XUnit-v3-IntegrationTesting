using Xunit.v3;
using System.Reflection;

namespace Xunit.v3.IntegrationTesting;

public class DependencyAwareFramework : XunitTestFramework
{
    public DependencyAwareFramework()
        : this(null)
    {
    }

    public DependencyAwareFramework(string? configFile)
        : base(configFile)
    {
        ConfigFile = configFile;
    }

    /// <summary>
    /// The configuration file path passed to the framework constructor.
    /// </summary>
    protected string? ConfigFile { get; }

    protected sealed override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        return CreateExecutor(new XunitTestAssembly(assembly, ConfigFile, assembly.GetName().Version));
    }

    /// <summary>
    /// Creates the <see cref="DependencyAwareFrameworkExecutor"/> for the given test assembly.
    /// Override this to return a custom executor subclass while reusing the default <see cref="IXunitTestAssembly"/> construction.
    /// </summary>
    protected virtual DependencyAwareFrameworkExecutor CreateExecutor(IXunitTestAssembly testAssembly)
    {
        return new DependencyAwareFrameworkExecutor(testAssembly);
    }
}