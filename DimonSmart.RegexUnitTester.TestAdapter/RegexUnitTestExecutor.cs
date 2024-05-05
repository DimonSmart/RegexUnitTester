﻿using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace DimonSmart.RegexUnitTester.TestAdapter;

[ExtensionUri("executor://RegexUnitTestExecutor")]
public class RegexUnitTestExecutor : ITestExecutor
{
    private bool _isCancelled;

    public void RunTests(IEnumerable<TestCase> testCases, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        foreach (var testCase in testCases)
        {
            if (_isCancelled) break;

            frameworkHandle.RecordStart(testCase);
            try
            {
                ExecuteTest(testCase, frameworkHandle);
            }
            catch (Exception ex)
            {
                frameworkHandle.RecordResult(new TestResult(testCase)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace
                });
            }
            frameworkHandle.RecordEnd(testCase, TestOutcome.Passed);
        }
    }

    private void ExecuteTest(TestCase testCase, IFrameworkHandle frameworkHandle)
    {
        var regexPattern = testCase.GetPropertyValue<string>(TestPropertyItems.RegexPattern, string.Empty);
        var regex = new Regex(regexPattern);
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{testCase.DisplayName}");
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Regex:{regexPattern}");

        foreach (var groupName in regex.GetGroupNames())
        {
            if (regex.GroupNumberFromName(groupName) != 0)
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{groupName}: {regex.GroupNumberFromName(groupName)}");
        }


        frameworkHandle.RecordStart(testCase);

        // Informational test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.InfoOnly, string.Empty) is { } infoOnly && !string.IsNullOrEmpty(infoOnly))
        {
            var infoMatch = regex.Match(infoOnly);
            RecordResult(true, $"InfoOnly test executed, pattern tested: '{infoOnly}', matched: {infoMatch.Success}", true);
        }

        // Expected match test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.ExpectedMatch, string.Empty) is { } expectedMatch && !string.IsNullOrEmpty(expectedMatch))
        {
            var matchResult = regex.Match(expectedMatch);
            RecordResult(matchResult.Success, $"Expected Match test, pattern: '{expectedMatch}', matched: {matchResult.Success}");
        }

        // Must not match test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.MustNotMatch, string.Empty) is { } mustNotMatch && !string.IsNullOrEmpty(mustNotMatch))
        {
            var matchResult = regex.Match(mustNotMatch);
            RecordResult(!matchResult.Success, $"MustNotMatch test, pattern: '{mustNotMatch}', should not match but did: {matchResult.Success}");
        }

        // General test in case no specific instructions provided
        if (string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.ExpectedMatch, string.Empty)) &&
            string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.MustNotMatch, string.Empty)) &&
            string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.InfoOnly, string.Empty)))
        {
            RecordResult(true, "No specific match requirements provided; regex tested without specific assertions.");
        }

        return;

        void RecordResult(bool isSuccess, string message, bool isInformational = false)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, message);

            frameworkHandle.RecordResult(new TestResult(testCase)
            {
                Outcome = isInformational ? TestOutcome.Passed : (isSuccess ? TestOutcome.Passed : TestOutcome.Failed),
                DisplayName = testCase.DisplayName,
                Messages = { new TestResultMessage(isSuccess ? TestResultMessage.StandardOutCategory : TestResultMessage.StandardErrorCategory, message) }
            });
        }
    }


    public void Cancel()
    {
        _isCancelled = true;
    }

    public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        // This method can be implemented if needed
    }
}