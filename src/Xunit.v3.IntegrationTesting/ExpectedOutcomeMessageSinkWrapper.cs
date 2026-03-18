using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.v3.IntegrationTesting;

/// <summary>
/// Wraps an <see cref="IMessageSink"/> and converts test results for methods
/// decorated with <see cref="ExpectedToFailAttribute"/> or
/// <see cref="ExpectedToBeSkippedAttribute"/>.
/// </summary>
internal sealed class ExpectedOutcomeMessageSinkWrapper : IMessageSink
{
    private readonly IMessageSink _inner;
    private readonly ConcurrentDictionary<string, IXunitTestCase> _testCaseLookup;

    public ExpectedOutcomeMessageSinkWrapper(IMessageSink inner, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        _inner = inner;
        _testCaseLookup = new ConcurrentDictionary<string, IXunitTestCase>(
            testCases.Select(tc => new KeyValuePair<string, IXunitTestCase>(tc.UniqueID, tc)));
    }

    public bool OnMessage(IMessageSinkMessage message)
    {
        switch (message)
        {
            // Expected-to-fail test actually failed → report as passed
            case TestFailed failed when HasAttribute<ExpectedToFailAttribute>(failed.TestCaseUniqueID):
                return _inner.OnMessage(new TestPassed
                {
                    AssemblyUniqueID = failed.AssemblyUniqueID,
                    ExecutionTime = failed.ExecutionTime,
                    FinishTime = failed.FinishTime,
                    Output = failed.Output,
                    TestCaseUniqueID = failed.TestCaseUniqueID,
                    TestClassUniqueID = failed.TestClassUniqueID,
                    TestCollectionUniqueID = failed.TestCollectionUniqueID,
                    TestMethodUniqueID = failed.TestMethodUniqueID,
                    TestUniqueID = failed.TestUniqueID,
                    Warnings = failed.Warnings,
                });

            // Expected-to-fail test unexpectedly passed → report as failed
            case TestPassed passed when HasAttribute<ExpectedToFailAttribute>(passed.TestCaseUniqueID):
                return _inner.OnMessage(new TestFailed
                {
                    AssemblyUniqueID = passed.AssemblyUniqueID,
                    Cause = FailureCause.Assertion,
                    ExceptionParentIndices = [-1],
                    ExceptionTypes = ["Xunit.v3.IntegrationTesting.UnexpectedTestPassException"],
                    ExecutionTime = passed.ExecutionTime,
                    FinishTime = passed.FinishTime,
                    Messages = ["Test was expected to fail but passed. Remove [ExpectedToFail] or fix the test."],
                    Output = passed.Output,
                    StackTraces = [""],
                    TestCaseUniqueID = passed.TestCaseUniqueID,
                    TestClassUniqueID = passed.TestClassUniqueID,
                    TestCollectionUniqueID = passed.TestCollectionUniqueID,
                    TestMethodUniqueID = passed.TestMethodUniqueID,
                    TestUniqueID = passed.TestUniqueID,
                    Warnings = passed.Warnings,
                });

            // Expected-to-be-skipped test actually skipped → report as passed
            case TestSkipped skipped when HasAttribute<ExpectedToBeSkippedAttribute>(skipped.TestCaseUniqueID):
                return _inner.OnMessage(new TestPassed
                {
                    AssemblyUniqueID = skipped.AssemblyUniqueID,
                    ExecutionTime = skipped.ExecutionTime,
                    FinishTime = skipped.FinishTime,
                    Output = skipped.Output,
                    TestCaseUniqueID = skipped.TestCaseUniqueID,
                    TestClassUniqueID = skipped.TestClassUniqueID,
                    TestCollectionUniqueID = skipped.TestCollectionUniqueID,
                    TestMethodUniqueID = skipped.TestMethodUniqueID,
                    TestUniqueID = skipped.TestUniqueID,
                    Warnings = skipped.Warnings,
                });

            default:
                return _inner.OnMessage(message);
        }
    }

    private bool HasAttribute<TAttribute>(string testCaseUniqueID) where TAttribute : Attribute
    {
        return _testCaseLookup.TryGetValue(testCaseUniqueID, out var testCase)
            && testCase.TestMethod.Method.GetCustomAttribute<TAttribute>(false) != null;
    }
}
