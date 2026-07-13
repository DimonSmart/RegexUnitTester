# DimonSmart.RegexUnitTester

`DimonSmart.RegexUnitTester` turns examples placed next to a regular expression into VSTest test cases. It works with VSTest and, through the official VSTest bridge, Microsoft.Testing.Platform.

Install both packages in the test project:

```xml
<PackageReference Include="DimonSmart.RegexUnitTester.Attributes" Version="2.0.0" />
<PackageReference Include="DimonSmart.RegexUnitTester.TestAdapter" Version="2.0.0" />
```

## Supported regex declarations

Attributes are supported only on static members. For string patterns, each attribute's `RegexOptions` is used to create the regex. For members that return `Regex`, the returned instance is used unchanged: its options and timeout are authoritative. Attribute options on a `Regex` member are ignored and reported as a discovery warning.

```csharp
using System.Text.RegularExpressions;
using DimonSmart.RegexUnitTester.Attributes;

public static partial class PostalCodeRegexes
{
    [ShouldMatch("10001"), ShouldNotMatch("ABCDE")]
    public const string UsZip = @"^\d{5}$";

    [ShouldMatch("ABC", RegexOptions.IgnoreCase)]
    private static string CaseInsensitive => "^abc$";

    [ShouldMatch("ABC"), ShouldNotMatch("XYZ")]
    private static readonly Regex Compiled = new("^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [GeneratedRegex("^abc$", RegexOptions.IgnoreCase)]
    [ShouldMatch("ABC"), ShouldNotMatch("XYZ")]
    private static partial Regex GeneratedMethod();

#if NET9_0_OR_GREATER
    [GeneratedRegex("^abc$", RegexOptions.IgnoreCase)]
    [ShouldMatch("ABC")]
    private static partial Regex GeneratedProperty { get; }
#endif

    [ShouldMatch("")]
    private const string Empty = "^$";

    [InfoMatch("borderline")]
    private const string Informational = "^value$";
}
```

`InfoMatch` always passes and records whether the supplied text matched. Source-generated regexes are executed by invoking their generated method or property; patterns are never reconstructed from `ToString()`.

Unsupported instance members, indexers, generic methods and methods with parameters are ignored with a discovery warning.

## Packages

- `DimonSmart.RegexUnitTester.Attributes` targets `netstandard2.0`.
- `DimonSmart.RegexUnitTester.TestAdapter` targets `netstandard2.0` and `net8.0`.

## License

0BSD.
