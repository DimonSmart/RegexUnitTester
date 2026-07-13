using DimonSmart.RegexUnitTester.Attributes;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.Tests;

public static partial class GeneratedRegularExpressions
{
    [GeneratedRegex("^(abc|def)$", RegexOptions.IgnoreCase, "en-US")]
    [ShouldNotMatch("xyz")]
    [ShouldMatch("ABC")]
    [ShouldMatch("def")]
    private static partial Regex AbcOrDefGeneratedRegex();

#if NET9_0_OR_GREATER
    [GeneratedRegex("^property$", RegexOptions.IgnoreCase)]
    [ShouldMatch("PROPERTY")]
    [ShouldNotMatch("other")]
    private static partial Regex GeneratedPropertyRegex { get; }
#endif
}

