﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;

namespace RegexUnitTester.Tests
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

            discoverer.DiscoverTests(new[] { Assembly.GetExecutingAssembly().Location }, null, mockLogger.Object, mockDiscoverySink.Object);

            Assert.IsTrue(testCases.Count > 0, "No test cases were discovered.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("ShouldMatch")), "ShouldMatch attribute was not processed correctly.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("ShouldNotMatch")), "ShouldNotMatch attribute was not processed correctly.");
            Assert.IsTrue(testCases.Any(tc => tc.DisplayName.Contains("InfoMatch")), "InfoMatch attribute was not processed correctly.");
        }
    }
}