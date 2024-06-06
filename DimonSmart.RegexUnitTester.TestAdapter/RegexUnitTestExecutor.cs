using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.TestAdapter;

[ExtensionUri("executor://RegexUnitTestExecutor")]
public class RegexUnitTestExecutor : ITestExecutor
{
    private bool _isCancelled;

    void ITestExecutor.RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (tests == null) return;
        RunTestCases(tests, frameworkHandle);
    }

    private void ExecuteTest(TestCase testCase, IFrameworkHandle? frameworkHandle)
    {
        if (frameworkHandle == null) return;
        var regexPattern = testCase.GetPropertyValue<string>(TestPropertyItems.RegexPattern, string.Empty);

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{testCase.DisplayName}");
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Regex:{regexPattern}");

        Regex regex;
        try
        {
            regex = new Regex(regexPattern);
        }
        catch (RegexParseException exception)
        {
            RecordResult(false, exception.Message);
            return;
        }

        foreach (var groupName in regex.GetGroupNames())
        {
            if (regex.GroupNumberFromName(groupName) != 0)
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{groupName}: {regex.GroupNumberFromName(groupName)}");
        }

        frameworkHandle.RecordStart(testCase);

        // Informational test
        if (testCase.GetPropertyValue<string>(
            TestPropertyItems.InfoOnly, string.Empty) is { } infoMatchText && !string.IsNullOrEmpty(infoMatchText))
        {
            var matchResult = regex.Match(infoMatchText);
            var status = matchResult.Success ? "MATCHED" : "NOT_MATCHED";
            var description = GetDescription(regexPattern, infoMatchText, matchResult, status);
            RecordResult(true, description);
        }

        // Should match test
        if (testCase.GetPropertyValue<string>(
            TestPropertyItems.ExpectedMatch, string.Empty) is { } expectedMatchText && !string.IsNullOrEmpty(expectedMatchText))
        {
            var matchResult = regex.Match(expectedMatchText);
            var status = GetStatusText(matchResult.Success);
            var description = GetDescription(regexPattern, expectedMatchText, matchResult, status);
            RecordResult(matchResult.Success, description);
        }

        // Should not match test
        if (testCase.GetPropertyValue<string>(
            TestPropertyItems.MustNotMatch, string.Empty) is { } shouldNotMatchText && !string.IsNullOrEmpty(shouldNotMatchText))
        {
            var matchResult = regex.Match(shouldNotMatchText);
            var status = GetStatusText(!matchResult.Success);
            var description = GetDescription(regexPattern, shouldNotMatchText, matchResult, status);
            RecordResult(!matchResult.Success, description);
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

    private string GetDescription(string regexPattern, string expectedMatch, Match matchResult, string status)
    {
        var details = GetMatchDetails(matchResult);

        details = string.IsNullOrEmpty(details) ? string.Empty :
                        $"{Environment.NewLine}" +
                        $"{Environment.NewLine}" +
                        $"Details:" +
                        $"{Environment.NewLine}" +
                        details;

        return $"Status: {status}" +
                        $"{Environment.NewLine}" +
                        $"Pattern: '{regexPattern}'" +
                        $"{Environment.NewLine}" +
                        $"Text: '{expectedMatch}'" +
                        details;
    }

    private static string GetStatusText(bool success) => success ? "PASSED" : "FAILED";

    private string GetMatchDetails(Match match)
    {
        if (!match.Success) return string.Empty;

        var details = new StringBuilder();

        for (int i = 0; i < match.Groups.Count; i++)
        {
            var group = match.Groups[i];
            var groupName = string.IsNullOrEmpty(group.Name) ? i.ToString() : group.Name;
            var successStatus = group.Success ? "Ok" : "No";
            var line = $"Group {groupName}: '{group.Value}' (Position: {group.Index}, Length: {group.Length}, Status: {successStatus})";

            details.AppendLine(line);
        }

        return details.ToString().TrimEnd('\r', '\n');
    }

    public void Cancel()
    {
        _isCancelled = true;
    }

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (sources == null) return;

        var discoverySink = new TestCaseDiscoverySink();
        var discoverer = new RegexUnitTestDiscoverer();
        var logger = new MessageLogger();

        foreach (var source in sources)
        {
            discoverer.DiscoverTests(new[] { source }, NullDiscoveryContext.Instance, logger, discoverySink);
        }

        RunTestCases(discoverySink.TestCases, frameworkHandle);
    }


    private void RunTestCases(IEnumerable<TestCase> testCases, IFrameworkHandle? frameworkHandle)
    {
        if (frameworkHandle == null) return;
        if (testCases == null) return;

        foreach (var testCase in testCases)
        {
            if (_isCancelled) break;

            frameworkHandle?.RecordStart(testCase);
            try
            {
                ExecuteTest(testCase, frameworkHandle);
            }
            catch (Exception ex)
            {
                frameworkHandle?.RecordResult(new TestResult(testCase)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace
                });
            }
            frameworkHandle?.RecordEnd(testCase, TestOutcome.Passed);
        }
    }

    private class MessageLogger : IMessageLogger
    {
        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            Console.WriteLine($"{testMessageLevel}: {message}");
        }
    }

    private class TestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = new List<TestCase>();

        public void SendTestCase(TestCase testCase)
        {
            TestCases.Add(testCase);
        }
    }

}