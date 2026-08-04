# Week 2 - Day 5
## Entity Framework Core Setup & Code-First Migrations

### Overview
Today's focus was on integrating Entity Framework Core with the existing ASP.NET Core Web API. I configured EF Core with SQL Server, created entity models based on the previously designed ERD, set up the `DbContext`, generated the initial code-first migration, and successfully created the database using SQL Server LocalDB.

---

## What I Learned

- Installing and configuring Entity Framework Core with SQL Server.
- Creating entity classes from an ERD.
- Defining relationships using navigation properties.
- Creating and configuring a `DbContext`.
- Registering `DbContext` in the dependency injection container.
- Managing connection strings through `appsettings.json`.
- Creating and applying Code-First Migrations.
- Verifying the generated database using SQL Server Management Studio (SSMS).

---

## Project Structure

```
MyFirstApi
│
├── Controllers/
├── Data/
│   └── AppDbContext.cs
│
├── Models/
│   ├── Car.cs
│   ├── Customer.cs
│   ├── CustomerPhone.cs
│   └── Order.cs
│
├── Migrations/
├── Program.cs
└── appsettings.json
```

---

## Implemented Features

### Entity Models

Created entity classes matching the ERD:

- Customer
- CustomerPhone
- Car
- Order

Each entity includes:

- Primary Keys
- Foreign Keys
- Navigation Properties

---

### Entity Framework Core Configuration

Configured Entity Framework Core with SQL Server by:

- Installing EF Core SQL Server provider.
- Installing EF Core Tools.
- Registering `AppDbContext`.
- Configuring the SQL Server connection string.

---

### DbContext

Created `AppDbContext` containing:

- `DbSet<Customer>`
- `DbSet<CustomerPhone>`
- `DbSet<Car>`
- `DbSet<Order>`

Configured decimal precision for monetary values using:

```csharp
.HasPrecision(18, 2)
```

---

### Code-First Migration

Generated the initial migration:

```bash
dotnet ef migrations add InitialCreate
```

Applied it to SQL Server:

```bash
dotnet ef database update
```

---

### Database Verification

Verified the generated database using SQL Server Management Studio (SSMS).

Created tables:

- Cars
- Customers
- CustomerPhones
- Orders
- __EFMigrationsHistory

Foreign key relationships and indexes were generated automatically by Entity Framework Core.

---

## Technologies Used

- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server LocalDB
- SQL Server Management Studio (SSMS)
- Code-First Migrations
- C#