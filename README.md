# XUnit-v3-IntegrationTesting
Extended framework for xunit.v3 that allows to establish dependencies between tests

Issues:
1. Analyzer to alert on a test having dependency to another test not decorated with DependsOn attribute
2. Current AttributeUsageAnalyzer will alert if none of the assembly / class level attribute is ours. Need other diagnostic for having custom orderer, etc 