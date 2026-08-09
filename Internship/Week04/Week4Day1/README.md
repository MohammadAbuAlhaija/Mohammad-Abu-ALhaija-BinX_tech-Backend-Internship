# Week 4 - Day 1: ASP.NET Core Identity & User Registration

## Overview

Today I started Week 4 by continuing development on the same **Car Dealership API** I worked on during Week 3.

Instead of starting a new project, I copied the existing project into my `Week4Day1` folder and built today's work on top of it. This was useful because the project already had the database setup, EF Core, migrations, and the main entities such as `Cars`, `Customers`, `CustomerPhones`, and `Orders`.

My goal today was to add the first real user-management feature to the API using **ASP.NET Core Identity**.

Until this point, my API was mainly focused on dealership data and CRUD operations. Today I added the foundation for creating and managing actual application users securely.

---

## Starting with ASP.NET Core Identity

The first thing I did was add Identity support to the existing project.

From the project directory, I installed:

```powershell
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

The package installed successfully and gave the project the Identity integration needed to work with Entity Framework Core.

What became clear to me here is that I do not need to create my own `Users` table and build password management from scratch. Identity already provides a complete system for users, passwords, roles, claims, tokens, and other authentication-related data.

---

## Connecting Identity to My Existing DbContext

My project already had an `AppDbContext` that inherited from:

```csharp
DbContext
```

Since I wanted Identity to use the same database as the rest of my application, I changed it to:

```csharp
IdentityDbContext<IdentityUser>
```

So my context became:

```csharp
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPhone> CustomerPhones => Set<CustomerPhone>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>()
            .Property(car => car.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(order => order.SalePrice)
            .HasPrecision(18, 2);
    }
}
```

I kept all of my previous entities and configurations. I only extended the context so that it could manage Identity data as well.

I also learned why this line matters:

```csharp
base.OnModelCreating(modelBuilder);
```

Identity has its own entity relationships and database configuration, so calling the base implementation allows those configurations to be added together with my own.

---

## Adding Identity Tables to My Database

After changing the context, the C# side was ready, but my actual SQL Server database still did not have any Identity tables.

So I created a new migration:

```powershell
dotnet ef migrations add AddIdentity
```

The migration built successfully.

Then I applied it:

```powershell
dotnet ef database update
```

This was one of the useful parts of today's work because I could actually see EF Core creating the Identity schema.

It created tables such as:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
AspNetUserLogins
AspNetUserTokens
```

At the same time, my original dealership tables stayed there.

So I now have one database containing both sides of the application:

```text
Car Dealership Data
├── Cars
├── Customers
├── CustomerPhones
└── Orders

Identity Data
├── AspNetUsers
├── AspNetRoles
├── AspNetUserRoles
├── AspNetUserClaims
├── AspNetUserLogins
└── ...
```

This helped me understand what `IdentityDbContext` actually does. It is not just a class I inherit from; it brings a complete Identity database model into my existing EF Core setup.

---

## Registering Identity in Program.cs

The next step was telling ASP.NET Core to actually use Identity.

I updated my DbContext registration to explicitly use `AppDbContext`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

Then I added:

```csharp
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

From this part, I understood the responsibilities a little better:

- `IdentityUser` represents the application user.
- `IdentityRole` represents roles that can later be assigned to users.
- `AppDbContext` tells Identity where its data should be stored.

After making these changes, I ran:

```powershell
dotnet build
```

and the project built successfully.

---

## Building My Registration Endpoint

With Identity connected, I created a new controller:

```text
Controllers/AuthController.cs
```

This is where I implemented my first authentication-related endpoint:

```http
POST /api/auth/register
```

The important part for me was using:

```csharp
UserManager<IdentityUser>
```

instead of directly accessing `AppDbContext` and inserting a user myself.

I injected it into the controller:

```csharp
private readonly UserManager<IdentityUser> _userManager;

