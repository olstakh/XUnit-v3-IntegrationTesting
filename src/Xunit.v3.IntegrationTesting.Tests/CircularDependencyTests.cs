namespace Xunit.v3.IntegrationTesting.Tests;

[TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
public class IntegrationTestsCircularDependency
{
    [FactDependsOn(Dependencies = [nameof(Test_B)])]
    [ExpectedToBeSkipped(Reason = "This test is expected to be skipped due to circular dependency with Test_B")]
    public void Test_A()
    {
        // This test depends on Test_B, creating a circular dependency
        Assert.True(true);
    }

    [FactDependsOn(Dependencies = [nameof(Test_A)])]
    [ExpectedToBeSkipped(Reason = "This test is expected to be skipped due to circular dependency with Test_A")]
    public void Test_B()
    {
        // This test depends on Test_A, creating a circular dependency
        Assert.True(true);
    }
}
