using Xunit;
using Xunit.v3;
using Xunit.v3.IntegrationTesting;
using Xunit.v3.IntegrationTesting.Manual;

[assembly: TestFramework(typeof(ExpectedOutcomeFramework))]
[assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]
[assembly: DependencyAwareBeforeAfterTest]
