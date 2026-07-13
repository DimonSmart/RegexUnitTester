using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DimonSmart.RegexUnitTester.Attributes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using static DimonSmart.RegexUnitTester.TestAdapter.RegexUnitTestConstants;

namespace DimonSmart.RegexUnitTester.TestAdapter;

[ExtensionUri(RegexUnitTestExecutorUriString)]
[DefaultExecutorUri(RegexUnitTestExecutorUriString)]
[FileExtension(ExeFileExtension)]
[FileExtension(DllFileExtension)]
[Category("managed")]
public sealed class RegexUnitTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources)
        {
            try
            {
                var assembly = Assembly.LoadFrom(source);
                foreach (var type in GetLoadableTypes(assembly, logger))
                    DiscoverTestsInType(type, source, logger, discoverySink);
            }
            catch (Exception exception) when (exception is BadImageFormatException or FileLoadException or FileNotFoundException)
            {
                logger.SendMessage(TestMessageLevel.Warning, $"Skipping '{source}': {exception.Message}");
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly, IMessageLogger logger)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException exception)
        {
            foreach (var loaderException in exception.LoaderExceptions.Where(x => x is not null))
                logger.SendMessage(TestMessageLevel.Warning, $"Could not load a type from '{assembly.Location}': {loaderException!.Message}");
            return exception.Types.Where(x => x is not null).Cast<Type>();
        }
    }

    private static void DiscoverTestsInType(Type type, string assemblyPath, IMessageLogger logger, ITestCaseDiscoverySink sink)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        foreach (var field in type.GetFields(flags)) ProcessMember(field, type, assemblyPath, logger, sink);
        foreach (var property in type.GetProperties(flags)) ProcessMember(property, type, assemblyPath, logger, sink);
        foreach (var method in type.GetMethods(flags)) ProcessMember(method, type, assemblyPath, logger, sink);
    }

    private static void ProcessMember(MemberInfo member, Type type, string assemblyPath, IMessageLogger logger, ITestCaseDiscoverySink sink)
    {
        IReadOnlyList<IRegexUnitTester> attributes;
        try { attributes = member.GetCustomAttributes().OfType<IRegexUnitTester>().ToArray(); }
        catch (Exception exception)
        {
            logger.SendMessage(TestMessageLevel.Warning, $"Ignoring '{type.FullName}.{member.Name}': cannot read attributes ({exception.Message}).");
            return;
        }

        if (attributes.Count == 0) return;
        if (!TryGetSourceKind(member, out var sourceKind, out var reason))
        {
            logger.SendMessage(TestMessageLevel.Warning, $"Ignoring '{type.FullName}.{member.Name}': {reason}.");
            return;
        }

        for (var index = 0; index < attributes.Count; index++)
        {
            var attribute = attributes[index];
            var descriptor = new RegexTestDescriptor(
                assemblyPath, type.FullName ?? type.Name, member.Name, sourceKind, GetTestKind(attribute), attribute.TestData,
                (int)attribute.RegexOptions, index, attribute.FileName, attribute.LineNumber);
            var testCase = CreateTestCase(descriptor);
            if (sourceKind is RegexSourceKind.RegexField or RegexSourceKind.RegexProperty or RegexSourceKind.RegexMethod &&
                attribute.RegexOptions != RegexOptions.None)
            {
                logger.SendMessage(TestMessageLevel.Warning,
                    $"RegexOptions on '{type.FullName}.{member.Name}' are ignored because the Regex instance supplies its own options.");
            }
            sink.SendTestCase(testCase);
        }
    }

    private static bool TryGetSourceKind(MemberInfo member, out RegexSourceKind sourceKind, out string reason)
    {
        sourceKind = default;
        reason = string.Empty;
        if (!IsStatic(member)) { reason = "only static members are supported"; return false; }
        switch (member)
        {
            case FieldInfo field when field.FieldType == typeof(string): sourceKind = RegexSourceKind.StringField; return true;
            case FieldInfo field when typeof(Regex).IsAssignableFrom(field.FieldType): sourceKind = RegexSourceKind.RegexField; return true;
            case FieldInfo: reason = "field must be string or Regex"; return false;
            case PropertyInfo property when property.GetIndexParameters().Length != 0: reason = "indexer properties are not supported"; return false;
            case PropertyInfo property when property.GetMethod is null: reason = "property must have a getter"; return false;
            case PropertyInfo property when property.PropertyType == typeof(string): sourceKind = RegexSourceKind.StringProperty; return true;
            case PropertyInfo property when typeof(Regex).IsAssignableFrom(property.PropertyType): sourceKind = RegexSourceKind.RegexProperty; return true;
            case PropertyInfo: reason = "property must return string or Regex"; return false;
            case MethodInfo method when method.IsGenericMethod: reason = "generic methods are not supported"; return false;
            case MethodInfo method when method.GetParameters().Length != 0: reason = "methods with parameters are not supported"; return false;
            case MethodInfo method when typeof(Regex).IsAssignableFrom(method.ReturnType): sourceKind = RegexSourceKind.RegexMethod; return true;
            case MethodInfo: reason = "method must return Regex"; return false;
            default: reason = "unsupported member type"; return false;
        }
    }

    private static bool IsStatic(MemberInfo member) => member switch
    {
        FieldInfo field => field.IsStatic,
        PropertyInfo property => property.GetMethod?.IsStatic == true,
        MethodInfo method => method.IsStatic,
        _ => false
    };

    private static TestCase CreateTestCase(RegexTestDescriptor descriptor)
    {
        var identity = string.Join("|", descriptor.DeclaringTypeName, descriptor.MemberName, descriptor.SourceKind,
            descriptor.TestKind, descriptor.TestData, descriptor.RegexOptionsValue, descriptor.AttributeIndex);
        string hash;
        using (var sha256 = SHA256.Create())
            hash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(identity))).Replace("-", string.Empty).Substring(0, 12);
        var fqn = $"{descriptor.DeclaringTypeName}.{descriptor.MemberName}.{descriptor.SourceKind}.{descriptor.TestKind}[{descriptor.AttributeIndex}].{hash}";
        var testCase = new TestCase(fqn, RegexUnitTestExecutorUri, descriptor.AssemblyPath)
        {
            DisplayName = $"{DisplayKind(descriptor.TestKind)}: {descriptor.MemberName} (\"{Truncate(descriptor.TestData)}\")",
            CodeFilePath = descriptor.CodeFilePath,
            LineNumber = descriptor.LineNumber
        };
        testCase.SetPropertyValue(TestPropertyItems.TestKind, descriptor.TestKind.ToString());
        testCase.SetPropertyValue(TestPropertyItems.TestData, descriptor.TestData);
        testCase.SetPropertyValue(TestPropertyItems.RegexSourceKind, descriptor.SourceKind.ToString());
        testCase.SetPropertyValue(TestPropertyItems.DeclaringTypeName, descriptor.DeclaringTypeName);
        testCase.SetPropertyValue(TestPropertyItems.MemberName, descriptor.MemberName);
        testCase.SetPropertyValue(TestPropertyItems.RegexOptionsValue, descriptor.RegexOptionsValue);
        testCase.SetPropertyValue(TestPropertyItems.AttributeIndex, descriptor.AttributeIndex);
        return testCase;
    }

    private static RegexTestKind GetTestKind(IRegexUnitTester attribute) => attribute switch
    {
        ShouldMatchAttribute => RegexTestKind.ShouldMatch,
        ShouldNotMatchAttribute => RegexTestKind.ShouldNotMatch,
        InfoMatchAttribute => RegexTestKind.Info,
        _ => throw new InvalidOperationException($"Unsupported test attribute '{attribute.GetType().FullName}'.")
    };

    private static string DisplayKind(RegexTestKind kind) => kind switch
    {
        RegexTestKind.ShouldMatch => "Should match",
        RegexTestKind.ShouldNotMatch => "Should not match",
        _ => "Info"
    };

    private static string Truncate(string value) => value.Length <= 80 ? value : value.Substring(0, 77) + "...";
}
