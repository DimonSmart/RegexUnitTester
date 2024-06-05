namespace DimonSmart.RegexUnitTester.TestAdapter;

public static class RegexUnitTestConstants
{
    public const string DllFileExtension = ".dll";
    public const string ExeFileExtension = ".exe";

    public const string RegexUnitTestExecutorUriString = @"executor://RegexUnitTestExecutor";
    public static Uri RegexUnitTestExecutorUri = new Uri(RegexUnitTestExecutorUriString);
}
