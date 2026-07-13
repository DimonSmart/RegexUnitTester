using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace DimonSmart.RegexUnitTester.TestAdapter;

[ExtensionUri(RegexUnitTestConstants.RegexUnitTestExecutorUriString)]
public sealed class RegexUnitTestExecutor : ITestExecutor
{
    private volatile bool _isCancelled;

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (sources is null || frameworkHandle is null) return;
        var sink = new TestCaseDiscoverySink();
        new RegexUnitTestDiscoverer().DiscoverTests(sources, NullDiscoveryContext.Instance, new FrameworkLogger(frameworkHandle), sink);
        RunTestCases(sink.TestCases, frameworkHandle);
    }

    void ITestExecutor.RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (tests is not null && frameworkHandle is not null) RunTestCases(tests, frameworkHandle);
    }

    public void Cancel() => _isCancelled = true;

    private void RunTestCases(IEnumerable<TestCase> testCases, IFrameworkHandle frameworkHandle)
    {
        _isCancelled = false;
        var resolver = new RegexSourceResolver();
        foreach (var testCase in testCases)
        {
            if (_isCancelled) break;
            frameworkHandle.RecordStart(testCase);
            var result = ExecuteTest(testCase, resolver);
            frameworkHandle.RecordResult(result);
            frameworkHandle.RecordEnd(testCase, result.Outcome);
        }
    }

    private static TestResult ExecuteTest(TestCase testCase, RegexSourceResolver resolver)
    {
        try
        {
            var descriptor = ReadDescriptor(testCase);
            var regex = resolver.Resolve(descriptor);
            var match = regex.Match(descriptor.TestData);
            var passed = descriptor.TestKind switch
            {
                RegexTestKind.ShouldMatch => match.Success,
                RegexTestKind.ShouldNotMatch => !match.Success,
                RegexTestKind.Info => true,
                _ => false
            };
            var status = descriptor.TestKind == RegexTestKind.Info ? (match.Success ? "MATCHED" : "NOT_MATCHED") : passed ? "PASSED" : "FAILED";
            return CreateResult(testCase, passed ? TestOutcome.Passed : TestOutcome.Failed,
                Describe(regex, descriptor.TestData, match, status));
        }
        catch (Exception exception)
        {
            return CreateResult(testCase, TestOutcome.Failed, exception.Message, exception);
        }
    }

    private static RegexTestDescriptor ReadDescriptor(TestCase testCase)
    {
        var source = testCase.GetPropertyValue(TestPropertyItems.RegexSourceKind, string.Empty);
        var kind = testCase.GetPropertyValue(TestPropertyItems.TestKind, string.Empty);
        if (!Enum.TryParse(source, out RegexSourceKind sourceKind) || !Enum.TryParse(kind, out RegexTestKind testKind))
            throw new InvalidOperationException($"Test case '{testCase.FullyQualifiedName}' does not contain a valid regex descriptor.");
        return new RegexTestDescriptor(testCase.Source, testCase.GetPropertyValue(TestPropertyItems.DeclaringTypeName, string.Empty),
            testCase.GetPropertyValue(TestPropertyItems.MemberName, string.Empty), sourceKind, testKind,
            testCase.GetPropertyValue(TestPropertyItems.TestData, string.Empty), testCase.GetPropertyValue(TestPropertyItems.RegexOptionsValue, 0),
            testCase.GetPropertyValue(TestPropertyItems.AttributeIndex, 0), testCase.CodeFilePath, testCase.LineNumber);
    }

    private static TestResult CreateResult(TestCase testCase, TestOutcome outcome, string message, Exception? exception = null)
    {
        var result = new TestResult(testCase) { Outcome = outcome, DisplayName = testCase.DisplayName };
        result.Messages.Add(new TestResultMessage(outcome == TestOutcome.Passed ? TestResultMessage.StandardOutCategory : TestResultMessage.StandardErrorCategory, message));
        if (exception is not null) { result.ErrorMessage = exception.Message; result.ErrorStackTrace = exception.StackTrace; }
        return result;
    }

    private static string Describe(Regex regex, string text, Match match, string status)
    {
        var details = new StringBuilder();
        if (match.Success)
            for (var i = 0; i < match.Groups.Count; i++)
                details.AppendLine($"Group {regex.GroupNameFromNumber(i)}: '{match.Groups[i].Value}' (Position: {match.Groups[i].Index}, Length: {match.Groups[i].Length})");
        return $"Status: {status}{Environment.NewLine}Pattern: '{regex}'{Environment.NewLine}Text: '{text}'" +
               (details.Length == 0 ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}Details:{Environment.NewLine}{details}".TrimEnd());
    }

    private sealed class FrameworkLogger : IMessageLogger
    {
        private readonly IFrameworkHandle _handle;
        public FrameworkLogger(IFrameworkHandle handle) => _handle = handle;
        public void SendMessage(TestMessageLevel level, string message) => _handle.SendMessage(level, message);
    }

    private sealed class TestCaseDiscoverySink : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = new();
        public void SendTestCase(TestCase testCase) => TestCases.Add(testCase);
    }
}
