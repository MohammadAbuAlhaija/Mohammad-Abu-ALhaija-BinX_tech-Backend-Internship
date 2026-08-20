# Cardiac Patient Monitoring System — Demo Scenario

## Demo Purpose

This demo shows the current working state of my **Cardiac Patient Monitoring System** ASP.NET Core Web API.

The goal is to demonstrate how the main backend concepts used in the project work together, including:

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- CRUD operations
- ASP.NET Core Identity
- JWT Authentication
- Protected endpoints
- FluentValidation
- LINQ filtering
- Custom Middleware
- Centralized Exception Handling
- Swagger / OpenAPI
- Postman Testing
- Unit Testing with xUnit
- Dependency Mocking with Moq
- Integration Testing with WebApplicationFactory
- Validation Unit Testing

All patient data used in this project is synthetic and created only for development and testing purposes.

---

# 1. Start the Project

From the main API project folder:

```bash
cd CardiacPatientMonitoringSystem
dotnet run
```

After the API starts, the terminal displays the local development URL.

In my current configuration:

```text
http://localhost:5075
```

The port may change depending on the local development configuration.

### What I Explain

This is an ASP.NET Core Web API connected to SQL Server using Entity Framework Core.

The main resources are:

- Patients
- Vital Signs
- Medications
- Appointments

The project also includes authentication, validation, middleware, error handling, and automated testing.

---

# 2. Open Swagger

Open:

```text
http://localhost:5075/swagger
```

Swagger displays the available controllers and endpoints.

The main controllers are:

- Auth
- Patients
- Vital Signs
- Medications
- Appointments

### What I Explain

Swagger/OpenAPI gives me a visual way to inspect and manually test the API without building a frontend.

I also configured JWT Bearer support so protected endpoints can be tested after authentication.

---

# 3. Register a User

Use:

```http
POST /api/auth/register
```

Example:

```text
http://localhost:5075/api/auth/register?email=mohammad@example.com&password=Test123!
```

### Expected Result

```text
200 OK
```

### What I Explain

I used ASP.NET Core Identity to manage application users.

Identity handles user storage and password hashing instead of manually storing plain-text passwords.

---

# 4. Login and Generate JWT

Use:

```http
POST /api/auth/login
```

Example:

```text
http://localhost:5075/api/auth/login?email=mohammad@example.com&password=Test123!
```

### Expected Result

The API returns a JWT.

Example response:

```json
{
  "token": "...",
  "expiresInMinutes": 30
}
```

### What I Explain

After successful login, the API generates a JWT that can be used to access protected endpoints.

The token is signed using HMAC SHA-256.

JWT validation checks:

- Issuer
- Audience
- Lifetime
- Signing Key

The general authentication flow is:

```text
Register
   ↓
Login
   ↓
Generate JWT
   ↓
Send Bearer Token
   ↓
Validate JWT
   ↓
Access Protected Endpoint
```

---

# 5. Test a Protected Endpoint Without JWT

Send:

```http
GET /api/patients
```

without a Bearer token.

### Expected Result

```text
401 Unauthorized
```

### What I Explain

The main resource controllers are protected using:

```csharp
[Authorize]
```

Without a valid JWT, ASP.NET Core Authentication rejects the request before the protected action is accessed.

---

# 6. Test the Same Endpoint With JWT

Send:

```http
GET /api/patients
```

with:

```text
Authorization: Bearer <token>
```

### Expected Result

```text
200 OK
```

The API returns the patients.

### What I Explain

Authentication answers:

```text
Who is the user?
```

Authorization answers:

```text
Is the user allowed to access this endpoint?
```

A valid JWT authenticates the request and allows access to the protected endpoint.

---

# 7. Show the Patients CRUD API

The Patients module supports full CRUD operations:

```http
GET    /api/patients
GET    /api/patients/{id}
POST   /api/patients
PUT    /api/patients/{id}
DELETE /api/patients/{id}
```

## Example Create Patient

```http
POST /api/patients
```

Example body:

```json
{
  "fullName": "Ahmad Khalil",
  "dateOfBirth": "1985-06-15",
  "gender": "Male",
  "phoneNumber": "0599123456",
  "address": "Jenin"
}
```

### Expected Result

```text
201 Created
```

### What I Explain

