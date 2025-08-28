namespace Xunit.v3.IntegrationTesting;

internal class CircularDependencyException<T> : Exception
{
    public CircularDependencyException() : base() { }
    public CircularDependencyException(string message) : base(message) { }
    public CircularDependencyException(string message, Exception innerException) : base(message, innerException) { }

    public IEnumerable<T> DependencyCycle { get; init; } = Array.Empty<T>();
}