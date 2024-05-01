using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;


[ExtensionUri("executor://RegexUnitTestExecutor")]
public class RegexUnitTestExecutor : ITestExecutor
{
    private bool isCancelled;

    public void RunTests(IEnumerable<TestCase> testCases, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        foreach (var testCase in testCases)
        {
            if (isCancelled)
                break;

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

        // Record the start of the test case
        frameworkHandle.RecordStart(testCase);

        // Helper method to record the test result
        void recordResult(bool isSuccess, string message, bool isInformational = false)
        {
            frameworkHandle.RecordResult(new TestResult(testCase)
            {
                Outcome = isInformational ? TestOutcome.Passed : (isSuccess ? TestOutcome.Passed : TestOutcome.Failed),
                DisplayName = testCase.DisplayName,
                Messages = { new TestResultMessage("Information", message) }
            });
        }

        // Informational test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.InfoOnly, string.Empty) is { } infoOnly && !string.IsNullOrEmpty(infoOnly))
        {
            var infoMatch = regex.Match(infoOnly);
            recordResult(true, $"InfoOnly test executed, pattern tested: '{infoOnly}', matched: {infoMatch.Success}", true);
        }

        // Expected match test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.ExpectedMatch, string.Empty) is { } expectedMatch && !string.IsNullOrEmpty(expectedMatch))
        {
            var matchResult = regex.Match(expectedMatch);
            recordResult(matchResult.Success, $"ExpectedMatch test, pattern: '{expectedMatch}', matched: {matchResult.Success}");
        }

        // Must not match test
        if (testCase.GetPropertyValue<string>(TestPropertyItems.MustNotMatch, string.Empty) is { } mustNotMatch && !string.IsNullOrEmpty(mustNotMatch))
        {
            var matchResult = regex.Match(mustNotMatch);
            recordResult(!matchResult.Success, $"MustNotMatch test, pattern: '{mustNotMatch}', should not match but did: {matchResult.Success}");
        }

        // General test in case no specific instructions provided
        if (string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.ExpectedMatch, string.Empty)) &&
            string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.MustNotMatch, string.Empty)) &&
            string.IsNullOrEmpty(testCase.GetPropertyValue<string>(TestPropertyItems.InfoOnly, string.Empty)))
        {
            recordResult(true, "No specific match requirements provided; regex tested without specific assertions.");
        }
    }


    public void Cancel()
    {
        isCancelled = true;
    }

    public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        // This method can be implemented if needed
    }
}