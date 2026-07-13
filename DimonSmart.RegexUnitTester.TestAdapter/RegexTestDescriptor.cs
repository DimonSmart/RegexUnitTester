namespace DimonSmart.RegexUnitTester.TestAdapter;

internal sealed class RegexTestDescriptor
{
    public RegexTestDescriptor(string assemblyPath, string declaringTypeName, string memberName,
        RegexSourceKind sourceKind, RegexTestKind testKind, string testData, int regexOptionsValue,
        int attributeIndex, string? codeFilePath, int lineNumber)
    {
        AssemblyPath = assemblyPath;
        DeclaringTypeName = declaringTypeName;
        MemberName = memberName;
        SourceKind = sourceKind;
        TestKind = testKind;
        TestData = testData;
        RegexOptionsValue = regexOptionsValue;
        AttributeIndex = attributeIndex;
        CodeFilePath = codeFilePath;
        LineNumber = lineNumber;
    }

    public string AssemblyPath { get; }
    public string DeclaringTypeName { get; }
    public string MemberName { get; }
    public RegexSourceKind SourceKind { get; }
    public RegexTestKind TestKind { get; }
    public string TestData { get; }
    public int RegexOptionsValue { get; }
    public int AttributeIndex { get; }
    public string? CodeFilePath { get; }
    public int LineNumber { get; }
}
