# Week 3 - Backend Internship Summary

## Car Dealership API: From Design to a Tested CRUD Backend

## Overview

Week 3 was focused on turning the Car Dealership API from an initial design into a working backend application.

The week started with designing the REST API and deciding how the main resources should be structured. After that, I designed and normalized the SQL Server database schema, converted that schema into Entity Framework Core models, generated the database using Code-First migrations, implemented complete CRUD operations, and finally organized and tested the API using Postman.

Instead of treating each day as a separate project, most of the work was connected. The API design from the beginning of the week became the database schema, the schema became EF Core entities, and those entities were then used by the CRUD endpoints.

The overall workflow was:

```text
REST API Design
      ↓
Database Schema & Normalization
      ↓
Entity Framework Core Models
      ↓
Code-First Migration
      ↓
SQL Server Database
      ↓
CRUD Endpoints
      ↓
Postman Testing & Documentation
```

---

# Day 1 - REST API Design & Resource Modeling

The first day focused on designing the Car Dealership Management API before writing database or CRUD code.

The main resources were defined as:

```text
Cars
Customers
Orders
```

The API followed REST conventions by using resource-based URLs and standard HTTP methods.

Examples:

```http
GET    /api/v1/cars
GET    /api/v1/cars/{id}
POST   /api/v1/cars
PUT    /api/v1/cars/{id}
DELETE /api/v1/cars/{id}
```

A nested resource was also designed to represent the relationship between customers and their orders:

```http
GET /api/v1/customers/{customerId}/orders
```

This endpoint represents all orders belonging to one customer.

## HTTP Status Codes

The API design also included expected HTTP responses.

| Operation | Expected Response |
|---|---|
| Get all cars | `200 OK` |
| Get car by ID | `200 OK` / `404 Not Found` |
| Create car | `201 Created` / `400 Bad Request` |
| Update car | `200 OK` or `204 No Content` |
| Delete car | `204 No Content` / `404 Not Found` |

API versioning was also included using URL-based versioning:

```text
/api/v1/cars
```

This allows future breaking changes to be introduced using versions such as:

```text
/api/v2/cars
```

### Main Takeaway

This day helped me understand that a REST API should be designed around resources and relationships before implementation begins.

---

# Day 2 - SQL Server Schema Design & Normalization

After defining the API resources, the next step was designing the database that would support them.

The final database included four main tables:

```text
Cars
Customers
CustomerPhones
Orders
```

One important design decision was creating a separate `CustomerPhones` table.

A customer can have more than one phone number, so storing several phone numbers inside one customer column would violate First Normal Form.

The relationship became:

```text
Customer
   │
   └── CustomerPhones
```

where one customer can have multiple phone records.

## Normalization

The database design was checked against:

### First Normal Form - 1NF

Every column contains a single atomic value.

For example, customer phone numbers were moved to their own table instead of storing multiple numbers in one field.

### Second Normal Form - 2NF

Each table uses a single primary key:

```text
Id
```

All other fields depend on that complete key.

### Third Normal Form - 3NF

Data that belongs to another entity is referenced using foreign keys instead of being duplicated.

For example:

```text
Orders.CustomerId → Customers.Id
Orders.CarId      → Cars.Id
```

## Final Relationships

```text
Customers
   │
   ├── CustomerPhones
   │
   └── Orders
           │
           └── Cars
```

The main foreign keys are:

```text
CustomerPhones.CustomerId → Customers.Id
Orders.CustomerId         → Customers.Id
Orders.CarId              → Cars.Id
```

The `Cars` table also keeps both:

```text
Id
VIN
```

The `Id` is the database primary key, while `VIN` identifies the actual physical vehicle and should remain unique.

## Column Types

Appropriate SQL Server data types were selected.

Examples:

```text
INT             → IDs and Year
NVARCHAR        → Names and general text
VARCHAR(17)     → VIN
DECIMAL(18,2)   → Monetary values
DATETIME2       → Dates and timestamps
```

Using:

```text
DECIMAL(18,2)
```

for prices was especially important because monetary values should not use floating-point types.

An ERD was also created using **dbdiagram.io** to visualize the tables, keys, and relationships.

### Main Takeaway

This day showed how API resources translate into a normalized relational database structure.

---

# Day 3 - Entity Framework Core Setup & Code-First Migrations

On Day 3, the database design was moved into the ASP.NET Core project using Entity Framework Core.

The existing `MyFirstApi` project was extended instead of starting from scratch.

The main entity classes created were:

