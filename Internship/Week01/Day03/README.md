## 📅 Week 1 — Day 3 C# Fundamentals II: Object-Oriented Programming

**Duration:** 8 hours
**Week:** Week 1 — Phase 1 (Onboarding & Foundations)

## Learning Objectives
- Choose between a `class`, a `record`, and a `struct` for a given modeling need.
- Apply encapsulation using access modifiers and properties.
- Use inheritance and interfaces to model shared behavior and polymorphism.

## Key Topics
1. Classes, Records, and Structs
2. Encapsulation and Access Modifiers
3. Inheritance and Interfaces
4. Polymorphism in Practice

## Quick Summary

| Concept | Use |
|---|---|
| `class` | Reference type, identity + behavior, mutable state |
| `record` | Immutable reference type, value-based equality — ideal for DTOs |
| `struct` | Small, immutable value type (coordinate, money amount...) |
| Encapsulation | Private fields + public properties protecting object state |
| Interface | Behavior contract with no implementation, for unrelated classes sharing a capability |
| Polymorphism | Handling different types through one shared base type/interface |

> **Important note:** Don't default to inheritance. If the relationship between two types is really "can do the same thing" rather than "is fundamentally the same kind of thing," an interface is almost always the better fit.

---

## Today's Code — Key Snippets (Library Exercise)

**1. `record` for an immutable DTO:**
```csharp
public record BookDto(string Title, string Author, int Year);
```

**2. `class` with proper encapsulation (private fields + public properties):**
```csharp
public class Book
{
    private string? title;
    public string? Title
    {
        get { return title; }
        set { title = value; }
    }
    // ... same pattern for Author and Year

    public Book(string title, string author, int year)
    {
        Title = title;
        Author = author;
        Year = year;
    }
}
```

**3. Interface implemented by two unrelated classes → polymorphism:**
```csharp
public interface IPrintable
{
    void Print();
}

public class BookReport : IPrintable
{
    public void Print() => Console.WriteLine("Printing Book Report...");
}

public class LibraryCard : IPrintable
{
    public void Print() => Console.WriteLine("Printing Library Card...");
}

void PrintItem(IPrintable item) => item.Print();

PrintItem(new BookReport());
PrintItem(new LibraryCard());
```

**4. A class holding a collection (sets up Day 4):**
```csharp
public class Library
{
    public List<Book> books = new List<Book>();
    public void AddBook(Book book) => books.Add(book);
    public void removeBook(Book book) => books.Remove(book);
}
```

### Connecting the Code to the Concepts
- **`BookDto`**: a `record` example — immutable, well-suited for data transfer.
- **`Book`**: full encapsulation — private backing fields and public properties, with a constructor that sets valid initial state.
- **`IPrintable`**: an interface implemented by two unrelated classes; `PrintItem` handles both uniformly — that's polymorphism in practice.
- **`Library`**: holds a `List<Book>` with add/remove methods — a natural lead-in to Day 4 (Collections & LINQ).

---

## Hands-On Lab Requirements
1. Design a small domain with at least 2 related classes. ✅ (Library: `Book` + `Library`)
2. Add at least one `record` type as a DTO. ✅ (`BookDto`)
3. Apply proper encapsulation: private backing fields, public properties, constructor enforcing valid state. ✅
4. Define one interface implemented by two unrelated classes, with a method accepting the interface type. ✅ (`IPrintable`)
5. Commit the work to your GitHub repository.

**Tools Used:** .NET SDK • VS Code or Visual Studio • Git