using System.Reflection;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.TestAdapter;

public static class RegexInformation
{
    public static int? GetTrackCount(string pattern)
    {
        var regex = new Regex(pattern);
        var regexType = typeof(Regex);
        var regexCodeField = regexType.GetField("_code", BindingFlags.NonPublic | BindingFlags.Instance);
        var regexCode = regexCodeField?.GetValue(regex);

        if (regexCode == null)
            return null;

        var regexCodeType = regexCode.GetType();
        var trackCountField = regexCodeType.GetField("TrackCount", BindingFlags.Public | BindingFlags.Instance);

        return trackCountField?.GetValue(regexCode) as int?;
    }
}
