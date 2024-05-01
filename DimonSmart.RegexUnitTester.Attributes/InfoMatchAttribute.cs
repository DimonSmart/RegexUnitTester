[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class InfoMatchAttribute : Attribute
{
    public InfoMatchAttribute(string testData)
    {
        TestData = testData;
    }

    public string TestData { get; }
}