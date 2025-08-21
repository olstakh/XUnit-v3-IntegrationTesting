using Xunit;
using Xunit.v3.IntegrationTesting;

[assembly: TestCaseOrderer(typeof(DependencyAwareTestCaseOrderer))]
[assembly: TestFramework(typeof(DependencyAwareFramework))]
