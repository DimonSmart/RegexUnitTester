# DimonSmart.RegexUnitTester.Attributes

## Overview
The `DimonSmart.RegexUnitTester.Attributes` NuGet package enhances regex testing by using custom attributes to specify test cases and document regex behaviors, improving code readability and facilitating automated testing via the `DimonSmart.RegexUnitTester.TestAdapter`.

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

## Integration with DimonSmart.RegexUnitTester.TestAdapter
For automated testing, pair this package with `DimonSmart.RegexUnitTester.TestAdapter`:
```bash
Install-Package DimonSmart.RegexUnitTester.TestAdapter
```

This streamlined approach using attributes not only facilitates easier unit testing but also improves documentation directly within the code, maintaining clarity and efficiency.