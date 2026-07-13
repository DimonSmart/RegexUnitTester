using System.Reflection;
using DimonSmart.RegexUnitTester.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VstestTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace DimonSmart.RegexUnitTester.Tests;

[TestClass]
public class DiscovererTests
{
    [TestMethod]
    public void Discoverer_creates_a_descriptor_for_each_supported_attribute()
    {
        var testCases = Discover();

#if NET9_0_OR_GREATER
        Assert.HasCount(32, testCases);
#else
        Assert.HasCount(30, testCases);
#endif
        Assert.AreEqual(testCases.Count, testCases.Select(x => x.FullyQualifiedName).Distinct().Count());
        CollectionAssert.IsSubsetOf(
            new[] { "StringField", "StringProperty", "RegexField", "RegexProperty", "RegexMethod" },
            testCases.Select(x => x.GetPropertyValue(TestPropertyItems.RegexSourceKind, string.Empty)).Distinct().ToArray());
        Assert.IsTrue(testCases.Any(x => x.GetPropertyValue(TestPropertyItems.TestData, "not empty") == string.Empty));
        Assert.IsTrue(testCases.Any(x => x.GetPropertyValue(TestPropertyItems.RegexOptionsValue, 0) != 0));
    }

    [TestMethod]
    public void Executor_records_one_complete_lifecycle_for_each_selected_test()
    {
        var selected = Discover().Where(x =>
            x.GetPropertyValue(TestPropertyItems.MemberName, string.Empty) is "CompiledRegex" or "RegexProperty" or "RegexMethod" or "AbcOrDefGeneratedRegex" or "EmptyStringRegex").ToArray();
        var starts = new List<TestCase>();
        var results = new List<VstestTestResult>();
        var ends = new List<(TestCase Test, TestOutcome Outcome)>();
        var handle = new Mock<IFrameworkHandle>();
        handle.Setup(x => x.RecordStart(It.IsAny<TestCase>())).Callback<TestCase>(starts.Add);
        handle.Setup(x => x.RecordResult(It.IsAny<VstestTestResult>())).Callback<VstestTestResult>(results.Add);
        handle.Setup(x => x.RecordEnd(It.IsAny<TestCase>(), It.IsAny<TestOutcome>()))
            .Callback<TestCase, TestOutcome>((test, outcome) => ends.Add((test, outcome)));

        ((ITestExecutor)new RegexUnitTestExecutor()).RunTests(selected, null, handle.Object);

        Assert.HasCount(selected.Length, starts);
        Assert.HasCount(selected.Length, results);
        Assert.HasCount(selected.Length, ends);
        Assert.IsTrue(results.All(x => x.Outcome == TestOutcome.Passed));
        CollectionAssert.AreEquivalent(results.Select(x => x.Outcome).ToArray(), ends.Select(x => x.Outcome).ToArray());
    }

    private static List<TestCase> Discover()
    {
        var sink = new Mock<ITestCaseDiscoverySink>();
        var testCases = new List<TestCase>();
        sink.Setup(x => x.SendTestCase(It.IsAny<TestCase>())).Callback<TestCase>(testCases.Add);
        new RegexUnitTestDiscoverer().DiscoverTests(new[] { Assembly.GetExecutingAssembly().Location }, NullDiscoveryContext.Instance,
            Mock.Of<IMessageLogger>(), sink.Object);
        return testCases;
    }
}