I use separate request DTOs for create and update operations instead of directly accepting EF Core entities.

The controller maps the request data to the entity and uses Entity Framework Core to save it.

---

# 8. Show Async Entity Framework Core

The CRUD operations use asynchronous EF Core methods such as:

```csharp
await _context.Patients.ToListAsync();

await _context.Patients.FindAsync(id);

await _context.SaveChangesAsync();
```

### What I Explain

I used `async` and `await` for database operations so the request thread does not remain blocked while waiting for database work to complete.

---

# 9. Test 404 Not Found

Request a Patient ID that does not exist:

```http
GET /api/patients/99999
```

### Expected Result

```text
404 Not Found
```

Example:

```json
{
  "message": "Patient with ID 99999 was not found."
}
```

### What I Explain

The controller checks whether the requested resource exists.

If it does not exist, the API returns `404 Not Found` instead of returning a successful response.

---

# 10. Test FluentValidation

Send an invalid Patient request:

```http
POST /api/patients
```

Example:

```json
{
  "fullName": "",
  "dateOfBirth": "2030-01-01",
  "gender": "",
  "phoneNumber": "",
  "address": ""
}
```

### Expected Result

```text
400 Bad Request
```

### What I Explain

I used FluentValidation to keep validation rules separate from the controllers.

For example:

```csharp
RuleFor(x => x.FullName)
    .NotEmpty();

RuleFor(x => x.DateOfBirth)
    .NotEmpty()
    .LessThan(DateTime.Today);
```

The project contains Create and Update validators for:

- Patients
- Vital Signs
- Medications
- Appointments

FluentValidation is registered using automatic validation, so invalid requests can be rejected before the normal controller operation continues.

---

# 11. Show Vital Signs

The Vital Signs API supports:

```http
GET    /api/vitalsigns
GET    /api/vitalsigns/{id}
POST   /api/vitalsigns
PUT    /api/vitalsigns/{id}
DELETE /api/vitalsigns/{id}
```

A Vital Sign contains:

- Patient ID
- Heart Rate
- Systolic Blood Pressure
- Diastolic Blood Pressure
- Measurement Time

### What I Explain

Before creating or updating a Vital Sign, I check whether the referenced Patient exists.

Example:

```csharp
var patientExists = await _context.Patients
    .AnyAsync(p => p.Id == request.PatientId);
```

This prevents creating a record that references a Patient that does not exist.

---

# 12. Show Medications

The Medications API supports:

```http
GET    /api/medications
GET    /api/medications/{id}
POST   /api/medications
PUT    /api/medications/{id}
DELETE /api/medications/{id}
```

Each Medication contains:

- Patient ID
- Name
- Dosage
- Frequency
- Start Date
- Optional End Date

### What I Explain

Medication has a relationship with Patient through `PatientId`.

One Patient can have multiple Medication records.

The API also checks that the referenced Patient exists before creating or updating related data.

---

# 13. Show Appointments

The Appointments API supports:

```http
GET    /api/appointments
GET    /api/appointments/{id}
POST   /api/appointments
PUT    /api/appointments/{id}
DELETE /api/appointments/{id}
```

Each Appointment contains:

- Patient ID
- Appointment Date
- Doctor Name
- Reason
- Status

### What I Explain

Appointments are related to Patients using `PatientId`.

One Patient can have multiple Appointments.

---

# 14. Demonstrate LINQ Filtering

Use:

```http
GET /api/appointments?status=Scheduled
```

### Expected Result

Only appointments where:

```text
Status = Scheduled
```

are returned.

The controller uses:

```csharp
var query = _context.Appointments.AsQueryable();

if (!string.IsNullOrWhiteSpace(status))
{
    query = query.Where(a => a.Status == status);
}

var appointments = await query.ToListAsync();
```

### What I Explain

`AsQueryable()` allows me to build the query before executing it.

The `Where` condition is added only when a status is provided.

`ToListAsync()` executes the query and returns the results.

This demonstrates LINQ filtering with Entity Framework Core.

---

# 15. Show Entity Relationships

The main database relationships are:

```text
Patient
 ├── VitalSigns
 ├── Medications
 └── Appointments
```

Which means:

```text
Patient 1 ---- * VitalSign
Patient 1 ---- * Medication
Patient 1 ---- * Appointment
```

