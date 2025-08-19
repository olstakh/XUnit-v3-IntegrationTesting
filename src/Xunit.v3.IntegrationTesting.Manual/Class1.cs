using Xunit;
using Xunit.v3;

namespace Xunit.V3.IntegrationTesting.Manual;

public class IntegrationTests
{
    public static bool Skip { get; } = true;

    [DependsOn]
    [BB]
    [BB2]
    public void Test_DatabaseSetup()
    {
        // Setup database
        Assert.False(true);
    }

    [BB]
    [BB2]
    [DependsOn(Dependencies = [nameof(Test_DatabaseSetup)], Skip = "aaa")]
    public void Test_CreateUser()
    {
        // Create user - depends on database setup
        Assert.True(true);
    }

    [BB]
    [BB2]
    [DependsOn(Dependencies = [nameof(Test_CreateUser)])]
    public void Test_UserLogin()
    {
        // Test user login - depends on user creation
        Assert.True(true);
    }

    [DependsOn(Dependencies = [nameof(Test_UserLogin), nameof(Test_CreateUser)])]
    [BB]
    [BB2]
    public void Test_UserProfile()
    {
        // Test user profile - depends on both user creation and login
        Assert.True(true);
    }
}