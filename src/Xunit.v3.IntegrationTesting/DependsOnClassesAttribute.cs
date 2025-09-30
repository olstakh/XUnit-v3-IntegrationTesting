namespace Xunit.v3.IntegrationTesting;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DependsOnClassesAttribute(params Type[] dependentClasses) : Attribute
{
    /// <summary>
    /// Gets the list of dependent classes for the test.
    /// </summary>
    public Type[] Dependencies { get; } = dependentClasses ?? Array.Empty<Type>();
}