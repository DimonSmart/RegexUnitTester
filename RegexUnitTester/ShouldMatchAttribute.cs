

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ShouldMatchAttribute : Attribute
{
    public string TestData { get; }

    public ShouldMatchAttribute(string testData)
    {
        TestData = testData;
    }
}