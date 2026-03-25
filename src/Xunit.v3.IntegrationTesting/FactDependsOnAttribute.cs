using System;
using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a fact that should be run
/// by the default test runner.
/// Allows to specify test dependencies that must run and succeed for current test not to be skipped.
/// </summary>
[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[IgnoreXunitAnalyzersRule1013]
public class FactDependsOnAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1
    ) : DependsOnAttributeBase(sourceFilePath, sourceLineNumber)
{
}

// see https://github.com/xunit/xunit/issues/3387#issuecomment-3750889641
internal sealed class IgnoreXunitAnalyzersRule1013Attribute : Attribute { }