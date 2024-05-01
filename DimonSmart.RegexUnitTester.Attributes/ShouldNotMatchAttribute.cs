

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ShouldNotMatchAttribute : Attribute
{
    public string TestData { get; }

    public ShouldNotMatchAttribute(string testData)
    {
        TestData = testData;
    }
}