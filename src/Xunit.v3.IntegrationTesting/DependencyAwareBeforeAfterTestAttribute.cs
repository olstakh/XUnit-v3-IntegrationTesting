using System.Reflection;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Records the outcome of each test into the shared key-value storage on <see cref="TestContext"/>
/// so that downstream dependency checks can determine whether upstream tests passed.
/// Applied automatically by <see cref="FactDependsOnAttribute"/> and <see cref="TheoryDependsOnAttribute"/>.
/// </summary>
public class DependencyAwareBeforeAfterTestAttribute : BeforeAfterTestAttribute
{
    /// <inheritdoc />
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        TestContext.Current.KeyValueStorage[ReadableTestId(test)] = TestContext.Current.TestState?.Result.ToString() ?? "Unknown result";

        static string ReadableTestId(IXunitTest test)
        {
            return $"{test.TestCase.TestCollection.TestCollectionDisplayName}.{test.TestCase.TestClassName}.{test.TestCase.TestMethodName}";
        }
    }

    /// <inheritdoc />
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
    }
}
