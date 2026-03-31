using System.Reflection;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Test framework that augments xUnit v3's default pipeline with dependency-aware
/// test discovery. When a subset of tests is selected (e.g., via a filter or IDE),
/// this framework ensures all transitive upstream dependencies are included so that
/// dependency-based ordering and skipping work correctly.
/// <para>
/// Register with: <c>[assembly: TestFramework(typeof(DependencyAwareFramework))]</c>
/// </para>
/// </summary>
public class DependencyAwareFramework : XunitTestFramework
{
    /// <summary>
    /// Initializes a new instance with no configuration file.
    /// </summary>
    public DependencyAwareFramework()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified xUnit runner configuration file.
    /// </summary>
    /// <param name="configFile">Path to <c>xunit.runner.json</c>, or <c>null</c> for defaults.</param>
    public DependencyAwareFramework(string? configFile)
        : base(configFile)
    {
        ConfigFile = configFile;
    }

    /// <summary>
    /// The configuration file path passed to the framework constructor.
    /// </summary>
    protected string? ConfigFile { get; }

    /// <inheritdoc />
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