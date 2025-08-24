; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
XIT0001 | Usage | Warning | Not supported class-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0002 | Usage | Warning | Missing test case orderer. Use DependencyAwareTestCaseOrderer
XIT0003 | Usage | Warning | Not supported assembly-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0004 | Usage | Warning | Missing test dependency
XIT0005 | Usage | Warning | Dependent tests should have DependsOn attribute
XIT0006 | Usage | Warning | Add assembly level [assembly: TestFramework(typeof(DependencyAwareFramework))] attribute to support partial test runs
XIT0007 | Usage | Warning | Use [assembly: TestFramework(typeof(DependencyAwareFramework))] attribute to support partial test runs