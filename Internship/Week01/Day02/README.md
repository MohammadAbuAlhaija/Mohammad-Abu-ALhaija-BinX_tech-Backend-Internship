## 📅 Week 1 — Day 2 C# Fundamentals I: Types, Variables & Control Flow

## Overview
Day 2 of Week 1 (BinX Tech Backend .NET Internship) focused on C# fundamentals: value types vs. reference types, variable declaration and type inference, control flow (switch expressions), and nullable reference types. Below is a summary of the hands-on lab tasks completed, with the corresponding code.

## Tasks Completed

- [x] Declared at least 3 value-type and 3 reference-type variables, printing each one's type using `GetType()`
- [x] Demonstrated value-vs-reference copy behavior, printing state before and after mutation
- [x] Implemented a grade-classifier method using a `switch` expression covering multiple score ranges
- [x] Read user input and handled a possibly-null value safely with nullable reference types enabled
- [x] Committed the day's work to GitHub with a clear commit message

## 1. Value Types vs. Reference Types

Declared value types (`int`, `double`, `bool`) and reference types (`string`, array, `List<string>`), and printed each one's runtime type using `GetType()`.

```csharp
int age = 22;
string MyName = "Mohammad";
Console.WriteLine($"age type: {age.GetType()}");
Console.WriteLine($"MyName type: {MyName.GetType()}");
```

## 2. Copy Behavior (Value vs. Reference)

Demonstrated the core distinction between the two:
- Copying an `int` (value type) creates an independent copy — changing `y` does not affect `x`.
- Copying a `List<string>` (reference type) copies the reference — changing `list2` also changes `list1`, since both point to the same object in memory.

```csharp
int x = 10;
int y = x;
y = 20; // x is still 10

List<string> list1 = new List<string> { "Apple", "Banana" };
List<string> list2 = list1;
list2[0] = "Orange"; // list1[0] is "Orange" too
```

## 3. Grade Classification (Switch Expression)

Used a `switch` expression with range patterns to map a numeric score to a letter grade (A–F), with `_` as the default/fallback case.

```csharp
string grade = score switch
{
    >= 90 => "A",
    >= 80 => "B",
    _ => "F"
};
```

## 4. Nullable Reference Types

Read console input into a `string? name` variable and used an `if (name is not null)` check to safely handle the case where no input was provided, avoiding a possible null-reference exception at compile time.

```csharp
string? name = Console.ReadLine();
if (name is not null)
{
    Console.WriteLine($"Hello, {name}!");
}
```

## Tools Used
.NET SDK • VS Code / Visual Studio • Git