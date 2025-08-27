using Xunit;
using Xunit.v3;
using Xunit.v3.IntegrationTesting;

namespace Xunit.v3.IntegrationTesting.Manual;

[TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
[Trait("Category", "LocalOnly")] // Some of these tests are expected to fail locally, don't run in CI pipeline
public class IntegrationTestsCircularDependency
{
    [FactDependsOn(Dependencies = [nameof(Test_B)])]
    public void Test_A()
    {
        // This test depends on Test_B, creating a circular dependency
        Assert.True(true);
    }

    [FactDependsOn(Dependencies = [nameof(Test_A)])]
    public void Test_B()
    {
        // This test depends on Test_A, creating a circular dependency
        Assert.True(true);
    }
}