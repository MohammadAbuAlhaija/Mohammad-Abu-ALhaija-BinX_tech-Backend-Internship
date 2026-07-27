## 📅 Week 2 — Day 1:Generics & Advanced Collections

## Concept
Built a generic `Repository<T>` reusable across any reference type, while keeping full type safety.

## Generic Constraint
```csharp
class Repository<T> where T : class
```
Restricts `T` to reference types, since the repository is meant to store objects, not value types.

## Add & GetAll
```csharp
public void Add(T item) => items.Add(item);
public IReadOnlyList<T> GetAll() => items.AsReadOnly();
```
`GetAll` returns `IReadOnlyList<T>` instead of `List<T>` — prevents the caller from modifying the collection directly, enforced via `AsReadOnly()`.

## Find with a Predicate
```csharp
public T? Find(Predicate<T> predicate) => items.Find(predicate);
```

## Usage
```csharp
Repository<Book> bookRepository = new();
bookRepository.Add(new Book("Clean Code"));

IReadOnlyList<Book> books = bookRepository.GetAll();
```
The same `Repository<T>` was reused with two different types (`Book` and `Library`) with zero code duplication.

## Note
Tried modifying an item in `books` (typed as `IReadOnlyList<Book>`) directly → resulted in a compile-time error, confirming the constraint works as intended.