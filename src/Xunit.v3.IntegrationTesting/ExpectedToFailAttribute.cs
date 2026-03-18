namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Marks a test that is expected to fail. If the test fails, the result is
/// reported as passed. If the test unexpectedly passes, the result is reported
/// as failed so the attribute can be removed.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ExpectedToFailAttribute : Attribute
{
    /// <summary>Optional reason why the failure is expected.</summary>
    public string? Reason { get; init; }
}
