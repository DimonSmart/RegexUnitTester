using Microsoft.VisualStudio.TestPlatform.ObjectModel;



public static class TestPropertyItems
{
    public static readonly TestProperty RegexPattern = TestProperty.Register(
        "RegexTestExecutor.RegexPattern", "Regex Pattern", typeof(string), typeof(TestPropertyItems));
    public static readonly TestProperty ExpectedMatch = TestProperty.Register(
        "RegexTestExecutor.ExpectedMatch", "Expected Match", typeof(string), typeof(TestPropertyItems));
    public static readonly TestProperty MustNotMatch = TestProperty.Register(
        "RegexTestExecutor.MustNotMatch", "Must Not Match", typeof(string), typeof(TestPropertyItems));
    public static readonly TestProperty InfoOnly = TestProperty.Register(
        "RegexTestExecutor.InfoOnly", "Information Only", typeof(string), typeof(TestPropertyItems));
}