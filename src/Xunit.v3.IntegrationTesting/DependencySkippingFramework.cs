namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// A test framework that extends <see cref="DependencyAwareFramework"/> with automatic
/// dependency-aware skipping at the collection level for all test attributes.
/// <para>
/// When using this framework, tests in collections whose upstream collection dependencies
/// have failed or not run will be automatically skipped — without requiring [FactDependsOn]
/// or [TheoryDependsOn] attributes. Plain [Fact] and [Theory] tests work as-is.
/// </para>
/// <para>
/// Method-level dependencies (where one test depends on another within the same class)
/// still require [FactDependsOn] / [TheoryDependsOn] since the dependency information
/// is declared on those attributes.
/// </para>
/// </summary>
public class DependencySkippingFramework : DependencyAwareFramework
{
    /// <summary>
    /// Initializes a new instance with no configuration file.
    /// </summary>
    public DependencySkippingFramework() : base() { }

    /// <summary>
    /// Initializes a new instance with the specified xUnit runner configuration file.
    /// </summary>
    /// <param name="configFile">Path to <c>xunit.runner.json</c>, or <c>null</c> for defaults.</param>
    public DependencySkippingFramework(string? configFile) : base(configFile) { }

    /// <inheritdoc />
    protected override DependencyAwareFrameworkExecutor CreateExecutor(IXunitTestAssembly testAssembly)
    {
        return new DependencySkippingExecutor(testAssembly);
    }
}
