using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CardiacPatientMonitoringSystem.Services;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();

// Built-in OpenAPI
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cardiac Patient Monitoring System API",
        Version = "v1"
    });

    // JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        }
    );
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
);

// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});

// Application Services
builder.Services.AddScoped<PatientVisitService>();

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!
            )
        )
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await DevelopmentDataSeeder.SeedAsync(
        context,
        userManager,
        roleManager
    );
}

// Seed application roles and initial Admin account
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Seed Roles
    string[] roles =
    {
        "Admin",
        "Doctor",
        "Patient"
    };

    foreach (var roleName in roles)
    {
        var roleExists =
            await roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            var roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName)
                );

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    $"Failed to create role '{roleName}'."
                );
            }
        }
    }

    // Seed Initial Admin
    var adminEmail = "admin@cardiac.com";
    var adminPassword = "Admin@12345";

    var existingAdmin =
        await userManager.FindByEmailAsync(adminEmail);

    if (existingAdmin == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createAdminResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword
            );

        if (!createAdminResult.Succeeded)
        {
            throw new Exception(
                "Failed to create initial Admin account."
            );
        }

        var addAdminRoleResult =
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin"
            );

        if (!addAdminRoleResult.Succeeded)
        {
            throw new Exception(
                "Failed to assign Admin role to initial Admin account."
            );
        }
    }
}

// Development tools
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Cardiac Patient Monitoring System API v1"
        );
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<CardiacPatientMonitoringSystem.Middleware.GlobalExceptionMiddleware>();
app.UseMiddleware<CardiacPatientMonitoringSystem.Middleware.RequestLoggingMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();

public partial class Program { }

