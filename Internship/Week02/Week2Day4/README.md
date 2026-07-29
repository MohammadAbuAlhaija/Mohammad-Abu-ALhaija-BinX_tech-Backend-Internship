
## Week 2 - Day 4: MyFirstApi
## Project Overview

This is a beginner-level ASP.NET Core Web API project built to practice the fundamentals of backend development with .NET. The goal was to get hands-on experience with:

- Controllers
- Minimal APIs
- Routing
- Route parameters
- HTTP GET requests
- Swagger / OpenAPI
- Postman

The project combines both Controller-based endpoints and Minimal API endpoints in the same app, so I could see how each approach works side by side.

## Technologies Used

- C#
- ASP.NET Core Web API
- .NET 10
- Swagger UI
- OpenAPI
- Postman

## Project Structure

```text
MyFirstApi
├── Controllers
│   └── BooksController.cs
├── Postman
│   └── MyFirstApi.postman_collection.json
├── Program.cs
├── appsettings.json
└── MyFirstApi.csproj
```

## Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MyFirstApi");
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5)
        .Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/cars", () =>
{
    var cars = new List<string>
    {
        "BMW",
        "Kia",
        "Toyota"
    };

    return cars;
});

app.MapGet("/cars/{id}", (int id) =>
{
    var cars = new List<string>
    {
        "BMW",
        "Kia",
        "Toyota"
    };

    if (id < 1 || id > cars.Count)
    {
        return Results.NotFound();
    }

    return Results.Ok(cars[id - 1]);
});

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

A quick breakdown of what's happening here:

- `AddControllers()` enables Controller support so the app can use classes like `BooksController`.
- `AddOpenApi()` generates the OpenAPI document describing the API.
- `UseSwaggerUI()` displays that documentation as an interactive page in the browser.
- `MapControllers()` maps the routes defined inside Controller classes.
- `MapGet()` creates a Minimal API GET endpoint directly in `Program.cs`, without needing a separate Controller.

## BooksController.cs

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetBooks()
    {
        var books = new List<string>
        {
            "Clean Code",
            "C# Programming",
            "Python Programming"
        };

        return Ok(books);
    }

    [HttpGet("{id}")]
    public IActionResult GetBookById(int id)
    {
        var books = new List<string>
        {
            "Clean Code",
            "C# Programming",
            "Python Programming"
        };

        if (id < 1 || id > books.Count)
        {
            return NotFound();
        }

        return Ok(books[id - 1]);
    }
}
```

What this controller does:

- `GET /api/Books` returns the full list of books.
- `GET /api/Books/{id}` returns a single book by its ID.
- If the ID doesn't match any book in the list, the endpoint returns a `404 Not Found`.

## API Endpoints

| Method | Route | Type | Description |
|--------|-------|------|-------------|
| GET | `/api/Books` | Controller | Returns all books |
| GET | `/api/Books/{id}` | Controller | Returns one book by ID |
| GET | `/cars` | Minimal API | Returns all cars |
| GET | `/cars/{id}` | Minimal API | Returns one car by ID |

Default endpoint included with the project template:

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/weatherforecast` | Returns sample weather data |

## Example Responses

### GET /api/Books

```json
[
  "Clean Code",
  "C# Programming",
  "Python Programming"
]
```

### GET /api/Books/1

```json
"Clean Code"
```

### GET /cars

```json
[
  "BMW",
  "Kia",
  "Toyota"
]
```

### GET /cars/1

```json
"BMW"
```

## How to Run

```bash
dotnet restore
dotnet run
```

Once it's running, the terminal will print the local URL the API is listening on, for example:

```text
http://localhost:5220
```

Note that the port number can be different on another machine — just use whatever the terminal shows.

## Swagger UI

With the project running, Swagger can be opened at:

```text
http://localhost:5220/swagger
```

The project needs to stay running in the terminal for Swagger to work.

## Testing with Postman

All four endpoints were tested manually and saved as a Postman Collection:

```text
GET http://localhost:5220/api/Books
GET http://localhost:5220/api/Books/1
GET http://localhost:5220/cars
GET http://localhost:5220/cars/1
```

The exported Postman JSON file is included in this repository under `Postman/MyFirstApi.postman_collection.json`.

## Controllers vs Minimal APIs

- **Controllers** keep related endpoints organized inside a separate class, which works well for larger APIs with many related routes.
- **Minimal APIs** define endpoints directly in `Program.cs`, which is quicker and works well for small APIs or simple examples like this one.

## Learning Outcomes

- Scaffolded an ASP.NET Core Web API project.
- Understood `Program.cs` and the minimal hosting model.
- Created Controller-based endpoints.
- Created Minimal API endpoints.
- Used route parameters.
- Returned `200 OK` and `404 Not Found` responses.
- Tested endpoints using Swagger and Postman.
- Exported a Postman Collection.