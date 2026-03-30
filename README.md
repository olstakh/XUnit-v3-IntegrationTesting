<WIP>

# XUnit-v3-IntegrationTesting
Extended framework for xunit.v3 that allows to establish dependencies between tests

[![NuGet Version](https://img.shields.io/nuget/vpre/xunit.v3.integrationtesting.svg)](https://www.nuget.org/packages/xunit.v3.integrationtesting)
[![GitHub license](https://img.shields.io/github/license/olstakh/XUnit-v3-IntegrationTesting.svg)](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/LICENSE)

This package provides the ability to establish dependencies between tests. Meaning the test should be executed only if all of its dependent tests have passed. The need for such dependencies typically arises in context of integration and end to end tests. Why run "Update entity" test if "Create entity" test has failed? Or ping test for deployed environment had failed - no need to run the others. This approach can save a lot of time in release pipelines.

# How to use

1. Add a package reference to [XUnit.v3.IntegrationTesting](https://www.nuget.org/packages/xunit.v3.integrationTesting) package in your xunit projects, or as a common package in the repo's [Directory.Packages.props](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/Directory.Packages.props)

## Test-level dependencies

2. Update `[Fact]` attributes in your tests to `[FactDependsOn]` (or `[Theory]` to `[TheoryDependsOn]`), if you want those tests to be dependent on other tests in the same class. Dependencies can be declared as

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

## Collection-level dependencies

3. Decorate collection definitions with `DependsOnCollections` attribute, if you want to order collections in dependent order. Note - `DisableParallelization` should be set to `true`, otherwise collections will be run in parallel and order won't matter.

If the test project doesn't have custom xunit framework extension (i.e. `[assembly: TestFramework(...)]` attribute), or assembly-level test collection orderer (i.e. `[assembly: TestCollectionOrderer(...)]`) - nothing else is needed.

### Which attribute to use for tests in collections with dependencies?

There are two approaches for handling tests in collections that have `[DependsOnCollections]`:

**Approach 1: Use `[FactDependsOn]` / `[TheoryDependsOn]`** (default)

All test methods in classes belonging to a collection with dependencies must use `[FactDependsOn]` / `[TheoryDependsOn]` instead of `[Fact]` / `[Theory]`. This is the default and is enforced by analyzer rule XIT0008. The skip logic for upstream collection failures is embedded in these attributes.

**Approach 2: Use `DependencySkippingFramework`** (opt-in)

If you prefer not to change your test attributes, you can enable `DependencySkippingFramework` — a custom test framework that handles collection-level skipping at the runner level. This allows plain `[Fact]` and `[Theory]` tests to be automatically skipped when their collection dependencies fail.

To opt in, set the following MSBuild property in your project file:

```xml
<PropertyGroup>
  <UseDependencySkippingFramework>true</UseDependencySkippingFramework>
</PropertyGroup>
```

When this framework is active, XIT0008 is suppressed.

> **Note:** Method-level dependencies (where one test depends on another within the same class) still require `[FactDependsOn]` / `[TheoryDependsOn]`, since the dependency information is declared on those attributes.

## Class-level dependencies (simplified)

4. Instead of manually creating collection definitions, you can use `[DependsOnClasses]` on test classes. A source generator will automatically create the corresponding collection definitions:

```csharp
[DependsOnClasses(Dependencies = [typeof(ClassB), typeof(ClassC)], Name = "DefinitionA")]
public class ClassA
{
    [FactDependsOn]
    public void Test1() { }
}
```

# How it works
Package includes custom `TestCaseOrderer`, which guarantees tests are run in the order of their dependencies. Test is executed only if all dependent tests have executed and succeeded. Otherwise test is [skipped dynamically](https://xunit.net/docs/getting-started/v3/whats-new#dynamically-skippable-tests). Same goes for `TestCollectionOrderer` that deals with collections.

# Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[FactDependsOn]` | Method | Replaces `[Fact]`. Supports `Dependencies` property to declare test-level dependencies. Contains skip logic that checks whether upstream dependencies passed before running the test. |
| `[TheoryDependsOn]` | Method | Replaces `[Theory]`. Same as `[FactDependsOn]` but for parameterized tests. |
| `[DependsOnCollections]` | Class (collection definition) | Declares that a collection depends on one or more other collections. Requires `DisableParallelization = true` on the `[CollectionDefinition]`. |
| `[DependsOnClasses]` | Class | Simplified syntax for class-level dependencies. A source generator creates the corresponding `[CollectionDefinition]` and `[DependsOnCollections]` automatically. |
| `[DependencyAwareBeforeAfterTest]` | Assembly | Auto-added by the package. Records test results in `KeyValueStorage` so dependency skip decisions can be made at runtime. |

# Frameworks

The package provides two custom test frameworks, both extending `XunitTestFramework`:

| Framework | Description |
|-----------|-------------|
| `DependencyAwareFramework` | Extends `XunitTestFramework` with dependency-aware test discovery. During filtered/partial test runs (e.g. selecting specific tests in Test Explorer or filtering via command line), it discovers the full set of tests so that transitive dependencies are included in the run. |
| `DependencySkippingFramework` | Extends `DependencyAwareFramework`. Adds automatic collection-level skipping at the runner level. Tests in collections whose upstream dependencies failed are skipped without requiring `[FactDependsOn]`. |

Both frameworks are extensible. If you have an existing custom `XunitTestFramework`, you can extend `DependencyAwareFramework` (or `DependencySkippingFramework`) instead:

```csharp
public class MyCustomFramework : DependencyAwareFramework
{
    protected override DependencyAwareFrameworkExecutor CreateExecutor(IXunitTestAssembly testAssembly)
        => new MyCustomExecutor(testAssembly);
}

public class MyCustomExecutor(IXunitTestAssembly testAssembly)
    : DependencyAwareFrameworkExecutor(testAssembly)
{
    protected override IMessageSink WrapMessageSink(IMessageSink sink, IReadOnlyCollection<IXunitTestCase> testCases)
        => new MyCustomSink(sink);
}
```

# Configuration

Following properties are available to be changed in `<PropertyGroup>` section of the project

| Name | Default value | Description |
|------|---------------|-------------|
| `AutoAddGlobalNamespace` | `true` | Adds `global using Xunit.v3.IntegrationTesting;` to the project |
| `UseDependencyAwareTestFramework` | `true` | Adds `[assembly: TestFramework(typeof(DependencyAwareFramework))]`. This adds support for filtered test runs. Ignored when `UseDependencySkippingFramework` is `true` |
| `UseDependencySkippingFramework` | `false` | Adds `[assembly: TestFramework(typeof(DependencySkippingFramework))]`. Enables automatic collection-level skipping for all test attributes including plain `[Fact]`/`[Theory]`. Takes precedence over `UseDependencyAwareTestFramework` |
| `UseDependencyAwareTestCaseOrderer` | `true` | Adds `[assembly: TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]`. This ensures test ordering based on provided dependencies |
| `UseDependencyAwareTestCollectionOrderer` | `true` | Adds `[assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]`. This ensures collection ordering based on provided dependencies |

## Notes
Custom test framework (i.e. `DependencyAwareFramework`) is used to support partial test runs (like selecting some tests in test explorer, or filtering tests in a command line). Because some of the selected/filtered tests might have dependencies that were not selected. This custom framework simply discovers all the tests, to make sure all dependencies are run, even if they were filtered out / not selected. This can be omitted, in which case such partial runs may be affected

Custom test case orderer (i.e. `DependencyAwareTestCaseOrderer`) is needed to ensure test case are ordered based on specified dependencies. If your test project already has assembly-level case orderer defined - you can add this attribute on a class level, which will take precedence. Otherwise test order will not be guaranteed to be dependency-aware, which will result in many skipped tests

Custom test collection orderer (i.e. `DependencyAwareTestCollectionOrderer`) is needed to ensure test collections are ordered based on specified dependencies. If your test project already has assembly-level collection orderer defined - collection order will not be guaranteed to be dependency-aware, which may result in skipped tests

# Rules

Following rules are included as part of the package:

| Id      | Default severity | Description |
|---------|------------------|-------------|
| XIT0001 | Warning | Class-level `TestCaseOrderer(...)` should be `DependencyAwareTestCaseOrderer` |
| XIT0002 | Warning | Assembly-level `TestCaseOrderer(...)` should be `DependencyAwareTestCaseOrderer` |
| XIT0003 | Warning | Project is missing class-level and assembly-level `TestCaseOrderer` attribute |
| XIT0004 | Warning | `FactDependsOn` has a dependency on a test method that doesn't exist |
| XIT0006 | Warning | Assembly is missing `TestFramework` attribute (should be `DependencyAwareFramework` or derived) |
| XIT0007 | Warning | Assembly has a `TestFramework` attribute that does not extend `DependencyAwareFramework` |
| XIT0008 | Warning | `[Fact]` or `[Theory]` should be replaced with `[FactDependsOn]` / `[TheoryDependsOn]` in classes belonging to collections with dependencies. Suppressed when `DependencySkippingFramework` is used |
| XIT0009 | Warning | Apply `DependsOnCollections` attribute only to collection definitions |
| XIT0010 | Warning | `CollectionDefinition` with `DependsOnCollections` must have `DisableParallelization` set to `true` |
| XIT0011 | Warning | `DependsOnCollections` attribute requires assembly-level `TestCollectionOrderer` |
| XIT0012 | Warning | `DependsOnCollections` attribute requires assembly-level `TestCollectionOrderer` to be `DependencyAwareTestCollectionOrderer` to respect test dependencies |

# Common questions

Q. Why do i need to use `FactDependsOn` instead of `Fact`?
A. `FactDependsOn` contains built-in skip logic that checks whether upstream dependencies (both test-level and collection-level) have passed before running the test. A regular `[Fact]` has no such logic, so it will run even when its dependencies have failed. You need `FactDependsOn` when declaring test-level dependencies via the `Dependencies` property, or when tests belong to collections with `[DependsOnCollections]` / `[DependsOnClasses]` attributes. If you'd rather not change your attributes, use `DependencySkippingFramework` which handles collection-level skipping at the runner level for all test attributes — though method-level dependencies still require `[FactDependsOn]`.

Q. I have a custom `SkipWhen` / `SkipUnless` / `SkipType` properties defined in `Fact` attribute - will they be respected when it's `FactDependsOn` attribute?
A. Yes. When decision is computed whether or not to skip the test - first the original `SkipWhen` / `SkipUnless` is executed, and if the result is that we should proceed - then dependency logic will be run. The original `Skip` message (defined by user) is appended with `or One or more dependencies were skipped or had failed.`

Q. If i have a test with dependencies and i run only this one test - dependencies will be executed as well?
A. Yes, that's the purpose of `TestFramework(typeof(DependencyAwareFramework))` - to load other needed tests. Unfortunately - they will not have visible results in VSTest UI (i.e. Test explorer) - but they will be executed and present in logs

Q. I already have a custom xUnit test framework. Can I still use this package?
A. Yes. Both `DependencyAwareFramework` and `DependencySkippingFramework` are designed to be extensible. Change your custom framework to extend one of them instead of `XunitTestFramework`. You can override `CreateExecutor(IXunitTestAssembly)` to provide your own executor subclass, and `WrapMessageSink` to intercept or filter test messages. The XIT0006/XIT0007 analyzers accept any type that derives from `DependencyAwareFramework`.

Q. What's the difference between `DependencyAwareFramework` and `DependencySkippingFramework`?
A. `DependencyAwareFramework` handles dependency-aware test discovery for filtered runs. Collection-level skipping requires tests to use `[FactDependsOn]` / `[TheoryDependsOn]`. `DependencySkippingFramework` extends it by also skipping tests at the runner level when their collection dependencies fail — this works for all test attributes including plain `[Fact]` / `[Theory]`.
