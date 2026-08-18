# Cardiac Patient Monitoring System

## Project Overview

The **Cardiac Patient Monitoring System** is an individual ASP.NET Core Web API project that I built to apply the backend concepts I learned during my training in a complete and practical project.

The idea of the system is to provide a backend API for managing cardiac patients and some of the information related to their monitoring and care.

The system currently manages four main resources:

- Patients
- Vital Signs
- Medications
- Appointments

I built the project step by step, starting with the project structure and data models, then connecting the API to SQL Server using Entity Framework Core. After that, I implemented the CRUD operations, authentication and authorization, input validation, middleware, seed data, filtering, Swagger documentation, Postman testing, and automated testing using xUnit, Moq, and WebApplicationFactory for both unit and integration testing.

The project uses only synthetic test data and does not contain real patient information.

---

## What I Practiced in This Project

This project helped me connect many backend concepts together instead of practicing each one separately.

During the project, I worked with:

- ASP.NET Core Web API
- Controllers and Routing
- Dependency Injection
- Middleware
- Async/Await
- LINQ
- DTOs
- Entity Framework Core
- SQL Server
- Code-First Migrations
- Entity Relationships
- CRUD Operations
- Synthetic Seed Data
- ASP.NET Core Identity
- JWT Authentication
- Protected API Routes
- FluentValidation
- HTTP Status Codes
- Filtering
- Swagger / OpenAPI
- Postman
- xUnit
- Unit Testing
- `[Fact]` and `[Theory]`
- Arrange-Act-Assert
- Moq
- Mocking Dependencies
- Repository Interfaces
- Mock Verification
- Integration Testing
- WebApplicationFactory
- HttpClient Testing
- EF Core In-Memory Database
- Testing Protected Endpoints with JWT

The most useful part for me was seeing how these concepts work together in one project.

I was also able to move from testing APIs manually using Postman and Swagger to writing automated tests at different levels. Unit tests allowed me to test small pieces of application logic directly, while integration tests allowed me to verify multiple parts of the API working together.

---

# Project Structure

I organized the project into separate folders so that each part has a clear responsibility.

```text
Project1/
│
├── CardiacPatientMonitoringSystem/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Middleware/
│   ├── Migrations/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Validators/
│   ├── Program.cs
│   └── appsettings.json
│
├── CardiacPatientMonitoringSystem.Tests/
│   ├── PatientServiceTests.cs
│   ├── PatientsApiTests.cs
│   └── CustomWebApplicationFactory.cs
│
├── Postman/
│   └── Cardiac Patient Monitoring System.postman_collection.json
│
├── screenshots/
│
└── README.md
```

### Main Folders

