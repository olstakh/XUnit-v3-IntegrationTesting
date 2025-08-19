using Xunit;
using Xunit.V3.IntegrationTesting;

[assembly: TestCaseOrderer(typeof(DependencyTestCaseOrderer))]
[assembly: TestFramework(typeof(DependencyAwareFramework))]
