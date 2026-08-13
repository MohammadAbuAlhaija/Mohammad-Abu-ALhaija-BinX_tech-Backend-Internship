using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyFirstApi.Validators;
using MyFirstApi.Data;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


// Add controllers and OpenAPI services
builder.Services.AddControllers();


// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Return 429 instead of the default rejection status code.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // General API limit: 20 requests per minute
    options.AddFixedWindowLimiter("general", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // Stricter limit for login: 5 requests per minute
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Enable automatic FluentValidation validation.
builder.Services.AddFluentValidationAutoValidation();

// Register validators from this project.
builder.Services.AddValidatorsFromAssemblyContaining<CreateCarValidator>();

builder.Services.AddOpenApi();


// Register the database context and connect it to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// Register ASP.NET Core Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();


// Configure JWT bearer authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Accept tokens issued only by this API.
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                // Make sure the token is intended for this API/client.
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                // Check the expiration time of the token.
                ValidateLifetime = true,

                // Reject the token immediately after it expires.
                ClockSkew = TimeSpan.Zero,

                // Verify that the token was signed with our secret key.
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )
                )
            };
    });


// Define a policy that requires the ManageCars permission claim.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageCars", policy =>
    {
        policy.RequireClaim("Permission", "ManageCars");
    });
});


var app = builder.Build();


// Create the default roles if they do not already exist.
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "User", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Get UserManager to assign roles to test users.
    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Find test users by email.
    var adminUser =
        await userManager.FindByEmailAsync("admin@example.com");

    var normalUser =
        await userManager.FindByEmailAsync("mohammad@example.com");

    // Assign Admin role.
    if (adminUser != null &&
        !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Assign User role.
    if (normalUser != null &&
        !await userManager.IsInRoleAsync(normalUser, "User"))
    {
        await userManager.AddToRoleAsync(normalUser, "User");
    }
}


// Enable OpenAPI and Swagger UI during development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MyFirstApi");
    });
}
else
{
    // Enable HSTS in production.
    // This tells browsers to use HTTPS only for this application.
    app.UseHsts();
}


// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();


// Apply the named CORS policy.
app.UseCors("AllowFrontend");


// Enable rate limiting middleware.
app.UseRateLimiter();


// Read and validate authentication information from the request.
app.UseAuthentication();

// Apply authorization rules such as [Authorize].
app.UseAuthorization();


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
.WithName("GetWeatherForecast")
.RequireRateLimiting("general");


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
})
.RequireRateLimiting("general");


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
})
.RequireRateLimiting("general");


app.MapControllers();

app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}