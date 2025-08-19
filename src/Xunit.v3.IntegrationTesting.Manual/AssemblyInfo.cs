using Xunit;
using Xunit.v3.IntegrationTesting;

[assembly: TestCaseOrderer(typeof(DependencyTestCaseOrderer))]
[assembly: TestFramework(typeof(DependencyAwareFramework))]
