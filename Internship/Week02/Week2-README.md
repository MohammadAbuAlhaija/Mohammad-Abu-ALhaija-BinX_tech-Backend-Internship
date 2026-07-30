# Week 2 - Backend .NET Internship Summary (BinX Tech)

### Advanced C# & ASP.NET Core Foundations

## Overview
Week 2 expanded on the C# fundamentals from Week 1 by introducing generics, advanced LINQ, asynchronous programming with `async/await`, and the basics of ASP.NET Core Web API. Throughout the week, I practiced building reusable components, writing efficient data queries, working with concurrent tasks, and developing my first Web API using both Controllers and Minimal APIs.

---

## Daily Breakdown

### Day 1 — Generics & Advanced Collections
Built a generic `Repository<T>` class using the `where T : class` constraint and explored collection interfaces such as `IEnumerable<T>`, `IReadOnlyList<T>`, and `IList<T>` to better understand abstraction, encapsulation, and collection design.

```csharp
public class Repository<T> where T : class
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();
}
```

---

### Day 2 — Advanced LINQ & Deferred Execution
Practiced advanced LINQ operations including `GroupBy`, `Join`, and `SelectMany`, and learned the difference between deferred and immediate execution by observing how queries behave when the source collection changes.

```csharp
var ordersByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new
    {
        CustomerId = g.Key,
        Total = g.Sum(o => o.Amount)
    });
```

---

### Day 3 — Async/Await Deep Dive & Concurrency
Implemented asynchronous methods using `async` and `await`, executed multiple tasks concurrently with `Task.WhenAll`, and added support for `CancellationToken` to safely cancel long-running operations.

```csharp
var result = await GetDataAsync();
```

---

### Day 4 — ASP.NET Core Project Setup & Routing
Created my first ASP.NET Core Web API using `dotnet new webapi`, explored the minimal hosting model, implemented API endpoints using both Controllers and Minimal APIs, and tested routing and HTTP endpoints using Postman.

```csharp
app.MapGet("/api/items/{id}", (int id) =>
    items.FirstOrDefault(i => i.Id == id));
```

---

### Day 5 — Middleware Pipeline & Dependency Injection
Implemented a custom logging middleware to inspect incoming requests, experimented with middleware ordering to understand request execution flow, registered services using Dependency Injection with the `Scoped` lifetime, and injected a service into a Minimal API endpoint.

```csharp
builder.Services.AddScoped<IGreetingService, GreetingService>();
```

---

## Key Concepts Learned

- Generic classes and generic constraints
- Collection interfaces and abstraction
- Advanced LINQ (GroupBy, Join, SelectMany)
- Deferred vs. Immediate Execution
- Asynchronous programming with `async` and `await`
- Concurrency using `Task.WhenAll`
- Cancellation with `CancellationToken`
- ASP.NET Core project structure
- Controllers and Minimal APIs
- Routing and HTTP endpoints
- Middleware pipeline and execution order
- Dependency Injection (DI)
- Service lifetimes (Transient, Scoped, Singleton)
- Interface-based design and service registration

---

## Deliverables

- Generic repository implementation
- LINQ practice exercises
- Async and concurrency demo using `Task.WhenAll`
- ASP.NET Core Web API with Controllers and Minimal API endpoints
- Custom logging middleware
- Dependency Injection example using a registered service
- Complete Week 2 source code published to GitHub

---

## Technologies

- C#
- .NET 10
- ASP.NET Core Web API
- Controllers
- Minimal APIs
- Middleware
- Dependency Injection (DI)
- Postman
- Git & GitHub

---

## Outcome

By the end of Week 2, I strengthened my understanding of advanced C# concepts and successfully built my first ASP.NET Core Web API. I learned how requests move through the middleware pipeline, how dependency injection manages application services, and how to expose HTTP endpoints using both Controllers and Minimal APIs. These skills provide a solid foundation for integrating databases with Entity Framework Core in the following weeks.