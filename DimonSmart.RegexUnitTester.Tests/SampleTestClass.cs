// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using DimonSmart.RegexUnitTester.Attributes;

#pragma warning disable CS0414 // Field is assigned but its value is never used
namespace DimonSmart.RegexUnitTester.Tests;
public class SampleTestClass
{
    // Group: Public constants and static fields

    [ShouldMatch("abc"), ShouldNotMatch("xyz"), InfoMatch("abc"), InfoMatch("a-b-c")]
    public const string PublicConstABC = "abc";
    public const string PublicConstDEF_not_UnitTested = "def";

    [ShouldMatch("abc"), ShouldNotMatch("a-b-c"), InfoMatch("abc a-b-c")]
    public static string PublicStaticABC = @"abc";

    [ShouldMatch("abc"), ShouldNotMatch("xyz"), InfoMatch("abc")]
    private const string PrivateConstABC = "abc";

    [ShouldMatch("abc"), ShouldNotMatch("a-b-c"), InfoMatch("abc a-b-c")]
    private static string PrivateStaticABC = @"abc";

    [InfoMatch("123-12-1234")]
    private static string RegexWithError = @"(\d{3}-\d{2}-\d{4}";

    [ShouldNotMatch("123_12-1234")]
    [ShouldMatch("123-12-1234")]
    private static string SSN = @"(?<Area>\d{3})-(?<Group>\d{2})-(?<Serial>\d{4})(?<OptionalPart>-\d{2})?";
}
