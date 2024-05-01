using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.Attributes;

public interface IRegexUnitTester
{
    public int LineNumber { get; }
    public string FileName { get; }
    public string TestData { get; }
    public RegexOptions RegexOptions { get; }
}