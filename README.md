# DimonSmart.RegexUnitTester.TestAdapter

## Overview
**Easily test your regex patterns without writing a single line of code!** The `DimonSmart.RegexUnitTester.TestAdapter` leverages custom attributes to automatically generate and execute regex tests, streamlining your testing process by eliminating the need for manual test script writing.

## Features
- **Purpose:** Automatically runs unit tests for regex patterns using specialized attributes:
  - **`ShouldMatchAttribute`**: Ensures a regex correctly matches expected data.
  - **`ShouldNotMatchAttribute`**: Confirms a regex fails to match unwanted data.
  - **`InfoMatchAttribute`**: Handles ambiguous cases for further analysis without affecting test outcomes.

For detailed descriptions and usage examples, check the [Attributes README](https://nuget.org/packages/DimonSmart.RegexUnitTester.Attributes).

## Installation
To install `DimonSmart.RegexUnitTester.TestAdapter`, execute:
```bash
Install-Package DimonSmart.RegexUnitTester.TestAdapter
```

## Contribution
Contributions are welcomed; please visit our contribution guidelines for more information.

## License
This project is licensed under the **0BSD License**, which is one of the most permissive licenses available. This means that the `DimonSmart.RegexUnitTester.TestAdapter` are **completely free** for both personal and commercial use. You are allowed to use, modify, and distribute the software without any restrictions.

For more details on the license, you can review it [here](https://opensource.org/licenses/0BSD).
