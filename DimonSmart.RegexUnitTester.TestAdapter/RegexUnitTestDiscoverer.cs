using System.ComponentModel;
using System.Reflection;
using DimonSmart.RegexUnitTester.Attributes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace DimonSmart.RegexUnitTester.TestAdapter;

[ExtensionUri("executor://RegexUnitTestExecutor")]
[DefaultExecutorUri("executor://RegexUnitTestExecutor")]
[FileExtension(".exe")]
[FileExtension(".dll")]
[Category("managed")]
public class RegexUnitTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources)
        {
            var assembly = Assembly.LoadFrom(source);
            foreach (var type in assembly.GetTypes())
            {
                DiscoverTestsInType(type, logger, discoverySink);
            }
        }
    }

    private void DiscoverTestsInType(Type type, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
        {
            var hasRelevantAttribute = field.GetCustomAttributes()
                .Any(attr => attr is ShouldMatchAttribute or ShouldNotMatchAttribute or InfoMatchAttribute);

            if (hasRelevantAttribute && field.FieldType == typeof(string))
            {
                var fieldValue = field.GetValue(null);
                ProcessMember(field, fieldValue?.ToString(), type, logger, discoverySink);
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
        {
            var hasRelevantAttribute = property.GetCustomAttributes()
                .Any(attr => attr is ShouldMatchAttribute or ShouldNotMatchAttribute or InfoMatchAttribute);

            if (hasRelevantAttribute && property.PropertyType == typeof(string))
            {
                var propertyValue = property.GetValue(null);
                ProcessMember(property, propertyValue?.ToString(), type, logger, discoverySink);
            }
        }
    }

    private static void ProcessMember(MemberInfo member, string value, Type declaringType, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        var attributes = member.GetCustomAttributes<Attribute>().OfType<IRegexUnitTester>();
        foreach (var attribute in attributes)
        {
            var testData = attribute.TestData;

            var fullyQualifiedName = $"{declaringType.FullName}.{member.Name}{testData}";
            var testCase = new TestCase(fullyQualifiedName, new Uri("executor://RegexUnitTestExecutor"), member.Module.Assembly.Location)
            {
                DisplayName = $"{GetCaseKind(attribute.GetType())}: {member.Name} ({testData})",
                CodeFilePath = attribute.FileName,
                LineNumber = attribute.LineNumber
            };

            testCase.SetPropertyValue(TestPropertyItems.RegexPattern, value);

            switch (attribute)
            {
                case ShouldMatchAttribute shouldMatchAttribute:
                    testCase.SetPropertyValue(TestPropertyItems.ExpectedMatch, shouldMatchAttribute.TestData);
                    break;

                case ShouldNotMatchAttribute shouldNotMatchAttribute:
                    testCase.SetPropertyValue(TestPropertyItems.MustNotMatch, shouldNotMatchAttribute.TestData);
                    break;

                case InfoMatchAttribute infoMatchAttribute:
                    testCase.SetPropertyValue(TestPropertyItems.InfoOnly, infoMatchAttribute.TestData);
                    break;
            }

            logger.SendMessage(TestMessageLevel.Informational, $"Discovered test: {testCase.DisplayName}");
            discoverySink.SendTestCase(testCase);
        }
    }

    private static readonly Dictionary<Type, string> AttributeToNameMap = new()
    {
        { typeof(ShouldMatchAttribute), "Should" },
        { typeof(ShouldNotMatchAttribute), "Should not" },
        { typeof(InfoMatchAttribute), "Info" }
    };

    private static string GetCaseKind(Type attributeType) => AttributeToNameMap[attributeType];
}