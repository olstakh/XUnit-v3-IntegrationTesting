using Xunit;
using Xunit.v3.IntegrationTesting;

[assembly: TestFramework(typeof(DependencyAwareFramework))]
[assembly: TestCollectionOrderer(typeof(DependencyAwareTestCollectionOrderer))]