```text
Car
Customer
CustomerPhone
Order
```

Each model included the required:

- Primary keys
- Foreign keys
- Navigation properties

The project structure included:

```text
MyFirstApi
│
├── Controllers
│
├── Data
│   └── AppDbContext.cs
│
├── Models
│   ├── Car.cs
│   ├── Customer.cs
│   ├── CustomerPhone.cs
│   └── Order.cs
│
├── Migrations
├── Program.cs
└── appsettings.json
```

## DbContext

An `AppDbContext` was created to connect the entity models to Entity Framework Core.

It exposes:

```csharp
DbSet<Car>
DbSet<Customer>
DbSet<CustomerPhone>
DbSet<Order>
```

The context was then registered in `Program.cs` using SQL Server.

Monetary values were configured using:

```csharp
.HasPrecision(18, 2)
```

to match the database design from Day 2.

## Code-First Migration

The initial migration was generated using:

```bash
dotnet ef migrations add InitialCreate
```

Then the migration was applied to SQL Server:

```bash
dotnet ef database update
```

This created the database schema directly from the C# entity models.

The database was verified using SQL Server Management Studio.

The generated tables included:

```text
Cars
Customers
CustomerPhones
Orders
__EFMigrationsHistory
```

Entity Framework Core also created the required foreign keys and indexes.

### Main Takeaway

This day connected the C# application to SQL Server and showed how Code-First migrations can create and maintain the database structure.

---

# Day 4 - CRUD Operations with Entity Framework Core

Day 4 focused on turning the database-backed API into a complete CRUD service.

The existing `CarsController` was extended with:

```http
POST   /api/cars
GET    /api/cars
GET    /api/cars/{id}
PUT    /api/cars/{id}
DELETE /api/cars/{id}
```

## Create

A new car is added using:

```csharp
_context.Cars.Add(car);

await _context.SaveChangesAsync();
```

After saving, the API returns:

```csharp
return CreatedAtAction(
    nameof(GetById),
    new { id = car.Id },
    car);
```

This produces:

```text
201 Created
```

and provides the location of the newly created resource.

Basic validation was also added before inserting the record.

---

## Read

All cars are retrieved using:

```csharp
var cars = await _context.Cars
    .AsNoTracking()
    .ToListAsync();
```

`AsNoTracking()` was used because these entities are only being read and do not need EF Core change tracking.

A single car can be retrieved using its ID:

```csharp
var car = await _context.Cars
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.Id == id);
```

If the car does not exist, the API returns:

```text
404 Not Found
```

---

## Update

The update flow is:

```text
Validate input
     ↓
Find existing car
     ↓
Update properties
     ↓
SaveChangesAsync()
```

Example:

```csharp
car.Make = updatedCar.Make;
car.Model = updatedCar.Model;
car.Year = updatedCar.Year;
car.Price = updatedCar.Price;
car.Status = updatedCar.Status;

await _context.SaveChangesAsync();
```

A successful update returns:

```text
204 No Content
```

---

## Delete

The entity is removed using:

```csharp
_context.Cars.Remove(car);

await _context.SaveChangesAsync();
```

Possible results are:

```text
204 No Content
404 Not Found
```

## EF Core Concepts Practiced

The CRUD implementation reinforced:

- Async database operations
- `SaveChangesAsync()`
- Change tracking
- `AsNoTracking()`
- LINQ queries
- Entity retrieval
- Entity updates
- Entity deletion

One important concept from this day was that changes are not actually written to SQL Server until:

```csharp
await _context.SaveChangesAsync();
```

is called.

---

# Day 5 - Testing & Documenting the API with Postman

The final day focused on making the API easier to test, verify, and share.

A complete Postman collection was created for the Cars API.

The collection included:

```text
Cars
│
├── GET - Get All Cars
├── GET - Get Car By Id
├── GET - Get Car By Id (Not Found)
├── POST - Create Car
├── POST - Create Car (Invalid)
├── PUT - Update Car
├── PUT - Update Car (Invalid)
├── PUT - Update Car (Not Found)
├── DELETE - Delete Car
└── DELETE - Delete Car (Not Found)
```

Both successful and failure scenarios were tested.

## Success Cases

| Endpoint | Expected Result |
|---|---|
| `GET /api/cars` | `200 OK` |
| `GET /api/cars/{id}` | `200 OK` |
| `POST /api/cars` | `201 Created` |
| `PUT /api/cars/{id}` | `204 No Content` |
| `DELETE /api/cars/{id}` | `204 No Content` |

