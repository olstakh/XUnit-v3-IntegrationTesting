using System.Reflection;
using Xunit;
using Xunit.v3;

public class DependencyAwareBeforeAfterTestAttribute : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        TestContext.Current.KeyValueStorage[ReadableTestId(test)] = TestContext.Current.TestState?.Result.ToString() ?? "Unknown result";

        static string ReadableTestId(IXunitTest test)
        {
            return $"{test.TestCase.TestCollection.TestCollectionDisplayName}.{test.TestCase.TestClassName}.{test.TestCase.TestMethodName}";
        }
    }

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
    }
}
