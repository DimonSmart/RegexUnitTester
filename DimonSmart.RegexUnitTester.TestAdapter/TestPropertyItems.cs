using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DimonSmart.RegexUnitTester.TestAdapter;

internal enum RegexSourceKind
{
    StringField,
    StringProperty,
    RegexField,
    RegexProperty,
    RegexMethod
}

internal enum RegexTestKind
{
    ShouldMatch,
    ShouldNotMatch,
    Info
}

public static class TestPropertyItems
{
    public static readonly TestProperty TestKind = Register("TestKind", typeof(string));
    public static readonly TestProperty TestData = Register("TestData", typeof(string));
    public static readonly TestProperty RegexSourceKind = Register("RegexSourceKind", typeof(string));
    public static readonly TestProperty DeclaringTypeName = Register("DeclaringTypeName", typeof(string));
    public static readonly TestProperty MemberName = Register("MemberName", typeof(string));
    public static readonly TestProperty RegexOptionsValue = Register("RegexOptionsValue", typeof(int));
    public static readonly TestProperty AttributeIndex = Register("AttributeIndex", typeof(int));

    private static TestProperty Register(string name, Type type) =>
        TestProperty.Register($"RegexUnitTester.{name}", name, type, typeof(TestPropertyItems));
}