## Error Cases

| Scenario | Expected Result |
|---|---|
| Invalid request body | `400 Bad Request` |
| Car does not exist | `404 Not Found` |

Testing failure cases was important because an API should behave correctly not only when the client sends valid requests, but also when invalid or missing data is provided.

---

# Automated Postman Tests

Postman test scripts were also added.

For example:

```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Response has an id", function () {
    const jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("id");
});
```

For a Not Found response:

```javascript
pm.test("Status code is 404", function () {
    pm.response.to.have.status(404);
});
```

For delete:

```javascript
pm.test("Status code is 204", function () {
    pm.response.to.have.status(204);
});
```

These scripts allow Postman to automatically check whether the API returned the expected result.

---

# Postman Environment

Instead of repeating the local URL inside every request, a Postman environment variable was created:

```text
baseUrl = http://localhost:5220
```

Requests can then use:

```text
{{baseUrl}}/api/cars
```

This makes the collection easier to move between environments.

For example:

```text
Local
Staging
Production
```

can each use a different `baseUrl` without changing every request manually.

---

# Week 3 API Endpoints

By the end of the week, the main Cars API supported:

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/cars` | Create a car |
| `GET` | `/api/cars` | Get all cars |
| `GET` | `/api/cars/{id}` | Get car by ID |
| `PUT` | `/api/cars/{id}` | Update a car |
| `DELETE` | `/api/cars/{id}` | Delete a car |

The original REST design also included:

```text
/api/v1/cars
/api/v1/customers
/api/v1/orders
```

and:

```text
GET /api/v1/customers/{customerId}/orders
```

These formed the broader design of the Car Dealership Management System.

---

# Technologies Used

Throughout Week 3 I worked with:

```text
ASP.NET Core Web API
C#
Entity Framework Core 10
SQL Server LocalDB
SQL Server Management Studio
LINQ
Postman
dbdiagram.io
Git
GitHub
Notion
```

---

# Week 3 Deliverables

By the end of the week, the main deliverables were:

- REST API resource design
- API versioning convention
- HTTP status code design
- Normalized SQL Server schema
- Database ERD
- Entity Framework Core models
- `AppDbContext`
- Code-First migration
- SQL Server database
- Complete Cars CRUD API
- Postman collection
- Success and error test cases
- Automated Postman test scripts
- Postman environment variables
- API documentation

---

# Week 3 Project Progress

The project progressed through several clear stages during the week:

```text
Day 1
REST API Design
      ↓
Day 2
Database Schema & Normalization
      ↓
Day 3
EF Core + Code-First Migration
      ↓
Day 4
CRUD Implementation
      ↓
Day 5
Postman Testing & Documentation
```

What I liked about this week was that every day depended on the previous one.

The REST resource design influenced the database structure. The database structure then became EF Core models. Those models were used inside the controller, and the finished endpoints were finally tested and documented in Postman.

Because of that, the week felt less like separate exercises and more like building one backend step by step.

---

# Key Concepts I Strengthened

During Week 3 I improved my understanding of:

### REST API Design

I became more comfortable choosing resources, endpoints, HTTP methods, nested routes, versioning, and status codes.

### Relational Database Design

I practiced normalization, primary keys, foreign keys, one-to-many relationships, and selecting suitable SQL data types.

### Entity Framework Core

I learned how entity classes, `DbContext`, migrations, SQL Server, and CRUD operations work together.

### Asynchronous Database Access

The API uses asynchronous EF Core operations such as:

```csharp
ToListAsync()
FirstOrDefaultAsync()
SaveChangesAsync()
```

### API Testing

Using Postman helped me verify both successful and failure scenarios instead of only checking the happy path.

### Reusable API Testing

Postman environments and automated tests made the API collection easier to reuse and verify.

---

# Final Reflection

Week 3 was the point where the project moved from backend concepts into a complete database-backed API.

At the beginning of the week, the Car Dealership system was mostly a REST design. By the end of the week, it had a normalized SQL Server database, Entity Framework Core integration, Code-First migrations, working CRUD endpoints, proper HTTP responses, and a reusable Postman test collection.

The biggest thing I took from this week was understanding how the different backend layers connect:

```text
API Design
   ↓
Database Design
   ↓
Entity Models
   ↓
DbContext
   ↓
Controller
   ↓
HTTP Request
   ↓
Database Operation
   ↓
HTTP Response
```

Seeing that full flow made the role of each part much clearer and gave me a stronger base for the authentication and authorization topics that follow.