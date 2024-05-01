using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;


[ExtensionUri("executor://RegexUnitTestExecutor")]
[DefaultExecutorUri("executor://RegexUnitTestExecutor")]
[FileExtension(".exe")]
[FileExtension(".dll")]
public class RegexUnitTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (string source in sources)
        {
            var assembly = Assembly.LoadFrom(source);
            foreach (Type type in assembly.GetTypes())
            {
                DiscoverTestsInType(type, logger, discoverySink);
            }
        }
    }

    private void DiscoverTestsInType(Type type, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        // Discover fields with specific attributes
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // Check if the field has any of the desired attributes
            var hasRelevantAttribute = field.GetCustomAttributes()
                .Any(attr => attr is ShouldMatchAttribute || attr is ShouldNotMatchAttribute || attr is InfoMatchAttribute);

            if (hasRelevantAttribute && field.FieldType == typeof(string))
            {
                var fieldValue = field.GetValue(null);
                ProcessMember(field, fieldValue?.ToString(), type, logger, discoverySink);
            }
        }

        // Discover properties with specific attributes
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            // Check if the property has any of the desired attributes
            var hasRelevantAttribute = property.GetCustomAttributes()
                .Any(attr => attr is ShouldMatchAttribute || attr is ShouldNotMatchAttribute || attr is InfoMatchAttribute);

            if (hasRelevantAttribute && property.PropertyType == typeof(string))
            {
                var propertyValue = property.GetValue(null); // Access static property without instance
                ProcessMember(property, propertyValue?.ToString(), type, logger, discoverySink);
            }
        }
    }

    private static void ProcessMember(MemberInfo member, string value, Type declaringType, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        var attributes = member.GetCustomAttributes<Attribute>().Where(attr => attr is ShouldMatchAttribute || attr is ShouldNotMatchAttribute || attr is InfoMatchAttribute);
        foreach (var attribute in attributes)
        {
            var testCase = new TestCase($"{declaringType.FullName}.{member.Name}", new Uri("executor://RegexUnitTestExecutor"), member.Module.Assembly.Location)
            {
                DisplayName = $"{declaringType.Name}.{member.Name}: {attribute.GetType().Name}"
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

}