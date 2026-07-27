## 📅 Week 1 — Day 4:Collections & LINQ Basics

## Summary
Hands-on practice with LINQ fundamentals (Filter, Projection, Aggregation), combined with async/await and exception handling, using a `Student` domain model.

## Key Highlights

### 1. Collection Setup
Created a `List<Student>` with 8 students, each with varied property values (Name, Grade, Major).

### 2. LINQ Queries (the 3 required)
```csharp
// Aggregation
var NumOfCseStudents = students
    .Where(s => s.Major == "CSE")
    .Count();

// Filter + Projection
var ExcellentStudents = students
    .Where(s => s.Grade >= 90)
    .OrderByDescending(s => s.Grade)
    .Select(s => s.Name)
    .ToList();
```
- `Where` → filters students by a condition (Major or Grade).
- `Select` → projects the result down to just names (projection).
- `Count` → aggregates the number of matching students.
- `OrderByDescending` → sorts results by grade, highest first.

### 3. Async/Await
```csharp
static async Task<string> LoadMessageAsync()
{
    await Task.Delay(3000);
    return "Students loaded successfully!";
}
```
- Simulates a slow I/O operation using `Task.Delay`.
- Called with `await LoadMessageAsync()` from the top level without blocking the thread.

### 4. Exception Handling
```csharp
try
{
    int age = int.Parse(Console.ReadLine()!);
    Console.WriteLine($"Your age is {age}");
}
catch (FormatException)
{
    Console.WriteLine("Invalid input! Please enter a valid number.");
}
```
- Catches a specific exception type (`FormatException`) instead of the generic `Exception`, with a clear message for the user.

## Conclusion
All four required elements of the Day 4 lab are covered: filter + projection + aggregation with LINQ, one async method, and handling of a specific exception — all built on top of the `Student` model.