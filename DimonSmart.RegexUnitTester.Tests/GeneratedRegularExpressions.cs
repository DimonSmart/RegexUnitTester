using DimonSmart.RegexUnitTester.Attributes;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.Tests;

public static partial class GeneratedRegularExpressions
{
    [GeneratedRegex("abc|def", RegexOptions.IgnoreCase, "en-US")]
    [ShouldNotMatch("123_12-1234")]
    [ShouldMatch("123-12-1234")]
    private static partial Regex AbcOrDefGeneratedRegex();
}

