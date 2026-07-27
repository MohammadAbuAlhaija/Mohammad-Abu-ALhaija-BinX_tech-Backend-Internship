# Week 2 – Backend .NET Internship (BinX Tech)
### Summary

## Day 1: Generics & Advanced Collections
- Built a generic `Repository<T>` reusable across different reference types with full type safety.
- Used a generic constraint (`where T : class`) to restrict `T` to reference types.
- Exposed data via `IReadOnlyList<T>` (using `AsReadOnly()`) to prevent callers from modifying the internal collection.
- Implemented lookup with `Predicate<T>` for flexible searching.
- Confirmed type safety: attempting to modify a read-only list caused a compile-time error.

## Day 2: Advanced LINQ & Deferred Execution
- Practiced grouping data with `GroupBy` (e.g., aggregating totals per customer).
- Combined related collections using `Join`.
- Flattened nested collections into a single sequence using `SelectMany`.
- Learned deferred execution: LINQ queries run only when enumerated (e.g. in a `foreach`), not when defined — so changes to the source collection before enumeration are reflected in the result.