# Week 3 - Day 4

# Implementing CRUD Operations with Entity Framework Core

## Overview

Today's session focused on **extending the existing `MyFirstApi` project** that was originally created during **Week 2 - Day 4**.

Instead of starting a new project, the goal was to continue developing the same ASP.NET Core Web API by implementing a complete CRUD (Create, Read, Update, Delete) workflow using Entity Framework Core and SQL Server.

This approach reflects how real-world applications evolve over time, where new features are added to an existing codebase rather than rebuilding the project from scratch.

---

# Project Continuation

Today's implementation was built on top of the existing **MyFirstApi** project.

The following components were already available from previous work:

* ASP.NET Core Web API project
* SQL Server database
* Entity Framework Core configuration
* Database migrations
* `AppDbContext`
* `Car`, `Customer`, `CustomerPhone`, and `Order` entities

Today's work focused on extending the API by implementing CRUD endpoints inside the existing `CarsController`.

---

# Create Operation (POST)

Implemented an endpoint to insert a new car into the database.

```csharp
_context.Cars.Add(car);

await _context.SaveChangesAsync();
```

After saving the entity, the API returns:

```csharp
return CreatedAtAction(
    nameof(GetById),
    new { id = car.Id },
    car);
```

This returns:

* HTTP **201 Created**
* The newly created object
* A **Location** header pointing to the new resource.

Basic validation was also added before inserting data.

```csharp
if (string.IsNullOrWhiteSpace(car.Make) ||
    string.IsNullOrWhiteSpace(car.Model) ||
    string.IsNullOrWhiteSpace(car.VIN) ||
    car.Year <= 0 ||
    car.Price <= 0)
{
    return BadRequest(new
    {
        message = "Invalid car data."
    });
}
```

---

# Read Operations (GET)

## Get All Cars

```http
GET /api/cars
```

Implemented using asynchronous Entity Framework Core queries.

```csharp
var cars = await _context.Cars
    .AsNoTracking()
    .ToListAsync();
```

`AsNoTracking()` improves performance because EF Core does not need to track entities that are only being read.

---

## Get Car By Id

```http
GET /api/cars/{id}
```

```csharp
var car = await _context.Cars
    .AsNoTracking()
    .FirstOrDefaultAsync(c => c.Id == id);

if (car == null)
{
    return NotFound();
}
```

Possible responses:

* **200 OK**
* **404 Not Found**

---

# Update Operation (PUT)

Implemented an endpoint for updating an existing car.

Workflow:

1. Validate input.
2. Search for the requested entity.
3. Update its properties.
4. Save the changes.

```csharp
car.Make = updatedCar.Make;
car.Model = updatedCar.Model;
car.Year = updatedCar.Year;
car.Price = updatedCar.Price;
car.Status = updatedCar.Status;

await _context.SaveChangesAsync();
```

Possible responses:

* **204 No Content**
* **400 Bad Request**
* **404 Not Found**

---

# Delete Operation (DELETE)

Implemented an endpoint that removes an existing car.

```csharp
_context.Cars.Remove(car);

await _context.SaveChangesAsync();
```

If the resource exists:

```text
204 No Content
```

Otherwise:

```text
404 Not Found
```

---

# Entity Framework Core Concepts Practiced

During today's implementation, the following EF Core concepts were reinforced:

* Async CRUD operations
* `SaveChangesAsync()`
* Change Tracking
* `AsNoTracking()`
* LINQ Async Queries
* Entity Retrieval
* Entity Updates
* Entity Deletion

One of the key concepts learned is that no database changes are actually committed until:

```csharp
await _context.SaveChangesAsync();
```

is executed.

---

# HTTP Status Codes Used

| Status Code     | Description                          |
| --------------- | ------------------------------------ |
| 200 OK          | Data retrieved successfully          |
| 201 Created     | New resource created                 |
| 204 No Content  | Update/Delete completed successfully |
| 400 Bad Request | Invalid input                        |
| 404 Not Found   | Requested resource does not exist    |

---

# Implemented API Endpoints

| Method | Endpoint         | Description             |
| ------ | ---------------- | ----------------------- |
| POST   | `/api/cars`      | Create a new car        |
| GET    | `/api/cars`      | Retrieve all cars       |
| GET    | `/api/cars/{id}` | Retrieve a specific car |
| PUT    | `/api/cars/{id}` | Update an existing car  |
| DELETE | `/api/cars/{id}` | Delete a car            |

---

# Testing with Postman

All endpoints were tested using Postman.

The following requests were created and verified:

```text
POST - Create Car
POST - Create Car (Invalid)

GET - Get All Cars
GET - Get Car By Id
GET - Get Car By Id (Not Found)

PUT - Update Car
PUT - Update Car (Invalid)
PUT - Update Car (Not Found)

DELETE - Delete Car
DELETE - Delete Car (Not Found)
```

Both successful scenarios and intentional error cases were tested to verify that the API returns the correct HTTP status codes and behaves as expected.

---

# Technologies Used

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* LINQ
* Postman
* Git
* GitHub

---

# Summary

By continuing the **MyFirstApi** project created in **Week 2 - Day 4**, today's implementation transformed the API into a complete RESTful CRUD service.

Beyond simply writing CRUD endpoints, this session strengthened my understanding of asynchronous database operations, Entity Framework Core change tracking, validation, proper REST response codes, and testing APIs with Postman. These improvements make the project much closer to a production-style backend and provide a solid foundation for upcoming features in the following training sessions.