public AuthController(UserManager<IdentityUser> userManager)
{
    _userManager = userManager;
}
```

Before creating a user, I check that the email and password were actually provided.

I also check whether the email is already registered:

```csharp
var existingUser = await _userManager.FindByEmailAsync(email);
```

Then I create the user object:

```csharp
var user = new IdentityUser
{
    UserName = email,
    Email = email
};
```

The most important line in today's work was:

```csharp
var result = await _userManager.CreateAsync(user, password);
```

At first glance it looks like a simple method call, but it actually handles a lot for me.

It validates the password, creates the user, hashes the password, and stores the user through Identity.

That means I never had to write something like:

```csharp
user.Password = password;
```

or create my own password hashing logic.

That was probably the biggest thing I understood from today's work: **password handling should be left to Identity instead of trying to build my own security logic.**

---

## My Postman Tests

After the code built successfully, I moved to Postman to test what I had implemented.

I created a new collection:

```text
Week 4 - Identity & Authentication
```

I kept it separate from my previous API requests because this collection will be focused on authentication and Identity.

My API was running locally on:

```text
http://localhost:5220
```

### First Registration Attempt

For my first registration test, I used:

```text
email    = mohammad@example.com
password = MMM123&
```

I expected it to work, but Identity returned:

```text
400 Bad Request
```

with:

```json
{
  "errors": [
    "Passwords must have at least one lowercase ('a'-'z')."
  ]
}
```

This was actually a useful result.

It showed me that the password validation was really being handled by Identity and not by some validation code I wrote in the controller.

My password had uppercase letters, numbers, and a special character, but I had forgotten a lowercase letter.

So I changed it to:

```text
MMM123&a
```

and sent the request again.

This time I got:

```text
200 OK
```

with:

```json
{
  "message": "User registered successfully.",
  "email": "mohammad@example.com"
}
```

So my first user registration worked successfully.

---

## Trying a Weak Password on Purpose

I then created another Postman request:

```text
Register User - Weak Password
```

This time I deliberately used:

```text
email    = weak@example.com
password = 123
```

The API returned:

```text
400 Bad Request
```

and Identity gave me several reasons:

```json
{
  "errors": [
    "Passwords must be at least 6 characters.",
    "Passwords must have at least one non alphanumeric character.",
    "Passwords must have at least one lowercase ('a'-'z').",
    "Passwords must have at least one uppercase ('A'-'Z')."
  ]
}
```

Seeing these errors directly in Postman helped me understand the benefit of returning `IdentityResult.Errors` from the endpoint.

Instead of just returning something vague like:

```text
Invalid password
```

the API can tell the client exactly what was wrong.

---

## What I Understand After Today's Work

Before today's work, my database mainly represented the business side of the application: cars, customers, phone numbers, and orders.

Now I understand how the user/account side can be added without manually building another authentication system.

The registration flow I implemented is basically:

```text
Postman
   ↓
POST /api/auth/register
   ↓
AuthController
   ↓
UserManager<IdentityUser>
   ↓
Validate User & Password
   ↓
Hash Password
   ↓
Entity Framework Core
   ↓
SQL Server
   ↓
AspNetUsers
```

The part I found most important is that my controller does not need to know **how** a password is hashed.

My controller only asks `UserManager` to create the user:

```csharp
await _userManager.CreateAsync(user, password);
```

Identity takes care of the security-sensitive part behind the scenes.

I also now understand the connection between the pieces better:

```text
IdentityUser
     ↓
UserManager
     ↓
IdentityDbContext
     ↓
Entity Framework Core
     ↓
SQL Server
```

Each one has a different responsibility, but they work together as one user-management system.

---

## What I Used Today

- ASP.NET Core Identity
- Entity Framework Core
- `IdentityUser`
- `IdentityRole`
- `IdentityDbContext`
- `UserManager`
- EF Core Migrations
- SQL Server
- ASP.NET Core Controllers
- Postman
- C#
- .NET 10

---

## Today's Progress

Today I took the existing **Car Dealership API** and added the foundation for user accounts without rebuilding the project or changing the business entities I already had.

I connected ASP.NET Core Identity to my existing database, generated its schema using EF Core migrations, registered the required services, created my own registration endpoint, and tested both successful and failed registrations in Postman.

The most useful part for me was seeing the complete process work instead of only reading about Identity. I saw the new tables being generated, used `UserManager` in an actual controller, successfully registered a user, and then saw Identity reject weak passwords with specific validation errors.

This gave me a much clearer picture of why ASP.NET Core Identity is useful and why user passwords should not be handled manually inside the application.