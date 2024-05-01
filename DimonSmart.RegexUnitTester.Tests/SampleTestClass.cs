// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable CS0414 // Field is assigned but its value is never used
namespace RegexUnitTester.Tests;
public class SampleTestClass
{
    // Group: Public constants and static fields

    [ShouldMatch("abc"), ShouldNotMatch("xyz"), InfoMatch("a b c")]
    public const string PublicValidRegexWithUnitTest = "abc";
    public const string PublicValidRegexWithoutUnitTest = "def";

    [ShouldMatch("Static regex pattern"), ShouldNotMatch("Non-static pattern"), InfoMatch("pattern")]
    public static string PublicStaticValidRegexWithUnitTest = @"Static regex pattern";
    public static string PublicStaticValidRegexWithoutUnitTest = @"Static regex pattern";

    // Group: Private copies of the above constants and static fields for internal use

    [ShouldMatch("abc"), ShouldNotMatch("xyz"), InfoMatch("a-b-c")]
    private const string PrivateValidRegexWithUnitTest = "abc";
    private const string PrivateValidRegexWithoutUnitTest = "def";

    [ShouldMatch("Static regex pattern"), ShouldNotMatch("Non-static pattern"), InfoMatch("pattern")]
    private static string PrivateStaticValidRegexWithUnitTest = @"Static regex pattern";
    private static string PrivateStaticValidRegexWithoutUnitTest = @"Static regex pattern";
}



