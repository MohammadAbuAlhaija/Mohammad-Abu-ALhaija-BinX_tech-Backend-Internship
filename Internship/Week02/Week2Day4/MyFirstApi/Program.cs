using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and OpenAPI services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register the database context and connect it to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Enable OpenAPI and Swagger UI during development
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

// Sample weather endpoint
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

// Sample endpoint that returns all cars
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

// Sample endpoint that returns one car by id
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