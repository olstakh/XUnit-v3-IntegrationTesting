; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
XIT0001 | Usage | Warning | Not supported class-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0002 | Usage | Warning | Missing test case orderer or collection case orderer. Use DependencyAwareTestCaseOrderer or DependencyAwareTestCollectionOrderer
XIT0003 | Usage | Warning | Not supported assembly-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0004 | Usage | Warning | Missing test dependency
XIT0006 | Usage | Warning | Add assembly level [assembly: TestFramework(typeof(DependencyAwareFramework))] attribute to support partial test runs
XIT0007 | Usage | Warning | Use [assembly: TestFramework(typeof(DependencyAwareFramework))] attribute to support partial test runs
XIT0008 | Usage | Info | Use FactDependsOn attribute
XIT0009 | Usage | Warning | Apply DependsOnCollections attribute only to collection definitions
XIT0010 | Usage | Warning | CollectionDefinition with DependsOnCollections must have DisableParallelization set to true
XIT0011 | Usage | Warning | DependsOnCollections attribute requires assembly-level TestCollectionOrderer
XIT0012 | Usage | Warning | DependsOnCollections attribute requires assembly-level TestCollectionOrderer to be DependencyAwareTestCollectionOrderer to respect test dependencies