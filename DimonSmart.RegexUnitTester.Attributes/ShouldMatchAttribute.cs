using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ShouldMatchAttribute : Attribute, IRegexUnitTester
{
    public ShouldMatchAttribute(string testData, RegexOptions regexOptions = RegexOptions.None,
        [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "")
    {
        TestData = testData;
        RegexOptions = regexOptions;
        LineNumber = lineNumber;
        FileName = fileName;
    }

    public string TestData { get; }
    public RegexOptions RegexOptions { get; }
    public int LineNumber { get; }
    public string FileName { get; }
}