using Xunit;
using Xunit.v3;
using Xunit.v3.IntegrationTesting;

[assembly: TestFramework(typeof(DependencyAwareFramework))]
[assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]
[assembly: DependencyAwareBeforeAfterTest]
