namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Used as a token to generate corresponding collection definition for test classes that have dependencies on other test classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DependsOnClassesAttribute : Attribute
{
    /// <summary>
    /// Gets the list of dependent classes for the test.
    /// </summary>
    public required Type[] Dependencies { get; init; }

    /// <summary>
    /// Gets or sets the name of the collection definition to generate for the test class.
    /// </summary>
    public required string CollectionName { get; init; }
}