using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
namespace DimonSmart.RegexUnitTester.TestAdapter;

public class NullDiscoveryContext : IDiscoveryContext
{
    private NullDiscoveryContext()
    {
    }
    public IRunSettings? RunSettings => null;
    public static readonly NullDiscoveryContext Instance = new NullDiscoveryContext();
}