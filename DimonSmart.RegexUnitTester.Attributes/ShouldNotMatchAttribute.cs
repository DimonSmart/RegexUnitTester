using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static System.AttributeTargets;
namespace DimonSmart.RegexUnitTester.Attributes;

[AttributeUsage(Field | Property | Method, AllowMultiple = true)]
public class ShouldNotMatchAttribute : Attribute, IRegexUnitTester
{
    public ShouldNotMatchAttribute(string testData, RegexOptions regexOptions = RegexOptions.None,
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