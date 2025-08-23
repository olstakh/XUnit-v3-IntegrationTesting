; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
XIT0001 | Usage | Warning | Not supported class-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0003 | Usage | Warning | Not supported assembly-level test case orderer. Use DependencyAwareTestCaseOrderer
XIT0002 | Usage | Warning | Missing test case orderer. Use DependencyAwareTestCaseOrderer