# How to suppress analyzer warnings

There are several ways to suppress a diagnostic warning produced by `Xunit.v3.IntegrationTesting` analyzers. The examples below use `XIT0001` as a placeholder — replace it with the actual rule ID you want to suppress.

## Via `.editorconfig`

Add a rule to your `.editorconfig` file to change the severity (or disable) a diagnostic for an entire project or folder:

```ini
[*.cs]
dotnet_diagnostic.XIT0001.severity = none
```

Valid severity values are `none`, `silent`, `suggestion`, `warning`, and `error`.

## Via `SuppressMessageAttribute`

Apply `[SuppressMessage]` to suppress the warning on a specific member or type:

```csharp
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("Usage", "XIT0001", Justification = "Custom orderer handles dependencies")]
public class MyTests
{
    // ...
}
```

This attribute can be applied at the method, class, or assembly level.

## Via `#pragma` directives

Use `#pragma warning disable` / `#pragma warning restore` to suppress a warning for a specific section of code:

```csharp
#pragma warning disable XIT0001
[TestCaseOrderer(typeof(MyCustomOrderer))]
public class MyTests
{
    // ...
}
#pragma warning restore XIT0001
```
