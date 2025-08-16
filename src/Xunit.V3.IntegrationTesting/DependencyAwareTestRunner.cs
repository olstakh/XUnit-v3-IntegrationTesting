using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

public class DependencyAwareTestRunner : XunitTestClassRunner
{
    private static readonly HashSet<string> FailedTests = new HashSet<string>();
    private static readonly object LockObject = new object();

    protected override ValueTask<RunSummary> RunTestMethods(XunitTestClassRunnerContext ctxt, Exception? exception)
    {
        return base.RunTestMethods(ctxt, exception);
    }

    protected override ValueTask<RunSummary> RunTestMethod(XunitTestClassRunnerContext ctxt, IXunitTestMethod? testMethod, IReadOnlyCollection<IXunitTestCase> testCases, object?[] constructorArguments)
    {
        return base.RunTestMethod(ctxt, testMethod, testCases, constructorArguments);
    }    
}