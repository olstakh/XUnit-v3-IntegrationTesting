using Xunit;
using Xunit.v3;
using Xunit.v3.IntegrationTesting;
using Fact = Xunit.v3.IntegrationTesting.FactDependsOnAttribute;

[assembly: TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
[assembly: TestFramework(typeof(DependencyAwareFramework))]

namespace Xunit.v3.IntegrationTesting.Manual;

[Trait("Category", "LocalOnly")] // Some of these tests are expected to fail locally, don't run in CI pipeline
public class IntegrationTests
{
    [@Fact]
    public void Test_DatabaseSetup()
    {
        // Setup database
        Assert.False(true);
    }

    [@Fact(Dependencies = [nameof(Test_DatabaseSetup)])]
    public void Test_CreateUser()
    {
        // Create user - depends on database setup
        Assert.True(true);
    }

    [@Fact]
    public void Test_UserLogin()
    {
        // Test user login - depends on user creation
        Assert.True(true);
    }

    [@Fact(Dependencies = [nameof(Test_UserLogin), nameof(Test_CreateUser)])]
    public void Test_UserProfile()
    {
        // Test user profile - depends on both user creation and login
        Assert.True(true);
    }
}