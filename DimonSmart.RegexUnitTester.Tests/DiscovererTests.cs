using System.Reflection;
using DimonSmart.RegexUnitTester.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DimonSmart.RegexUnitTester.Tests
{
    [TestClass]
    public class DiscovererTests
    {
        [TestMethod]
        public void Discoverer_Should_Identify_Tests_Correctly()
        {
            var discoverer = new RegexUnitTestDiscoverer();
            var mockLogger = new Mock<IMessageLogger>();
            var mockDiscoverySink = new Mock<ITestCaseDiscoverySink>();
            var testCases = new List<TestCase>();

            mockDiscoverySink.Setup(x => x.SendTestCase(It.IsAny<TestCase>()))
                .Callback<TestCase>(testCase => testCases.Add(testCase));

            discoverer.DiscoverTests(new[] { Assembly.GetExecutingAssembly().Location }, NullDiscoveryContext.Instance, mockLogger.Object, mockDiscoverySink.Object);

            Assert.IsTrue(testCases.Count > 10, "No test cases were discovered.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("Should")), "ShouldMatch attribute was not processed correctly.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("Should not")), "ShouldNotMatch attribute was not processed correctly.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("Info")), "InfoMatch attribute was not processed correctly.");
        }
    }
}