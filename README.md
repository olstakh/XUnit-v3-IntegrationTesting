<WIP>

# XUnit-v3-IntegrationTesting
Extended framework for xunit.v3 that allows to establish dependencies between tests

[![GitHub license](https://img.shields.io/github/license/olstakh/XUnit-v3-IntegrationTesting.svg)](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/LICENSE)

This package provides the ability to establish dependencies between tests. Meaning the test should be executed only if all of its dependent tests have passed. The need for such dependencies typically arises in context of integration and end to end tests. Why run "Update entity" test if "Create entity" test has failed? Or ping test for deployed environment had failed - no need to run the others. This approach can save a lot of time in release pipelines.

# How to use

1. Add a package reference to [XUnit.v3.IntegrationTesting](https://www.nuget.org/XUnit.v3.IntegrationTesting) package in your xunit projects, or as a common package in the repo's [Directory.Packages.props](https://github.com/olstakh/XUnit-v3-IntegrationTesting/blob/main/Directory.Build.props)

2. Update `[Fact]` attributes in your tests to `[FactDependsOn]`. This can be done by running string replacement from `[Fact(` to `[FactDependsOn(`, or by running the following command in the command line:

```
dotnet format
```
