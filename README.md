<WIP>

# XUnit-v3-IntegrationTesting
Extended framework for xunit.v3 that allows to establish dependencies between tests

[![GitHub license](https://img.shields.io/github/license/olstakh/XUnit-v3-IntegrationTesting.svg)](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/LICENSE)

This package provides the ability to establish dependencies between tests. Meaning the test should be executed only if all of its dependent tests have passed. The need for such dependencies typically arises in context of integration and end to end tests. Why run "Update entity" test if "Create entity" test has failed? Or ping test for deployed environment had failed - no need to run the others. This approach can save a lot of time in release pipelines.

# How to use

1. Add a package reference to [XUnit.v3.IntegrationTesting](https://www.nuget.org/XUnit.v3.IntegrationTesting) package in your xunit projects, or as a common package in the repo's [Directory.Packages.props](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/Directory.Build.props)

2. Update `[Fact]` attributes in your tests to `[FactDependsOn]`. This can be done by running string replacement from `[Fact(` to `[FactDependsOn(`, or by running the following command in the command line:

```
dotnet format analyzers --diagnostics XIT0008 --severity info --verbosity detailed
```

This assumes namespace `Xunit.v3.IntegrationTesting` is opened (which is by default added to global usings with `<AutoAddGlobalNamespace>true</AutoAddGlobalNamespace>`). This can be disabled and namespace can be opened by other available means (or even using full name for `Xunit.v3.IntegrationTesting.FactDependsOn` attribute)

3. Update `FactDependsOn` attribute with dependencies for those tests that need it. For example
```csharp
[FactDependsOn(Dependencies = [nameof(Test2), nameof(Test3)])]
public void Test1() {}

[FactDependsOn]
public void Test2() {}

[FactDependsOn]
public void Test3() {}
```
Now `Test1` will be run after `Test2` and `Test3` and only if both have passed.


If the test project doesn't have custom xunit framework extension (i.e. `[assembly: TestFramework(...)]` attribute), or assembly-level test case orderer (i.e. `[assembly: TestCaseOrderer(...)]`) - nothing else is needed.

# How it works
Package includes custom `TestCaseOrderer`, which guarantees tests are run in the order of their dependencies. Test is executed only if all dependent tests have executed and succeeded. Otherwise test is [skipped dynamically](https://xunit.net/docs/getting-started/v3/whats-new#dynamically-skippable-tests)

# Configuration

Following properties are available to be changed in `<PropertyGroup>` section of the project

| Name | Default value | Description |
|------|---------------|-------------|
| AutoAddGlobalNamespace | `true` | Adds `global using Xunit.v3.IntegrationTesting;` to the project |
| UseDependencyAwareTestFramework | `true` | Adds `[assembly: Xunit.TestFramework(Xunit.v3.IntegrationTesting.DependencyAwareFramework)]`. This adds support for filtered test runs |
| UseDependencyAwareTestCaseOrderer | `true` | Adds `[assembly: Xunit.TestCaseOrderer(typeof(Xunit.v3.IntegrationTesting.DependencyAwareTestCaseOrderer))]`. This ensures test ordering based on provided dependencies |

## Notes
Custom test framework (i.e. `DependencyAwareFramework`) is used to support partial test runs (like selecting some tests in test explorer, or filtering tests in a command line). Because some of the selected/filtered tests might have dependencies that were not selected. This custom framework simply discovers all the tests, to make sure all dependencies are run, even if they were filtered out / not selected. This can be omitted, in which case such partial runs may be affected

Custom test case orderer (i.e. `DependencyAwareTestCaseOrderer`) is needed to ensure test case are ordered based on specified dependencies. If your test project already has assembly-level case orderer defined - you can add this attribute on a class level, which will take precedence. Otherwise test order will not be guaranteed to be dependency-aware, which will result in many skipped tests

# Rules

Following rules are included as part of the package:

| Id      | Default severity | Description |
|---------|------------------|-------------|
| XIT0001 | Warning | Class-level `TestCaseOrderer(...)` should be `DependencyAwareTestCaseOrderer` |
| XIT0002 | Warning | Assembly-level `TestCaseOrderer(...)` should be `DependencyAwareTestCaseOrderer` |
| XIT0003 | Warning | Project is missing class-level and assembly-level `TestCaseOrderer` attribute |
| XIT0004 | Warning | `FactDependsOn` has a dependency on a test method that doesn't exist |
| XIT0005 | Warning | `FactDependsOn` has a dependency on a test method, not decorated with `FactDependsOn` attribute |
| XIT0006 | Warning | Assembly-level `TestFramework(...)` should be `DependencyAwareFramework` |
| XIT0007 | Warning | Project is missing assembly-level `TestFramework` attribute |
| XIT0008 | Info | `Fact` attribute should be replaced with `FactDependsOn` |

# Common questions

Q. Why do i need to change `Fact` to `FactDependsOn` for all the tests, and not just for those that have dependencies?
A. `FactDependsOn` implements `IBeforeAfterTestAttribute` interface, making not of a current test result, in case someone depends on it. Otherwise if a test with `FactDependsOn` attribute depends on a test with `Fact` attribute - it will always be skipped

Q. I have a custom `SkipWhen` / `SkipUnless` / `SkipType` properties defined in `Fact` attribute - will they be respected when it's `FactDependsOn` attribute?
A. Yes. When decision is computed whether or not to skip the test - first the original `SkipWhen` / `SkipUnless` is executed, and it the result is that we should proceed - then dependency logic will be run. The original `Skip` message (defined by user) is appended with `or One or more dependencies were skipped or had failed.`

Q. If i have a test with dependencies and i run only this one test - dependencies will be executed as well?
A. Yes, that's the purpose of `TestFramework(typeof(DependencyAwareFramework))` - to load other needed tests. Unfortunately - they will not have visible results in VSTest UI (i.e. Test explorer) - but they will be executed and present in logs
