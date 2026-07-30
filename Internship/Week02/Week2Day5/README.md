# Week 2 - Day 5: Middleware Pipeline & Dependency Injection

## Learning Objectives
- Explain the middleware pipeline.
- Understand middleware execution order.
- Learn built-in middleware.
- Understand Dependency Injection (DI).
- Learn service lifetimes (Transient, Scoped, Singleton).
- Use constructor/dependency injection.

## Project Overview
This project demonstrates how ASP.NET Core processes HTTP requests through the middleware pipeline, and how Dependency Injection (DI) is used to provide services automatically to endpoints without manually creating them.

## Tasks Completed

### 1. Custom Logging Middleware
I created a custom middleware that logs two pieces of information for every incoming request:
- HTTP Request Method
- HTTP Request Path

```csharp
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    Console.WriteLine($"Request Method: {context.Request.Method}");
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next(context);
});
```

### 2. Middleware Ordering
I intentionally placed a terminal middleware in the wrong position in the pipeline to observe how request processing stops when a middleware doesn't call the next component.

```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Request stopped here");
});
```

Since `app.Run` is terminal middleware, it ends the pipeline immediately and prevents the request from reaching any middleware or endpoint registered after it. After observing this behavior, I restored the correct middleware order so requests could flow through the full pipeline and reach the intended endpoints successfully.

### 3. Dependency Injection
I created an interface and a service implementing it, then registered the service with the built-in DI container.

**IGreetingService.cs**
```csharp
public interface IGreetingService
{
    string GetGreeting();
}
```

**GreetingService.cs**
```csharp
public class GreetingService : IGreetingService
{
    public string GetGreeting()
    {
        return "Hello from Greeting Service!";
    }
}
```

**Service Registration**
```csharp
builder.Services.AddScoped<IGreetingService, GreetingService>();
```

### 4. Injecting the Service
The `IGreetingService` was injected directly into a Minimal API endpoint, letting the DI container supply the implementation automatically at request time.

```csharp
app.MapGet("/greeting", (IGreetingService greetingService) =>
{
    return greetingService.GetGreeting();
});
```

## Key Concepts Learned
- Middleware Pipeline
- Request and Response flow
- Middleware execution order
- Custom Middleware
- Dependency Injection
- Service Registration
- Scoped Lifetime
- Interface-based programming
- Minimal API endpoint injection

## Technologies
- C#
- .NET 10
- ASP.NET Core Web API
- Minimal API

## Outcome
This project demonstrates how ASP.NET Core handles incoming requests through an ordered middleware pipeline, and how Dependency Injection provides services to endpoints in a clean, testable way. By building a custom logging middleware, experimenting with middleware ordering, and injecting a service into a Minimal API endpoint, this exercise reinforces the core mechanics every ASP.NET Core application relies on to process requests and manage dependencies.