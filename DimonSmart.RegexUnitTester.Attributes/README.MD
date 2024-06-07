# DimonSmart.RegexUnitTester.Attributes

⚠ - [This package is for attributes only! for TestAdapter please check TestAdapter package](https://nuget.org/packages/DimonSmart.RegexUnitTester.TestAdapter)


## Overview
The `DimonSmart.RegexUnitTester.Attributes` NuGet package enhances regex testing by using custom attributes to specify test cases and document regex behaviors. This package focuses solely on providing attribute tools for marking test cases directly in your code, improving code readability and documentation.

## Features
- **ShouldMatchAttribute:** Asserts that a regex should match the provided test data.
- **ShouldNotMatchAttribute:** Asserts that a regex should not match the provided test data, useful for patterns intended to exclude certain formats.
- **InfoMatchAttribute:** Captures cases that fall into a gray area, neither strictly matching nor non-matching, for logging and informational purposes without triggering errors.

## Installation
To install `DimonSmart.RegexUnitTester.Attributes`, run:
```bash
Install-Package DimonSmart.RegexUnitTester.Attributes
```

## Comparison Example
### Traditional Comments
```csharp
// Regex "10001" matches, "ABCDE" does not match, "123 45" is logged.
public const string USZipCodeRegex = @"^\d{5}$";
```

### Attribute Usage
```csharp
[ShouldMatch("10001"), ShouldNotMatch("ABCDE"), InfoMatch("123 45")]
public const string USZipCodeRegex = @"^\d{5}$";
```

## Detailed Attribute Usage Example
```csharp
using System.Text.RegularExpressions;
using DimonSmart.RegexUnitTester.Attributes;

namespace PostalCodeValidation
{
    public class PostalCodeRegexTests
    {
        [ShouldMatch("10001"), ShouldNotMatch("ABCDE"), InfoMatch("123 45")]
        public const string USZipCodeRegex = @"^\d{5}$";

        [ShouldMatch("A1A 1A1"), ShouldNotMatch("12345"), InfoMatch("A1A-1A1")]
        public const string CanadaPostalCodeRegex = @"^[A-Z]\d[A-Z] \d[A-Z]\d$";
    }
}
```

### Key Scenarios
- **ShouldMatch:** "10001" correctly matches the US postal code format.
- **ShouldNotMatch:** "ABCDE" fails to match as it's not a valid postal code format.
- **InfoMatch:** "123 45" represents a borderline case which is logged for potential reevaluation but does not trigger a test failure.

## Related Packages
For automated testing integration, consider using the `DimonSmart.RegexUnitTester.TestAdapter` package. This adapter works seamlessly with the attributes provided here to facilitate automated unit testing.

```bash
Install-Package DimonSmart.RegexUnitTester.TestAdapter
```
[Find out more about the TestAdapter package here](https://nuget.org/packages/DimonSmart.RegexUnitTester.TestAdapter)

## License
This project is licensed under the **0BSD License**, which is one of the most permissive licenses available. This means that the `DimonSmart.RegexUnitTester.Attributes` are **completely free** for both personal and commercial use. You are allowed to use, modify, and distribute the software without any restrictions.

For more details on the license, you can review it [here](https://opensource.org/licenses/0BSD).