### What I Explain

`PatientId` works as the foreign key in the related entities.

These are one-to-many relationships because one Patient can have multiple Vital Signs, Medications, and Appointments.

---

# 16. Show EF Core Migrations

Show the `Migrations` folder.

The database work includes migrations for:

- Initial database creation
- ASP.NET Core Identity
- Synthetic seed data

The database can be updated using:

```bash
dotnet ef database update
```

### What I Explain

I used the EF Core Code-First approach.

The C# entity models define the application data structure, and migrations are used to apply database schema changes to SQL Server.

---

# 17. Show Synthetic Seed Data

Test:

```http
GET /api/patients/1001
```

using a valid JWT.

### Expected Result

```text
200 OK
```

The API returns the seeded Patient.

### What I Explain

I added synthetic seed data so the database contains example records after the migrations are applied.

The seed data includes:

- One Patient
- One Vital Sign
- One Medication
- One Appointment

No real patient information is used.

---

# 18. Show Request Logging Middleware

Send any request, for example:

```http
GET /api/patients
```

Then show the server output.

Example:

```text
Request: GET /api/patients
Response Status: 200
```

The middleware performs logic before and after:

```csharp
await _next(context);
```

### What I Explain

I created a custom `RequestLoggingMiddleware`.

Before `_next(context)`, it logs information about the incoming request.

`_next(context)` passes the request to the next component in the ASP.NET Core pipeline.

After the request finishes, execution returns to the middleware and it logs the response status code.

This helped me understand how the ASP.NET Core middleware pipeline works.

---

# 19. Show Global Exception Handling

The project also contains a custom:

```text
GlobalExceptionMiddleware
```

Its purpose is to handle unexpected exceptions in one central place.

The main idea is:

```csharp
try
{
    await _next(context);
}
catch (Exception ex)
{
    // Log the real exception
    // Return a safe response
}
```

### What I Explain

Instead of repeating `try/catch` logic in every controller, the middleware wraps the remaining request pipeline.

If an unhandled exception occurs later in the pipeline, it can catch the exception centrally.

The flow is:

```text
Request
   ↓
GlobalExceptionMiddleware
   ↓
try
   ↓
_next(context)
   ↓
Remaining Pipeline / Endpoint
   ↓
Unexpected Exception
   ↓
catch
   ↓
Log Error
   ↓
500 Internal Server Error
```

---

# 20. Show Safe ProblemDetails Response

For unexpected server errors, the middleware returns a safe `ProblemDetails` response.

