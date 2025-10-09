# Usage
For test methods you wish to have dependencies for - change `[Fact]` attribute to `[FactDependsOn]` from `Xunit.v3.IntegrationTesting` namespace.
They have the same signature, the only addition is `Dependencies = [...]` property, which will define dependencies.

For example, let's say we have these 4 tests defined:

```csharp
    [Fact]
    public void Test_DatabaseSetup()
    {
    }

    [Fact]
    public void Test_CreateUser()
    {
    }

    [Fact]
    public void Test_UserLogin()
    {
    }

    [Fact]
    public void Test_UserProfile()
    {
    }
```

Let's say we don't want to even run `Test_CreateUser` if database setup test had failed, the same way we want to skip running `UserLogin` and `UserProfile` when `CreateUser` failed. This can save a lot of time for end to end or other heavy test scenarios so we can fail early.

To achieve this - just declare corresponding dependencies:

```csharp
    [Fact]
    public void Test_DatabaseSetup()
    {
    }

    [FactDependsOn(Dependencies = [nameof(Test_DatabaseSetup)])]
    public void Test_CreateUser()
    {
    }

    [FactDependsOn(Dependencies = [nameof(Test_CreateUser)])]
    public void Test_UserLogin()
    {
    }

    [FactDependsOn(Dependencies = [nameof(Test_CreateUser) /*, other dependencies */])]
    public void Test_UserProfile()
    {
    }
```

A test will be executed only if all of its dependencies succeeded.

This is achieved by having the following assembly level attribute
```csharp
[assembly: TestCaseOrderer(typeof(Xunit.v3.IntegrationTesting.DependencyAwareTestCaseOrderer))]
```
It is added by default via `<UseDependencyAwareTestCaseOrderer>` msbuild property which is set to `true`.
If you have your own test case orderer - you can set it to `false` and apply this attribute to a class or collection definition level that has tests with dependencies. Without it - the order won't be guaranteed.