**Controllers/**  
Contains the API endpoints for authentication, patients, vital signs, medications, and appointments.

**Models/**  
Contains the Entity Framework Core entities that represent the database tables.

**DTOs/**  
Contains the request objects used when creating and updating resources. I used DTOs so that API requests are separated from the database entities.

**Data/**  
Contains `AppDbContext`, which is responsible for the connection between Entity Framework Core and SQL Server.

**Migrations/**  
Contains the EF Core migrations used to create and update the database schema.

**Validators/**  
Contains the FluentValidation rules for the Create and Update request DTOs.

**Middleware/**  
Contains the custom request logging middleware.

**Repositories/**  
Contains repository interfaces used to separate service logic from its dependencies. I added `IPatientRepository` while practicing dependency isolation and mocking with Moq.

**Services/**  
Contains simple application logic that can be separated from the controllers. `PatientService` currently contains the patient age calculation and a method that retrieves a patient's name through `IPatientRepository`.

**CardiacPatientMonitoringSystem.Tests/**  
Contains the automated unit and integration tests written using xUnit, Moq, and WebApplicationFactory.

The test project includes service-level unit tests, mocked dependency tests, and HTTP integration tests against the API.

---

# Database Design

I used **SQL Server LocalDB** with **Entity Framework Core Code First**.

The main entities are:

```text
Patient
 ├── VitalSigns
 ├── Medications
 └── Appointments
```

A patient can have multiple vital-sign measurements, medications, and appointments.

The relationships are therefore:

```text
Patient 1 ---- * VitalSign

Patient 1 ---- * Medication

Patient 1 ---- * Appointment
```

Each related entity contains a `PatientId` foreign key.

For example:

```csharp
public int PatientId { get; set; }

public Patient Patient { get; set; } = null!;
```

This helped me understand how one-to-many relationships are represented both in C# classes and in the SQL Server database.

---

# Entity Framework Core and Migrations

I configured `AppDbContext` to expose the main entities through `DbSet`.

```csharp
public DbSet<Patient> Patients { get; set; }
public DbSet<VitalSign> VitalSigns { get; set; }
public DbSet<Medication> Medications { get; set; }
public DbSet<Appointment> Appointments { get; set; }
```

I used EF Core migrations to build and update the database instead of manually creating the tables.

The project currently includes migrations for:

- Initial database creation
- ASP.NET Core Identity
- Synthetic seed data

The main commands I used were:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

This gave me more practice with the Code-First workflow and helped me understand how changes in the C# models can be reflected in the database schema.

---

# Synthetic Seed Data

I added synthetic seed data so the database contains example information after the migrations are applied.

The seed data includes:

- A patient
- A vital-sign measurement
- A medication
- An appointment

The seeded patient uses:

```text
Patient ID: 1001
```

For example, I can retrieve the seeded patient using:

```http
GET /api/patients/1001
```

### Seed Data Test

![Seed Data Patient](./screenshots/seed-data-patient.png)

Adding seed data made the project easier to test after creating the database because I do not need to manually create every record before verifying the API.

---

# Patient API

The Patient API supports full asynchronous CRUD operations.

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/patients` | Get all patients |
| GET | `/api/patients/{id}` | Get a patient by ID |
| POST | `/api/patients` | Create a patient |
| PUT | `/api/patients/{id}` | Update a patient |
| DELETE | `/api/patients/{id}` | Delete a patient |

I used asynchronous EF Core methods such as:

```csharp
await _context.Patients.ToListAsync();
await _context.Patients.FindAsync(id);
await _context.SaveChangesAsync();
```

This gave me practical experience using `async` and `await` inside a real Web API instead of only using them in isolated examples.

### Get All Patients

![Get All Patients](./screenshots/patients-get-all.png)

### Delete Patient

![Delete Patient](./screenshots/patients-Delete.png)

---

# Vital Signs API

The Vital Signs module stores measurements related to a patient.

The current model contains:

- Heart Rate
- Systolic Blood Pressure
- Diastolic Blood Pressure
- Measurement Date and Time
- Patient ID

The available endpoints are:

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/vitalsigns` | Get all vital signs |
| GET | `/api/vitalsigns/{id}` | Get a vital sign by ID |
| POST | `/api/vitalsigns` | Create a vital sign |
| PUT | `/api/vitalsigns/{id}` | Update a vital sign |
| DELETE | `/api/vitalsigns/{id}` | Delete a vital sign |

Before creating or updating a vital sign, I also check that the referenced patient exists.

```csharp
var patientExists = await _context.Patients
    .AnyAsync(p => p.Id == request.PatientId);
```

This prevents creating a vital-sign record for a patient that does not exist.

### Get All Vital Signs

![Get All Vital Signs](./screenshots/vitalsigns-get-all.png)

### Delete Vital Sign

![Delete Vital Sign](./screenshots/vitalsigns-delete.png)

---

# Medications API

The Medication module allows medications to be associated with a patient.

Each medication contains information such as:

- Medication name
- Dosage
- Frequency
- Start date
- Optional end date
- Patient ID

Available endpoints:

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/medications` | Get all medications |
| GET | `/api/medications/{id}` | Get a medication by ID |
| POST | `/api/medications` | Create a medication |
| PUT | `/api/medications/{id}` | Update a medication |
| DELETE | `/api/medications/{id}` | Delete a medication |

Just like Vital Signs, I verify that the patient exists before creating or updating the medication.

### Get All Medications

![Get All Medications](./screenshots/medications-get-all.png)

### Delete Medication

![Delete Medication](./screenshots/medications-delete.png)

---

# Appointments API

The Appointment module manages patient appointments.

An appointment contains:

- Patient ID
- Appointment date
- Doctor name
- Reason
- Status

Available endpoints:

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/appointments` | Get all appointments |
| GET | `/api/appointments/{id}` | Get an appointment by ID |
| POST | `/api/appointments` | Create an appointment |
| PUT | `/api/appointments/{id}` | Update an appointment |
| DELETE | `/api/appointments/{id}` | Delete an appointment |

### Get All Appointments

![Get All Appointments](./screenshots/appointments-get-all.png)

### Delete Appointment

![Delete Appointment](./screenshots/appointments-delete.png)

---

# LINQ Filtering

I used LINQ to add filtering to the Appointments endpoint.

For example:

```http
GET /api/appointments?status=Scheduled
```

The controller builds an EF Core query and only applies the filter when a status is provided.

```csharp
var query = _context.Appointments.AsQueryable();

if (!string.IsNullOrWhiteSpace(status))
{
    query = query.Where(a => a.Status == status);
}

var appointments = await query.ToListAsync();
```

This was useful because it showed me how LINQ expressions can become database queries when working with Entity Framework Core.

### Filter Appointments by Status

![Filter Appointments](./screenshots/appointments-filter-by-status.png)

---

# Authentication with ASP.NET Core Identity

I integrated **ASP.NET Core Identity** with the same EF Core database.

Identity is responsible for storing and managing application users, including password hashing.

The authentication API provides:

```http
POST /api/auth/register
POST /api/auth/login
```

For the current training implementation, the register and login endpoints accept `email` and `password` parameters.

### Register User

![Register User](./screenshots/auth-register.png)

---

# JWT Authentication

After a successful login, the API generates a **JSON Web Token (JWT)**.

The token contains claims identifying the user and has a limited expiration time.

The JWT is signed using:

```text
HMAC SHA-256
```

The API validates:

- Issuer
- Audience
- Token lifetime
- Signing key

A successful login returns a token that can then be sent with protected API requests.

### Login and JWT Generation

![Login JWT](./screenshots/auth-login-jwt.png)

Working with JWT helped me understand the difference between:

**Authentication** — determining who the user is.

and

**Authorization** — deciding whether the request can access a protected endpoint.

---

# Protected Routes

The main resource controllers are protected using:

```csharp
[Authorize]
```

This includes:

- Patients
- Vital Signs
- Medications
- Appointments

The authentication endpoints remain available without a JWT because users need to register and login before they have a token.

If I try to access a protected endpoint without a token, the API rejects the request.

### Request Without JWT

![Unauthorized Request](./screenshots/auth-protected-401.png)

The response is:

```text
401 Unauthorized
```

After sending a valid JWT using Bearer authentication, the same protected endpoint can be accessed successfully.

### Request With JWT

![Authorized Request](./screenshots/auth-protected-200.png)

This returns:

```text
200 OK
```

This part helped me understand the complete authentication flow:

```text
Register
   ↓
Login
   ↓
Generate JWT
   ↓
Send Bearer Token
   ↓
JWT Validation
   ↓
Protected Controller
```

---

# Input Validation with FluentValidation

I used **FluentValidation** instead of placing validation logic directly inside every controller.

I created validators for both Create and Update DTOs for:

- Patient
- Vital Sign
- Medication
- Appointment

For example, the Patient validator checks that:

- Full name is provided
- Date of birth is provided
- Date of birth is in the past
- Gender is provided
- Phone number is provided
- Address is provided

Example:

```csharp
RuleFor(x => x.FullName)
    .NotEmpty()
    .WithMessage("Patient full name is required.");

RuleFor(x => x.DateOfBirth)
    .NotEmpty()
    .LessThan(DateTime.Today)
    .WithMessage("Date of birth must be in the past.");
```

I registered the validators in the application pipeline:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientValidator>();
```

One thing I found useful here is that validation happens before the controller performs the database operation, which keeps the controller cleaner.

### Validation Error

I tested an invalid Patient request in Postman.

![Patient Validation Error](./screenshots/validation-patient-400.png)

The API correctly returns:

```text
400 Bad Request
```

with clear validation errors.

---

# HTTP Status Codes

While testing the API, I made sure that the endpoints return HTTP status codes that match the result of the request.

Some of the main responses I tested are:

| Status Code | Meaning | Example |
| --- | --- | --- |
| `200 OK` | Successful request | Getting resources |
| `201 Created` | Resource successfully created | Creating a patient |
| `204 No Content` | Successful operation without response body | Update/Delete |
| `400 Bad Request` | Invalid input | FluentValidation failure |
| `401 Unauthorized` | Authentication required | Missing JWT |
| `404 Not Found` | Resource does not exist | Invalid patient ID |

---

# 404 Not Found Handling

I also tested what happens when a client requests a resource that does not exist.

Example:

```http
GET /api/patients/99999
```

Instead of returning an incorrect success response, the controller returns:

```text
404 Not Found
```

### Patient Not Found

![Patient Not Found](./screenshots/patient-not-found-404.png)

This helped me practice handling expected API failure scenarios and returning a status code that clearly describes the result.

---

# Custom Middleware

I added a custom request logging middleware to better understand the ASP.NET Core request pipeline.

The middleware logs the HTTP method and path before continuing the request:

```csharp
Console.WriteLine(
    $"Request: {context.Request.Method} {context.Request.Path}"
);
```

It then calls the next component:

```csharp
await _next(context);
```

After the request finishes, it logs the response status code:

```csharp
Console.WriteLine(
    $"Response Status: {context.Response.StatusCode}"
);
```

For example:

```text
Request: GET /api/patients
Response Status: 200
```

This helped me understand that middleware sits in the request pipeline and can execute logic before and after the next component handles the request.

---

# Unit Testing with xUnit

As the next step in the project, I started adding automated unit tests using **xUnit**.

I created a separate test project:

```text
CardiacPatientMonitoringSystem.Tests
```

and referenced the main `CardiacPatientMonitoringSystem` project from it.

This keeps the tests separate from the main API while still allowing them to access the application classes that need to be tested.

## PatientService

For the first unit-testing exercise, I added a simple `PatientService` containing a pure method for calculating a patient's age.

```csharp
public int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
{
    int age = referenceDate.Year - dateOfBirth.Year;

    if (dateOfBirth.Date > referenceDate.AddYears(-age).Date)
    {
        age--;
    }

    return age;
}
```

The method does not depend on the database, HTTP requests, or any external service, which made it suitable for a basic unit test.

I used a reference date as an input instead of using the current system date directly. This keeps the test predictable because the same input will always produce the same expected result.

## Fact Tests

I wrote three `[Fact]` tests for `CalculateAge()`.

The tests cover:

- A birthday that has already passed during the reference year.
- A birthday that has not occurred yet.
- A birthday that occurs on the reference date.

Each test follows the **Arrange-Act-Assert** pattern.

```text
Arrange
   ↓
Prepare the service and test data

Act
   ↓
Call CalculateAge()

Assert
   ↓
Verify the returned age
```

Using this pattern made each test easier to read because the setup, action, and expected result are clearly separated.

## Theory Test

I also created a `[Theory]` test using `[InlineData]`.

Instead of writing a separate test method for every set of inputs, the same test can run multiple times with different dates and expected ages.

The Theory currently covers three different input cases.

This helped me understand the main difference between the two xUnit test types:

**`[Fact]`** — useful for testing one specific scenario.

**`[Theory]`** — useful for running the same test logic with multiple sets of input data.

## Running the Tests

I can run the test project using:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

The first test run contained:

```text
3 Fact test cases
+
3 Theory input cases
=
6 test cases
```

All six test cases passed successfully.

### xUnit Test Results

![xUnit Tests Passed](./screenshots/xunit-tests-passed.png)

Adding unit testing showed me a different way of verifying the project. Postman and Swagger are useful for testing the API through HTTP requests, while xUnit allows me to test a small piece of application logic directly without starting the API or connecting to the database.

---

# Mocking Dependencies with Moq

After starting unit testing with xUnit, I continued improving the test setup by learning how to isolate a service from its dependencies using **Moq**.

The goal was to test `PatientService` without connecting to the real database.

To do this, I added an `IPatientRepository` interface:

```csharp
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
}
```

I then updated `PatientService` so that it receives the repository through its constructor:

```csharp
private readonly IPatientRepository _patientRepository;

public PatientService(IPatientRepository patientRepository)
{
    _patientRepository = patientRepository;
}
```

This allows the repository dependency to be replaced with a controlled mock during unit testing.

I also added `GetPatientNameAsync()`, which uses the repository to retrieve a patient and return the patient's name.

```csharp
public async Task<string> GetPatientNameAsync(int id)
{
    try
    {
        var patient = await _patientRepository.GetByIdAsync(id);

        if (patient == null)
        {
            return "Patient not found";
        }

        return patient.FullName;
    }
    catch (Exception)
    {
        return "Unable to retrieve patient";
    }
}
```

---

## Mocking Repository Return Values

I used Moq to create a mock implementation of `IPatientRepository`.

```csharp
var mockRepo = new Mock<IPatientRepository>();
```

Using `Setup()` and `ReturnsAsync()`, I configured the mock to return a specific patient.

```csharp
var patient = new Patient
{
    Id = 1,
    FullName = "Ahmad Ali"
};

mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(patient);
```

I then called `GetPatientNameAsync(1)` and checked that the service returned:

```text
Ahmad Ali
```

This allowed me to test how the service processes the repository result without accessing SQL Server or Entity Framework Core.

---

## Testing Dependency Failures

I also tested what happens when the repository throws an exception.

Using `ThrowsAsync()`, I configured the mock repository to simulate a failure:

```csharp
mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ThrowsAsync(new Exception("Database error"));
```

The service handles the exception and returns:

```text
Unable to retrieve patient
```

This made it possible to test a failure scenario without intentionally causing a real database failure.

---

## Verifying Repository Calls

In addition to checking the returned result, I used Moq's `Verify()` method to make sure that `PatientService` actually called the repository.

```csharp
mockRepo.Verify(
    r => r.GetByIdAsync(1),
    Times.Once
);
```

This confirms that:

```csharp
GetByIdAsync(1)
```

was called exactly once.

This helped me understand that a unit test can check both the final result and the interaction between the service and its dependency.

---

## Moq Test Results

I ran the complete test project using:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

The existing xUnit tests and the new Moq tests passed successfully.

### Moq Tests Passed

![Moq Tests Passed](./screenshots/moq-tests-passed.png)

Using Moq helped me understand how a service can be tested independently from a real dependency by controlling what that dependency returns, simulating failures, and verifying how it was called.

---

# Integration Testing with WebApplicationFactory

After unit testing individual methods and services, I continued by adding integration testing using `WebApplicationFactory`.

The goal was to test the API through real HTTP requests while still running everything inside a test environment.

Unlike the previous unit tests, these tests verify multiple parts of the application working together, including routing, JWT authentication, controllers, Entity Framework Core, and HTTP responses.

---

## Setting Up WebApplicationFactory

I added the ASP.NET Core integration testing package:

```bash
dotnet add CardiacPatientMonitoringSystem.Tests/CardiacPatientMonitoringSystem.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

Because the application uses top-level statements in `Program.cs`, I exposed the generated `Program` class for the test project.

At the end of `Program.cs`, I added:

```csharp
public partial class Program { }
```

This allows the test project to use:

```csharp
WebApplicationFactory<Program>
```

and start the API inside the test environment without manually running the application.

---

## Using a Separate Test Database

I did not want the integration tests to use the real SQL Server development database.

For this reason, I added the EF Core In-Memory provider:

```bash
dotnet add CardiacPatientMonitoringSystem.Tests/CardiacPatientMonitoringSystem.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
```

I then created:

```text
CustomWebApplicationFactory.cs
```

The custom factory replaces the normal SQL Server configuration with an isolated EF Core In-Memory database while integration tests are running.

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(
        "CardiacPatientMonitoringTestDb"
    );
});
```

The test database is recreated before the integration tests run:

```csharp
context.Database.EnsureDeleted();
context.Database.EnsureCreated();
```

This keeps test data isolated from the normal development database.

During the setup, I initially encountered a database provider conflict because both SQL Server and the In-Memory provider were registered in the test environment.

The error indicated that:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

and:

```text
Microsoft.EntityFrameworkCore.InMemory
```

were both registered.

I fixed the problem by removing the existing `AppDbContext` database configuration inside `ConfigureTestServices()` before registering the In-Memory provider.

This helped me better understand how dependency injection configuration can be replaced specifically for integration testing.

---

## Get Patient Integration Test

I created:

```text
PatientsApiTests.cs
```

The test class uses an `HttpClient` created by `CustomWebApplicationFactory`.

The first integration test covers the successful Get-by-ID path:

```http
GET /api/patients/1001
```

The expected response is:

```text
200 OK
```

The response is deserialized into a `Patient` object:

```csharp
var patient =
    await response.Content.ReadFromJsonAsync<Patient>();
```

I then verify the returned patient information:

```csharp
Assert.Equal(1001, patient.Id);
Assert.Equal("Ahmad Khalil", patient.FullName);
Assert.Equal(new DateTime(1985, 6, 15), patient.DateOfBirth);
Assert.Equal("Male", patient.Gender);
Assert.Equal("0599123456", patient.PhoneNumber);
Assert.Equal("Jenin", patient.Address);
```

This verifies that the API returns the expected patient data from the test database.

---

## Testing the Not Found Path

I also added an integration test for a patient ID that does not exist:

```http
GET /api/patients/99999
```

The expected response is:

```text
404 Not Found
```

The test verifies the status code using:

```csharp
Assert.Equal(
    HttpStatusCode.NotFound,
    response.StatusCode
);
```

This confirms that the same API endpoint correctly handles both successful and not-found scenarios when tested through HTTP.

---

## Testing a Protected Endpoint with JWT

The `PatientsController` is protected using:

```csharp
[Authorize]
```

Because of this, the integration test needs a valid JWT before it can access the endpoint.

I created a test JWT using the same issuer, audience, signing key, and signing algorithm expected by the API.

The token is attached to the request using:

```csharp
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue(
        "Bearer",
        token
    );
```

The request then passes through the real JWT authentication middleware before reaching the controller.

This allows the integration test to verify an authenticated and protected endpoint while still running entirely inside the test environment.

---

## Integration Test Flow

The integration tests follow a flow similar to:

```text
xUnit Integration Test
        ↓
HttpClient
        ↓
ASP.NET Core Pipeline
        ↓
JWT Authentication
        ↓
Routing
        ↓
PatientsController
        ↓
AppDbContext
        ↓
In-Memory Test Database
        ↓
HTTP Response
        ↓
Assertions
```

This is different from the unit tests because multiple parts of the application are being tested together.

---

## Integration Test Results

I ran the complete test project using:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests/CardiacPatientMonitoringSystem.Tests.csproj
```

The final result was:

```text
Total tests: 10
Passed: 10
Failed: 0
```

This includes the existing xUnit tests, the Moq tests, and the new integration tests.

### Integration Tests Passed

![Integration Tests Passed](./screenshots/integration-tests-passed.png)

Adding integration tests helped me understand how automated testing can verify the API from a client's point of view without manually running Swagger or Postman.

---

# Swagger / OpenAPI

I configured Swagger so the API can be explored and tested without needing a separate frontend application.

Swagger displays the available controllers and endpoints and also supports JWT Bearer authentication.

After running the project, Swagger can currently be opened at:

```text
http://localhost:5075/swagger
```

> The development port may change depending on the local launch configuration. The active URL is also displayed in the terminal when the API starts.

### Swagger API Overview

![Swagger API Overview](./screenshots/swagger-api-overview.png)

Swagger was useful for seeing the API as one complete system instead of testing every endpoint individually.

---

# Postman Testing

I also created a Postman collection for testing the API.

The collection contains requests for:

### Authentication

- Register User
- Login User
- Protected request without token
- Protected request with token

### Patients

- Create
- Get All
- Get By ID
- Update
- Delete
- Not Found test
- Validation error test

### Vital Signs

- Create
- Get All
- Get By ID
- Update
- Delete

### Medications

- Create
- Get All
- Get By ID
- Update
- Delete

### Appointments

- Create
- Get All
- Get By ID
- Update
- Delete
- Filter by status

The exported Postman collection is available at:

```text
Postman/Cardiac Patient Monitoring System.postman_collection.json
```

Using Postman throughout development helped me test each feature immediately after implementing it instead of waiting until the whole project was finished.

---

# Running the Project

## Requirements

To run the project locally, the following are required:

- .NET 10 SDK
- SQL Server LocalDB
- Entity Framework Core CLI tools

---

## 1. Open the Project

From the `Project1` directory:

```bash
cd CardiacPatientMonitoringSystem
```

---

## 2. Restore Packages

```bash
dotnet restore
```

---

## 3. Create / Update the Database

Run:

```bash
dotnet ef database update
```

This applies the existing migrations and creates the required SQL Server database structure and seed data.

---

## 4. Run the API

```bash
dotnet run
```

The terminal will display the local address used by the API.

In my current development configuration, I use:

```text
http://localhost:5075
```

---

## 5. Open Swagger

```text
http://localhost:5075/swagger
```

---

# Running the Automated Tests

The automated test project can be run separately from the API.

From the `Project1` directory:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests/CardiacPatientMonitoringSystem.Tests.csproj
```

This builds both the main API project and the test project and then runs the available automated tests.

The API does not need to be started manually in Swagger or Postman before running the tests.

The unit tests execute application logic directly, while the integration tests use `WebApplicationFactory` to start the API inside the test environment and send HTTP requests through an in-memory `HttpClient`.

The integration tests also use an isolated EF Core In-Memory database instead of the normal SQL Server development database.

---

# Database Configuration

The development database connection is configured using:

```text
(localdb)\MSSQLLocalDB
```

Database name:

```text
CardiacPatientMonitoringDb
```

Entity Framework Core uses the `DefaultConnection` connection string from the application configuration.

The integration test environment does not use this database.

Instead, it replaces the SQL Server configuration with:

```text
CardiacPatientMonitoringTestDb
```

using the EF Core In-Memory provider.

---

# Example API Flow

A normal authenticated request in the project follows a flow similar to:

```text
Client / Postman / Swagger
          ↓
ASP.NET Core Middleware
          ↓
JWT Authentication
          ↓
Authorization
          ↓
FluentValidation
          ↓
Controller
          ↓
Entity Framework Core
          ↓
SQL Server
          ↓
HTTP Response
```

Building the project this way helped me see how the individual topics from the training connect together inside a real backend application.

The unit tests that use Moq follow a smaller path:

```text
xUnit Test
    ↓
PatientService
    ↓
Mock<IPatientRepository>
    ↓
Controlled Result
    ↓
Assert / Verify
```

The integration tests use a wider application flow:

```text
Integration Test
      ↓
HttpClient
      ↓
ASP.NET Core Pipeline
      ↓
JWT Authentication
      ↓
Controller
      ↓
Entity Framework Core
      ↓
In-Memory Test Database
      ↓
HTTP Response
      ↓
Assertions
```

This helped me understand that different types of tests verify the application at different levels.

---

# Screenshots and Testing Evidence

The following screenshots were captured while implementing and testing the project.

## Patients

### Get All Patients

![Get All Patients](./screenshots/patients-get-all.png)

### Delete Patient

![Delete Patient](./screenshots/patients-Delete.png)

### Patient Not Found - 404

![Patient Not Found](./screenshots/patient-not-found-404.png)

---

## Vital Signs

### Get All Vital Signs

![Get All Vital Signs](./screenshots/vitalsigns-get-all.png)

### Delete Vital Sign

![Delete Vital Sign](./screenshots/vitalsigns-delete.png)

---

## Medications

### Get All Medications

![Get All Medications](./screenshots/medications-get-all.png)

### Delete Medication

![Delete Medication](./screenshots/medications-delete.png)

---

## Appointments

### Get All Appointments

![Get All Appointments](./screenshots/appointments-get-all.png)

### Delete Appointment

![Delete Appointment](./screenshots/appointments-delete.png)

### Filter Appointments by Status

![Appointment Filtering](./screenshots/appointments-filter-by-status.png)

---

## Authentication

### Register

![Register](./screenshots/auth-register.png)

### Login and JWT

![Login JWT](./screenshots/auth-login-jwt.png)

### Protected Endpoint Without Token

![401 Unauthorized](./screenshots/auth-protected-401.png)

### Protected Endpoint With Token

![200 Authorized](./screenshots/auth-protected-200.png)

---

## Validation

### Invalid Patient Request

![Validation Error](./screenshots/validation-patient-400.png)

---

## Seed Data

### Seeded Patient

![Seed Patient](./screenshots/seed-data-patient.png)

---

## Automated Testing

### xUnit Tests

![xUnit Tests Passed](./screenshots/xunit-tests-passed.png)

### Moq Tests

![Moq Tests Passed](./screenshots/moq-tests-passed.png)

### Integration Tests

![Integration Tests Passed](./screenshots/integration-tests-passed.png)

---

## Swagger

### API Documentation

![Swagger](./screenshots/swagger-api-overview.png)

---

# What I Learned

The biggest benefit of this project was moving from small separate exercises into one API where multiple backend concepts depend on each other.

I became more comfortable with:

- Designing entities and relationships before implementing endpoints.
- Using DTOs instead of exposing entity classes directly for create/update requests.
- Working with SQL Server through Entity Framework Core.
- Creating and applying migrations.
- Writing asynchronous CRUD operations.
- Using LINQ inside EF Core queries.
- Understanding foreign-key relationships between resources.
- Using Identity for user and password management.
- Generating and validating JWT tokens.
- Protecting API endpoints.
- Validating incoming requests before performing database operations.
- Returning meaningful HTTP status codes.
- Understanding where middleware executes in the ASP.NET Core pipeline.
- Testing APIs with both Postman and Swagger.
- Creating a separate xUnit test project.
- Referencing the main API project from a test project.
- Understanding the difference between `[Fact]` and `[Theory]`.
- Using `[InlineData]` to test multiple input cases.
- Structuring unit tests using Arrange-Act-Assert.
- Testing simple application logic independently from the API and database.
- Understanding why dependencies should be isolated during unit testing.
- Creating mocks with Moq.
- Configuring mock return values using `Setup()` and `ReturnsAsync()`.
- Simulating dependency failures using `ThrowsAsync()`.
- Using `Verify()` and `Times.Once` to check dependency interactions.
- Understanding the difference between unit tests and integration tests.
- Using `WebApplicationFactory` to run the API inside the test environment.
- Sending HTTP requests from integration tests using `HttpClient`.
- Replacing SQL Server with an EF Core In-Memory database during integration testing.
- Keeping integration test data isolated from the development database.
- Testing protected API endpoints using a valid JWT.
- Troubleshooting dependency injection and database provider conflicts during integration testing.
- Keeping testing evidence while developing instead of only testing at the end.

I also became more comfortable reading the complete request flow and understanding which part of the application is responsible for each operation.

The introduction to unit testing helped me understand that not every test needs to go through the complete HTTP request pipeline. Some logic can be tested directly.

Using Moq extended this idea by allowing me to test a service even when it depends on another component, without using the real dependency during the test.

Integration testing added another level by allowing me to send HTTP requests through the real ASP.NET Core pipeline and verify routing, authentication, controllers, and database access together.

---

# Current Project Status

At the current stage, I have completed the main API structure, database integration, CRUD modules, authentication, validation, middleware, filtering, seed data, Swagger documentation, Postman verification, automated unit testing with xUnit and Moq, and API integration testing using WebApplicationFactory.

The current xUnit tests cover the `CalculateAge()` method using `[Fact]` and `[Theory]`.

The Moq tests cover a service method that depends on `IPatientRepository`.

They include:

- Returning controlled repository data.
- Testing how the service processes the returned data.
- Simulating a repository exception.
- Verifying that a repository method is called exactly once.

The integration tests currently cover:

- A successful `GET /api/patients/1001` request.
- Verifying the full returned patient response.
- A `404 Not Found` request for a missing patient.
- Using an isolated EF Core In-Memory test database.
- Accessing a protected endpoint using a valid test JWT.

The complete automated test project currently contains:

```text
Total tests: 10
Passed: 10
Failed: 0
```

The remaining training-aligned work will be completed after covering the corresponding topics:

- Centralized error handling
- Additional automated testing as the project continues

After the remaining topics are implemented, I will perform the final project cleanup and update the documentation and test instructions accordingly.

---

# Technologies Used

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- EF Core In-Memory
- ASP.NET Core Identity
- JWT Bearer Authentication
- FluentValidation
- LINQ
- xUnit
- Moq
- Microsoft.AspNetCore.Mvc.Testing
- WebApplicationFactory
- Swagger / OpenAPI
- Postman
- Git / GitHub

---

## Final Note

This project is being developed as an individual backend training project.

My goal is not only to make the endpoints work, but also to understand how the main parts of an ASP.NET Core backend application work together, from receiving an HTTP request to validating it, authenticating the user, accessing the database, and returning the correct response.

As the project continues, I am also building automated tests at different levels.

Using xUnit helped me understand how application logic can be tested directly.

Using Moq helped me isolate services from their dependencies.

Using WebApplicationFactory helped me move one step further and test the API through real HTTP requests inside an isolated test environment.

Together, these testing approaches helped me understand the difference between testing one piece of code in isolation and testing multiple parts of the backend application working together.