namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Marks a test that is expected to be skipped (e.g. because a dependency failed).
/// If the test is skipped, the result is reported as passed. If the test unexpectedly
/// runs and passes, the result is kept as-is.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ExpectedToBeSkippedAttribute : Attribute
{
    /// <summary>Optional reason why the skip is expected.</summary>
    public string? Reason { get; init; }
}
