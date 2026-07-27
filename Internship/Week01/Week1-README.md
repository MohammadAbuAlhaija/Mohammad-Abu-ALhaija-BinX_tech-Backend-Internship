# Week 1 – Backend .NET Internship (BinX Tech)

## Overview
This week covered the foundations of .NET development: environment setup, C# language fundamentals, object-oriented programming, collections/LINQ/async programming, and Git/GitHub collaboration workflow.

---

## Day 1 – .NET Environment Setup

### SDK vs Runtime
- **SDK (Software Development Kit)**: includes everything needed to build and run .NET apps (compiler, CLI tools, runtime).
- **Runtime**: only what's needed to *run* an already-built app, without build tools.

### Project Structure
A minimal console project contains:
- `Program.cs` – application entry point
- `*.csproj` – project file defining target framework, dependencies, and build settings

### dotnet CLI Basics
```bash
dotnet new console -n MyApp   # create a new console project
dotnet build                  # compile the project
dotnet run                    # build + run in one step
dotnet --version              # check installed SDK version
```

---

## Day 2 – C# Fundamentals

### Value Types vs Reference Types
- **Value types** (`int`, `bool`, `struct`, etc.) are stored on the **Stack** and hold their data directly.
- **Reference types** (`class`, `string`, `array`, etc.) are stored on the **Heap**; the variable holds a reference (pointer) to the data.

```csharp
int a = 5;        // value type, copied by value
Person p = new();  // reference type, copied by reference
```

### Variables & Type Declaration
- `var` lets the compiler infer the type at compile time (still statically typed, not dynamic).
- Explicit typing (`int x = 5;`) improves readability in some cases.

### const vs readonly
- `const`: value fixed at **compile time**, must be assigned at declaration.
- `readonly`: value fixed at **runtime**, can be assigned in the constructor.

### Control Flow
```csharp
if (condition) { }
else if (condition2) { }
else { }

switch (value)
{
    case 1: break;
    default: break;
}

for (int i = 0; i < 10; i++) { }
while (condition) { }
foreach (var item in collection) { }
```

---

## Day 3 – Object-Oriented Programming (OOP)

### Classes vs Records vs Structs
| Type | Use Case | Mutability |
|------|----------|------------|
| `class` | Complex entities with behavior | Mutable (reference type) |
| `record` | DTOs / immutable data models | Immutable by default, value-based equality |
| `struct` | Small, lightweight data | Value type, best when immutable |

```csharp
public record PersonDto(string Name, int Age); // concise immutable data model
```

### Encapsulation
Protecting internal state using access modifiers and properties:
```csharp
public class Account
{
    private decimal _balance;
    public decimal Balance => _balance; // read-only exposure
}
```
Access modifiers: `private`, `public`, `protected`, `internal`.

### Inheritance
```csharp
public class Animal { public virtual void Speak() { } }
public class Dog : Animal { public override void Speak() { } }
```

### Interfaces
Define a contract without implementation — a class can implement multiple interfaces (unlike single class inheritance):
```csharp
public interface IShape
{
    double Area();
}
```
**Interface vs Abstract Class**: interfaces define *what* must be done (no shared code), abstract classes can provide *partial* shared implementation.

### Polymorphism
Achieved via `virtual` (base) + `override` (derived), allowing different behavior for the same method call depending on the actual object type at runtime.

---

## Day 4 – Collections, LINQ, Async/Await, Exception Handling

### Collections
- `List<T>` – ordered, resizable collection.
- `Dictionary<TKey, TValue>` – key-value pairs for fast lookups.
- Choose `List` for sequential data, `Dictionary` when lookups by key matter.

```csharp
var names = new List<string> { "Ali", "Sara" };
var ages = new Dictionary<string, int> { ["Ali"] = 25 };
```

### LINQ (Language Integrated Query)
Declarative way to query collections:
```csharp
var adults = people.Where(p => p.Age >= 18)
                    .Select(p => p.Name);
```
Common operators: `Where`, `Select`, `OrderBy`, `FirstOrDefault`, `Any`, `Count`.

### Async/Await
Enables non-blocking operations (I/O, network calls) without freezing the main thread:
```csharp
public async Task<string> GetDataAsync()
{
    var result = await httpClient.GetStringAsync(url);
    return result;
}
```
- `Task` – represents an async operation with no return value.
- `Task<T>` – represents an async operation returning a value of type `T`.

### Exception Handling
```csharp
try
{
    // risky code
}
catch (SpecificException ex)
{
    // handle specific case
}
finally
{
    // always executes (cleanup)
}
```
Custom exceptions can be created by inheriting from `Exception` for domain-specific error handling.

---

## Day 5 – Git/GitHub Workflow

### Feature Branches
Isolate each new feature/fix in its own branch to keep `main` stable:
```bash
git checkout -b feature/login-page
```

### Commit Messages
Best practice: clear, concise, explains **why**, not just what changed.
```
feat: add JWT validation middleware
fix: correct null reference in user service
```

### Pull Requests (PRs)
- Opens a review process before merging into `main`.
- Enables code review, discussion, and catching issues early.
- Encourages team collaboration and knowledge sharing.

---

## Key Takeaways
- Solid grasp of .NET tooling and project structure.
- Strong foundation in C# syntax, types, and control flow.
- Understanding of OOP principles as applied in C#.
- Practical use of collections, LINQ, and asynchronous programming patterns.
- Adopted a professional Git workflow using branches, meaningful commits, and PRs.