# Cardiac Patient Monitoring System

## Project Overview

The **Cardiac Patient Monitoring System** is an individual ASP.NET Core Web API project that I built to apply the backend concepts I learned during my training in a complete and practical project.

The idea of the system is to provide a backend API for managing cardiac patients and some of the information related to their monitoring and care.

The system currently manages four main resources:

- Patients
- Vital Signs
- Medications
- Appointments

I built the project step by step, starting with the project structure and data models, then connecting the API to SQL Server using Entity Framework Core. After that, I implemented the CRUD operations, authentication and authorization, input validation, middleware, seed data, filtering, Swagger documentation, Postman testing, automated testing using xUnit, Moq, and WebApplicationFactory, and centralized exception handling using ProblemDetails and structured logging.

During Week 5, I expanded the automated testing setup by prioritizing important and higher-risk logic, adding validation unit tests, and running the complete test suite together.

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
- Validation Unit Testing
- Risk-Based Test Prioritization
- Global Exception Handling
- ProblemDetails
- Structured Logging with ILogger

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
│   ├── CreatePatientValidatorTests.cs
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
Contains the custom request logging middleware and the global exception-handling middleware. The exception middleware catches unexpected errors, logs their details on the server, and returns a safe standardized `ProblemDetails` response to the client.

**Repositories/**  
Contains repository interfaces used to separate service logic from its dependencies. I added `IPatientRepository` while practicing dependency isolation and mocking with Moq.

**Services/**  
Contains simple application logic that can be separated from the controllers. `PatientService` currently contains the patient age calculation and a method that retrieves a patient's name through `IPatientRepository`.

**CardiacPatientMonitoringSystem.Tests/**  
Contains the automated unit and integration tests written using xUnit, Moq, FluentValidation, and WebApplicationFactory.

The test project includes service-level unit tests, mocked dependency tests, validation tests, and HTTP integration tests against the API.

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

### Register User

![Register User](./screenshots/auth-register.png)

---

# JWT Authentication

After a successful login, the API generates a **JSON Web Token (JWT)**.

The JWT is signed using:

```text
HMAC SHA-256
```

The API validates:

- Issuer
- Audience
- Token lifetime
- Signing key

### Login and JWT Generation

![Login JWT](./screenshots/auth-login-jwt.png)

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

### Request Without JWT

![Unauthorized Request](./screenshots/auth-protected-401.png)

```text
401 Unauthorized
```

### Request With JWT

![Authorized Request](./screenshots/auth-protected-200.png)

```text
200 OK
```

The authentication flow is:

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

For example:

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

### Validation Error

![Patient Validation Error](./screenshots/validation-patient-400.png)

The API correctly returns:

```text
400 Bad Request
```

---

# HTTP Status Codes

| Status Code | Meaning | Example |
| --- | --- | --- |
| `200 OK` | Successful request | Getting resources |
| `201 Created` | Resource successfully created | Creating a patient |
| `204 No Content` | Successful operation without response body | Update/Delete |
| `400 Bad Request` | Invalid input | FluentValidation failure |
| `401 Unauthorized` | Authentication required | Missing JWT |
| `404 Not Found` | Resource does not exist | Invalid patient ID |
| `500 Internal Server Error` | Unexpected server error | Global exception handler |

---

# 404 Not Found Handling

Example:

```http
GET /api/patients/99999
```

returns:

```text
404 Not Found
```

### Patient Not Found

![Patient Not Found](./screenshots/patient-not-found-404.png)

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

After the request finishes:

```csharp
Console.WriteLine(
    $"Response Status: {context.Response.StatusCode}"
);
```

Example:

```text
Request: GET /api/patients
Response Status: 200
```

---

# Global Exception Handling

I added centralized exception handling to the project using:

```text
GlobalExceptionMiddleware.cs
```

The middleware wraps the remaining request pipeline:

```csharp
try
{
    await _next(context);
}
catch (Exception ex)
{
    // Log the exception and return a safe response
}
```

This provides one central place for handling unexpected exceptions.

---

## ProblemDetails Response

For unexpected errors, the API returns a standardized response:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "instance": "/api/patients/test-error"
}
```

The response uses:

```text
500 Internal Server Error
application/problem+json
```

The actual exception message and stack trace are not exposed to the client.

---

## Structured Exception Logging

The real exception is logged on the server using `ILogger`.

```csharp
_logger.LogError(
    ex,
    "Unhandled exception occurred for request {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

This keeps debugging information on the server while returning a safe response to the client.

### Global Exception Response

![Global Exception Response](./screenshots/global-exception-500.png)

### Server Exception Log

![Global Exception Log](./screenshots/global-exception-log.png)

---

# Unit Testing with xUnit

I created a separate xUnit test project:

```text
CardiacPatientMonitoringSystem.Tests
```

For the first unit-testing exercise, I added a `PatientService` containing a method for calculating a patient's age.

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

I used a reference date instead of the current system date directly so the tests remain predictable.

I wrote three `[Fact]` tests covering:

- Birthday already passed.
- Birthday not yet reached.
- Birthday occurring on the reference date.

I also created a `[Theory]` with three `[InlineData]` cases.

The tests follow:

```text
Arrange
   ↓
Act
   ↓
Assert
```

The first test suite contained:

```text
3 Fact cases
+
3 Theory cases
=
6 test cases
```

### xUnit Test Results

![xUnit Tests Passed](./screenshots/xunit-tests-passed.png)

---

# Mocking Dependencies with Moq

I continued testing by learning how to isolate `PatientService` from its dependency using **Moq**.

I created:

```csharp
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
}
```

`PatientService` receives the repository through constructor injection:

```csharp
private readonly IPatientRepository _patientRepository;

public PatientService(IPatientRepository patientRepository)
{
    _patientRepository = patientRepository;
}
```

Using Moq, I can control what the repository returns:

```csharp
var mockRepo = new Mock<IPatientRepository>();

mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(patient);
```

I also simulated a dependency failure:

```csharp
mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ThrowsAsync(new Exception("Database error"));
```

and verified the repository interaction:

```csharp
mockRepo.Verify(
    r => r.GetByIdAsync(1),
    Times.Once
);
```

### Moq Tests Passed

![Moq Tests Passed](./screenshots/moq-tests-passed.png)

---

# Integration Testing with WebApplicationFactory

After unit testing individual methods and services, I added integration testing using `WebApplicationFactory`.

The goal was to test the API through real HTTP requests while still running everything inside a test environment.

The tests verify multiple parts of the application together:

```text
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

## Separate Test Database

I used an **EF Core In-Memory database** instead of the normal SQL Server development database.

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(
        "CardiacPatientMonitoringTestDb"
    );
});
```

The database is recreated for the test environment:

```csharp
context.Database.EnsureDeleted();
context.Database.EnsureCreated();
```

---

## Get Patient Integration Tests

The integration tests cover:

```http
GET /api/patients/{id}
```

The first scenario tests:

```http
GET /api/patients/1001
```

Expected:

```text
200 OK
```

The returned patient is also checked to make sure the complete expected data is returned.

The second scenario tests:

```http
GET /api/patients/99999
```

Expected:

```text
404 Not Found
```

Because `PatientsController` is protected, the integration tests generate and send a valid test JWT.

### Integration Tests Passed

![Integration Tests Passed](./screenshots/integration-tests-passed.png)

---

# Validation Unit Testing

As part of applying the Week 5 testing concepts to the project, I added unit tests for `CreatePatientValidator`.

The validator is important because it protects the Patient API from invalid input before the controller performs a database operation.

I created:

```text
CreatePatientValidatorTests.cs
```

The tests currently cover three scenarios.

### Valid Patient

A valid request should pass validation without errors.

```csharp
var request = new CreatePatientRequest
{
    FullName = "Ahmad Khalil",
    DateOfBirth = new DateTime(1990, 5, 10),
    Gender = "Male",
    PhoneNumber = "0599123456",
    Address = "Jenin"
};

var result = validator.Validate(request);

Assert.True(result.IsValid);
```

### Empty Patient Name

An empty `FullName` should fail validation.

```csharp
Assert.Contains(
    result.Errors,
    error => error.PropertyName == "FullName"
);
```

### Future Date of Birth

A date of birth in the future should also fail validation.

```csharp
Assert.Contains(
    result.Errors,
    error => error.PropertyName == "DateOfBirth"
);
```

These tests allow the validation rules to be checked directly without starting the API or accessing the database.

---

# Applying Testing Priorities

At the end of Week 5, I reviewed the project and identified the areas that were more important to test first.

Instead of trying to test every method equally, I focused on logic with branching, dependencies, validation rules, and important API behavior.

The three main areas I prioritized were:

| Area | Reason |
| --- | --- |
| `PatientService.CalculateAge()` | Contains date calculation and branching logic |
| `PatientService.GetPatientNameAsync()` | Depends on `IPatientRepository` and contains success/failure paths |
| `CreatePatientValidator` | Prevents invalid patient information from reaching the API |

The first area is tested using xUnit `[Fact]` and `[Theory]` tests.

The second area is tested using Moq to control and verify the repository dependency.

The third area is tested directly through the FluentValidation validator.

This helped me understand that useful testing is not about reaching 100% coverage. The first priority should be the code where incorrect behavior could have a larger effect on the application.

---

# Week 5 Test Suite

By the end of Week 5, the automated test suite includes:

```text
Automated Tests
│
├── PatientService Unit Tests
│   ├── Fact tests
│   └── Theory tests
│
├── Moq Tests
│   ├── Repository success
│   ├── Repository failure
│   └── Verify repository call
│
├── Validation Tests
│   ├── Valid patient
│   ├── Empty full name
│   └── Future date of birth
│
└── Integration Tests
    ├── Existing patient → 200 OK
    └── Missing patient → 404 Not Found
```

The complete suite currently contains:

```text
Total tests: 13
Passed: 13
Failed: 0
Skipped: 0
```

This means all unit, Moq, validation, and integration tests currently pass together.

---

# Running the Project

## Requirements

- .NET 10 SDK
- SQL Server LocalDB
- Entity Framework Core CLI tools

## 1. Open the Project

From `Project1`:

```bash
cd CardiacPatientMonitoringSystem
```

## 2. Restore Packages

```bash
dotnet restore
```

## 3. Create / Update the Database

```bash
dotnet ef database update
```

## 4. Run the API

```bash
dotnet run
```

## 5. Open Swagger

In my current development configuration:

```text
http://localhost:5075/swagger
```

> The development port may change depending on the local launch configuration.

---

# Running the Automated Tests

From the `Project1` directory:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

The test project builds the required projects and runs the complete automated test suite.

The API does not need to be started manually before running these tests.

The current result is:

```text
Total tests: 13
Passed: 13
Failed: 0
Skipped: 0
```

---

# Database Configuration

The development database uses:

```text
(localdb)\MSSQLLocalDB
```

Database:

```text
CardiacPatientMonitoringDb
```

The integration tests use:

```text
CardiacPatientMonitoringTestDb
```

through the EF Core In-Memory provider instead of the development SQL Server database.

---

# Example API Flow

A normal authenticated API request follows:

```text
Client / Postman / Swagger
          ↓
Global Exception Middleware
          ↓
Request Logging Middleware
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

A Moq unit test follows:

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

An integration test follows:

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

---

# Screenshots and Testing Evidence

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

## Global Exception Handling

### Safe 500 ProblemDetails Response

![Global Exception Response](./screenshots/global-exception-500.png)

### Structured Server Log

![Global Exception Log](./screenshots/global-exception-log.png)

---

## Swagger

### API Documentation

![Swagger](./screenshots/swagger-api-overview.png)

---

# What I Learned

The biggest benefit of this project was moving from small separate exercises into one API where multiple backend concepts depend on each other.

I became more comfortable with:

- Designing entities and relationships.
- Using DTOs for API requests.
- Working with SQL Server through Entity Framework Core.
- Creating and applying migrations.
- Writing asynchronous CRUD operations.
- Using LINQ inside EF Core queries.
- Working with Identity and JWT authentication.
- Protecting API endpoints.
- Validating incoming requests.
- Returning meaningful HTTP status codes.
- Understanding the ASP.NET Core middleware pipeline.
- Testing APIs with Postman and Swagger.
- Creating a separate xUnit test project.
- Using `[Fact]`, `[Theory]`, and `[InlineData]`.
- Following Arrange-Act-Assert.
- Testing pure application logic.
- Mocking dependencies with Moq.
- Using `Setup()`, `ReturnsAsync()`, and `ThrowsAsync()`.
- Verifying dependency calls using `Verify()` and `Times.Once`.
- Understanding unit tests versus integration tests.
- Using `WebApplicationFactory`.
- Sending test HTTP requests using `HttpClient`.
- Using an isolated EF Core In-Memory database.
- Testing protected endpoints with a valid JWT.
- Testing FluentValidation rules directly.
- Prioritizing tests based on risk and complexity.
- Handling unexpected exceptions centrally.
- Returning standardized `ProblemDetails`.
- Logging server-side exception information with `ILogger`.

One of the most important lessons from Week 5 was that automated testing is not about writing a test for every line of code.

It is more useful to identify the parts of the application where a bug could have a larger impact and test those areas first.

For this project, that meant focusing on branching logic, dependency behavior, validation, and important API endpoints.

---

# Current Project Status

At the current stage, I have completed the main API structure, database integration, CRUD modules, authentication, validation, middleware, filtering, seed data, Swagger documentation, Postman verification, automated testing, and centralized exception handling.

The automated testing setup currently includes:

### xUnit

Tests for `CalculateAge()` using:

- `[Fact]`
- `[Theory]`
- `[InlineData]`

### Moq

Tests for `PatientService` with `IPatientRepository`, including:

- Controlled repository return data.
- Repository failure simulation.
- Repository call verification.

### Validation

Tests for `CreatePatientValidator`, including:

- Valid patient data.
- Empty patient name.
- Future date of birth.

### Integration Testing

Tests for:

```http
GET /api/patients/{id}
```

including:

- Existing patient → `200 OK`.
- Full patient response verification.
- Missing patient → `404 Not Found`.
- Valid JWT authentication.
- EF Core In-Memory test database.

### Global Error Handling

The project also includes centralized handling for unexpected exceptions.

The middleware:

- Catches unhandled exceptions.
- Logs the real exception with `ILogger`.
- Includes request method and path in the log.
- Returns `500 Internal Server Error`.
- Uses `ProblemDetails`.
- Does not expose exception messages or stack traces to the client.

### Current Test Result

```text
Total tests: 13
Passed: 13
Failed: 0
Skipped: 0
```

The complete test suite currently passes successfully.

The remaining training-aligned work will be added as the next topics are covered. After completing the remaining requirements, I will perform the final project cleanup and documentation review.

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
- ASP.NET Core ProblemDetails
- ILogger
- Swagger / OpenAPI
- Postman
- Git / GitHub

---

## Final Note

This project is being developed as an individual backend training project.

My goal is not only to make the endpoints work, but also to understand how the main parts of an ASP.NET Core backend application work together.

Week 5 added another important layer to the project: automated testing and centralized error handling.

Using xUnit helped me test application logic directly.

Using Moq helped me isolate services from their dependencies.

Using WebApplicationFactory helped me test the API through real HTTP requests inside an isolated environment.

Testing FluentValidation directly helped me verify important input rules without having to start the API.

The final Week 5 exercise also helped me understand that I should prioritize tests based on risk and important application behavior instead of simply trying to test everything.

The project currently has **13 automated tests, and all 13 are passing**.

Together, these additions make the project easier to verify as it continues to grow.