Example:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "instance": "/api/patients/test-error"
}
```

### What I Explain

The `test-error` endpoint was created temporarily to test the exception middleware and was removed after testing.

The important point is that the client receives a safe generic response.

The API does not expose the real exception message or stack trace to the client.

---

# 21. Show Server Exception Logging

The real exception is logged on the server using `ILogger`.

Example logic:

```csharp
_logger.LogError(
    ex,
    "Unhandled exception occurred for request {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

### What I Explain

The client receives a safe error response, while the developer can still see the real exception details on the server for debugging.

```text
Client
→ Safe ProblemDetails

Server
→ Detailed Exception Log
```

---

# 22. Show the Postman Collection

Show:

```text
Postman/
Cardiac Patient Monitoring System.postman_collection.json
```

The collection contains requests used during development for:

- Authentication
- Protected endpoints
- Patients CRUD
- Vital Signs CRUD
- Medications CRUD
- Appointments CRUD
- Appointment filtering
- Validation errors
- 404 Not Found

### What I Explain

I used Postman throughout development for manual API testing.

After implementing each feature, I used the collection to verify the request, response, and HTTP status code.

---

# 23. Run the Automated Tests

From the `Project1` folder, run:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

### Expected Result

```text
Total:   13
Passed:  13
Failed:   0
Skipped:  0
```

### What I Explain

The project now includes automated tests in addition to the manual Swagger and Postman testing.

The automated test suite includes:

- xUnit Unit Tests
- Moq dependency tests
- FluentValidation Unit Tests
- Integration Tests using WebApplicationFactory

---

# 24. Show Unit Testing with xUnit

The first Unit Tests focus on:

```text
PatientService.CalculateAge()
```

The method accepts:

```text
Date of Birth
Reference Date
```

and calculates the patient's age.

### Test Scenarios

I tested situations such as:

- Birthday already passed
- Birthday has not happened yet
- Birthday is today

I used both:

```csharp
[Fact]
```

and:

```csharp
[Theory]
[InlineData(...)]
```

### What I Explain

`[Fact]` is useful for a specific fixed test scenario.

`[Theory]` allows the same test logic to run with multiple input values using `[InlineData]`.

The tests follow the:

```text
Arrange
   ↓
Act
   ↓
Assert
```

pattern.

The `CalculateAge` tests produce six test cases in total.

Using a supplied `referenceDate` also keeps the tests predictable instead of depending directly on the current system date.

---

# 25. Show Dependency Mocking with Moq

The project contains:

```text
IPatientRepository
```

with:

```csharp
Task<Patient?> GetByIdAsync(int id);
```

`PatientService` depends on this interface for the `GetPatientNameAsync` exercise.

### What I Explain

I used Moq to replace the repository dependency with a mock during the Unit Test.

This allows me to test `PatientService` without using a real database.

For a successful scenario, the mock can be configured using:

```text
Setup
   ↓
ReturnsAsync
```

For a failure scenario:

```text
Setup
   ↓
ThrowsAsync
```

I also use:

```text
Verify
   ↓
Times.Once
```

to verify that the expected repository method was called once.

### Important Note

The main CRUD controllers in this project still use `AppDbContext` directly.

`IPatientRepository` was introduced specifically while practicing dependency isolation and Moq. The project does not use a repository implementation as the architecture for all CRUD operations.

---

# 26. Show Integration Testing with WebApplicationFactory

The project contains automated Integration Tests using:

```text
WebApplicationFactory
HttpClient
EF Core InMemory
JWT
```

### What I Explain

`WebApplicationFactory` starts the ASP.NET Core application inside the test environment.

The test uses `HttpClient` to send an HTTP request instead of calling a controller method directly.

Because the Patient endpoints are protected, the Integration Tests use a valid JWT.

The test flow is:

```text
xUnit Test
    ↓
HttpClient
    ↓
ASP.NET Core Pipeline
    ↓
JWT Authentication
    ↓
Authorization
    ↓
PatientsController
    ↓
Entity Framework Core
    ↓
InMemory Database
    ↓
HTTP Response
    ↓
Assertions
```

---

# 27. Show the In-Memory Test Database

For Integration Tests, SQL Server is replaced with:

```text
EF Core InMemory
```

### What I Explain

I use an InMemory database so Integration Tests do not depend on or modify the normal SQL Server database.

The test database can be reset and recreated to provide a known test environment.

This database is used only for testing.

---

# 28. Show Patient Integration Tests

Two important endpoint scenarios are tested.

## Existing Patient

Request:

```http
GET /api/patients/1001
```

with a valid JWT.

### Expected

```text
200 OK
```

The test also verifies the returned Patient data.

## Missing Patient

Request:

```http
GET /api/patients/99999
```

with a valid JWT.

### Expected

```text
404 Not Found
```

### What I Explain

Unlike a Unit Test that tests one method directly, these tests send real HTTP requests through multiple parts of the application.

This verifies that the API components work together correctly.

---

# 29. Show FluentValidation Unit Tests

I also test:

```text
CreatePatientValidator
```

directly.

The current validation tests cover:

```text
Valid Patient Request
→ Valid

Empty FullName
→ Validation Error

Future DateOfBirth
→ Validation Error
```

### What I Explain

These are Unit Tests for the validator itself.

They do not require:

- HTTP requests
- Controllers
- JWT
- SQL Server

This allows the validation rules to be tested directly and automatically.

---

# 30. Explain the Testing Strategy

The automated tests focus on important behavior instead of trying to test every line.

Examples:

```text
CalculateAge
→ Date logic and branching

GetPatientNameAsync
→ Dependency behavior and failure handling

CreatePatientValidator
→ Input validation rules

Patient Integration Tests
→ HTTP endpoint behavior
```

### What I Explain

I focused on logic that contains decisions, external dependencies, validation rules, and important API behavior.

The current automated test suite is not intended to represent complete test coverage of every method in the application.

---

# 31. Show Testing Evidence

The project contains screenshots showing important manual test results, including examples such as:

```text
patients-get-all.png
patients-Delete.png

vitalsigns-get-all.png
vitalsigns-delete.png

medications-get-all.png
medications-delete.png

appointments-get-all.png
appointments-delete.png
appointments-filter-by-status.png

auth-register.png
auth-login-jwt.png
auth-protected-401.png
auth-protected-200.png

validation-patient-400.png
patient-not-found-404.png
seed-data-patient.png

swagger-api-overview.png
```

Additional evidence from the newer work demonstrates:

- Global 500 exception handling
- Server-side exception logging
- Successful automated test execution

### What I Explain

The screenshots provide evidence of the manual API scenarios, while `dotnet test` provides the automated testing result.

---

# 32. Explain HTTP Status Codes

The main HTTP status codes demonstrated in the project are:

| Status Code | Meaning | Example |
|---|---|---|
| `200 OK` | Successful request | GET Patient |
| `201 Created` | Resource created | POST Patient |
| `204 No Content` | Successful operation without response body | Update / Delete |
| `400 Bad Request` | Invalid input | FluentValidation |
| `401 Unauthorized` | Authentication required | Missing JWT |
| `404 Not Found` | Resource does not exist | Missing Patient |
| `500 Internal Server Error` | Unexpected server error | Global exception handler |

### What I Explain

The API returns an appropriate HTTP status depending on the result of the request.

---

# 33. Explain the Complete Request Flow

At the end of the API demonstration, I summarize the request flow:

```text
Postman / Swagger / Client
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

### What I Explain

When a request enters the application, it passes through the ASP.NET Core request pipeline.

The Global Exception Middleware can catch unexpected exceptions from later stages.

The Request Logging Middleware records request and response information.

If the endpoint is protected, JWT Authentication validates the token.

Authorization determines whether the authenticated user can access the protected endpoint.

The incoming request DTO is validated.

The Controller performs the requested operation.

Entity Framework Core communicates with SQL Server.

Finally, the API returns the appropriate HTTP response.

---

# 34. Explain the Automated Integration Test Flow

For Integration Testing, the flow is slightly different because the normal SQL Server database is replaced by an InMemory test database:

```text
xUnit
  ↓
WebApplicationFactory
  ↓
HttpClient
  ↓
ASP.NET Core Pipeline
  ↓
JWT Authentication
  ↓
Authorization
  ↓
PatientsController
  ↓
Entity Framework Core
  ↓
InMemory Test Database
  ↓
HTTP Response
  ↓
Assertions
```

### What I Explain

This allows me to automatically test an API endpoint through multiple layers without manually starting the API or modifying the normal development database.

---

# 35. Final Demo Summary

At its current stage, the project demonstrates:

- C#
- .NET 10
- ASP.NET Core Web API
- Controllers and Routing
- Dependency Injection
- Middleware
- Async / Await
- LINQ
- DTOs
- Entity Framework Core
- SQL Server LocalDB
- Code-First Migrations
- Entity Relationships
- Synthetic Seed Data
- CRUD Operations
- ASP.NET Core Identity
- Registration and Login
- JWT Authentication
- Protected API Routes
- FluentValidation
- HTTP Status Codes
- LINQ Filtering
- Request Logging Middleware
- Centralized Exception Handling
- ProblemDetails
- ILogger
- Swagger / OpenAPI
- Postman
- xUnit
- Moq
- WebApplicationFactory
- HttpClient
- EF Core InMemory
- Unit Testing
- Validation Testing
- Integration Testing

---

# Current Automated Test Status

```text
Total Tests: 13
Passed:      13
Failed:       0
Skipped:      0
```

The automated tests currently cover selected important scenarios rather than every line of the application.

---

# Short Closing Explanation

This project helped me connect the backend concepts I learned during the training inside one ASP.NET Core application.

I started with the database models and CRUD operations, then added authentication, JWT protection, validation, filtering, middleware, and centralized exception handling.

After that, I moved from only manual API testing with Swagger and Postman to automated testing using xUnit, Moq, FluentValidation tests, and Integration Testing with WebApplicationFactory.

The project now demonstrates the complete path from receiving and validating an HTTP request, authenticating the user, accessing data through Entity Framework Core, handling errors safely, returning the appropriate HTTP response, and automatically testing important application behavior.