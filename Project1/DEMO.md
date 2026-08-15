# Cardiac Patient Monitoring System — Demo Scenario

## Demo Purpose

This demo shows the current working state of my **Cardiac Patient Monitoring System** ASP.NET Core Web API.

The goal is to demonstrate how the API starts, how users authenticate, how protected endpoints work, how CRUD operations are tested, how validation is handled, and how the project connects to SQL Server through Entity Framework Core.

At the current stage, the demo covers the features I have already implemented and tested.

---

# 1. Start the Project

I start from the project folder:

```bash
cd CardiacPatientMonitoringSystem
dotnet run
```

After the API starts successfully, the terminal shows the local development URL.

In my current configuration, the API runs on:

```text
http://localhost:5075
```

### What I Explain

At this point, I explain that this is an ASP.NET Core Web API project connected to SQL Server using Entity Framework Core.

I also mention that the project includes:

- Patients
- Vital Signs
- Medications
- Appointments
- User authentication
- JWT protection
- Validation
- Middleware
- Swagger
- Postman testing

---

# 2. Open Swagger

I open:

```text
http://localhost:5075/swagger
```

Swagger displays the API controllers and endpoints.

### What I Explain

I explain that Swagger gives me a visual way to inspect and test the API without building a frontend application.

The main controllers currently available are:

- Auth
- Patients
- Vital Signs
- Medications
- Appointments

I also explain that Swagger supports JWT Bearer authentication in this project.

---

# 3. Register a User

I use the register endpoint:

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

I explain that I used ASP.NET Core Identity to manage application users.

Identity handles user storage and password hashing instead of storing passwords manually.

---

# 4. Login and Generate JWT

Next, I use:

```http
POST /api/auth/login
```

Example:

```text
http://localhost:5075/api/auth/login?email=mohammad@example.com&password=Test123!
```

### Expected Result

The API returns a JWT token.

Example response structure:

```json
{
  "token": "...",
  "expiresInMinutes": 30
}
```

### What I Explain

I explain that after successful login, the API generates a JWT.

The token contains information about the authenticated user and is signed so the API can verify it later.

The token validation checks:

- Issuer
- Audience
- Lifetime
- Signing Key

---

# 5. Test a Protected Endpoint Without JWT

I use:

```http
GET /api/patients
```

without sending a Bearer token.

### Expected Result

```text
401 Unauthorized
```

### What I Explain

I explain that the Patients controller is protected with:

```csharp
[Authorize]
```

Because the request does not contain a valid JWT, ASP.NET Core authentication rejects it before the protected action can be accessed.

---

# 6. Test the Same Endpoint With JWT

I send the same request again:

```http
GET /api/patients
```

but this time I add the JWT using:

```text
Authorization: Bearer <token>
```

### Expected Result

```text
200 OK
```

and the API returns the patients.

### What I Explain

I explain the difference between authentication and authorization.

Authentication confirms the identity of the user.

Authorization controls access to protected parts of the API.

---

# 7. Show the Patients CRUD API

The Patients module supports full CRUD operations.

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

I explain that I use request DTOs for create and update operations instead of directly accepting the EF Core entity.

The controller maps the request DTO to the entity and then uses Entity Framework Core to save it.

---

# 8. Show Async Entity Framework Core

Inside the CRUD operations, I use asynchronous EF Core methods such as:

```csharp
await _context.Patients.ToListAsync();
await _context.Patients.FindAsync(id);
await _context.SaveChangesAsync();
```

### What I Explain

I explain that I used `async` and `await` so database operations do not block the request thread while waiting for database work to complete.

---

# 9. Test 404 Not Found

I request a Patient ID that does not exist.

```http
GET /api/patients/99999
```

### Expected Result

```text
404 Not Found
```

Example response:

```json
{
  "message": "Patient with ID 99999 was not found."
}
```

### What I Explain

I explain that the controller checks whether the requested resource exists.

If it does not exist, the API returns `404 Not Found` instead of returning a successful response.

---

# 10. Test FluentValidation

I send an invalid Patient request.

```http
POST /api/patients
```

Example invalid body:

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

I explain that I used FluentValidation to keep validation rules outside the controller.

For example:

```csharp
RuleFor(x => x.FullName)
    .NotEmpty();

RuleFor(x => x.DateOfBirth)
    .LessThan(DateTime.Today);
```

This keeps the controller cleaner and allows invalid requests to be rejected before performing database operations.

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

Before creating or updating a Vital Sign, I check whether the patient exists.

Example:

```csharp
var patientExists = await _context.Patients
    .AnyAsync(p => p.Id == request.PatientId);
```

This prevents creating records that reference a patient that does not exist.

---

# 12. Show Medications

The Medications module supports:

```http
GET    /api/medications
GET    /api/medications/{id}
POST   /api/medications
PUT    /api/medications/{id}
DELETE /api/medications/{id}
```

Each medication contains:

- Patient ID
- Name
- Dosage
- Frequency
- Start Date
- Optional End Date

### What I Explain

I explain that Medication has a relationship with Patient through `PatientId`.

A patient can have multiple medication records.

---

# 13. Show Appointments

The Appointments module supports:

```http
GET    /api/appointments
GET    /api/appointments/{id}
POST   /api/appointments
PUT    /api/appointments/{id}
DELETE /api/appointments/{id}
```

Each appointment contains:

- Patient ID
- Appointment Date
- Doctor Name
- Reason
- Status

---

# 14. Demonstrate LINQ Filtering

I use the following request:

```http
GET /api/appointments?status=Scheduled
```

### Expected Result

Only appointments with:

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

I explain that this demonstrates LINQ filtering with Entity Framework Core.

The LINQ query is translated into a database query and executed by SQL Server.

---

# 15. Show Entity Relationships

The main database relationship is:

```text
Patient
 ├── VitalSigns
 ├── Medications
 └── Appointments
```

This means:

```text
Patient 1 ---- * VitalSign
Patient 1 ---- * Medication
Patient 1 ---- * Appointment
```

### What I Explain

I explain that `PatientId` works as the foreign key in the related entities.

This is a one-to-many relationship because one patient can have many related records.

---

# 16. Show EF Core Migrations

I show the `Migrations` folder.

The current database work includes migrations for:

- Initial database creation
- ASP.NET Core Identity
- Synthetic seed data

The database can be created using:

```bash
dotnet ef database update
```

### What I Explain

I explain that I used the EF Core Code-First approach.

The C# entities define the structure, and migrations are used to apply that structure to SQL Server.

---

# 17. Show Seed Data

I test:

```http
GET /api/patients/1001
```

with a valid JWT.

### Expected Result

```text
200 OK
```

The API returns the seeded Patient.

### What I Explain

I explain that I added synthetic seed data so the database contains example records immediately after the migrations are applied.

The seed data includes:

- Patient
- Vital Sign
- Medication
- Appointment

No real patient data is used.

---

# 18. Show Custom Middleware

I send any API request, for example:

```http
GET /api/patients
```

and then show the terminal output.

Example:

```text
Request: GET /api/patients
Response Status: 200
```

### What I Explain

I explain that I created a custom request logging middleware.

The middleware performs logic before and after the next component in the ASP.NET Core request pipeline.

Example:

```csharp
Console.WriteLine(
    $"Request: {context.Request.Method} {context.Request.Path}"
);

await _next(context);

Console.WriteLine(
    $"Response Status: {context.Response.StatusCode}"
);
```

This helped me understand the ASP.NET Core middleware pipeline more clearly.

---

# 19. Show Postman Collection

I show the exported Postman collection:

```text
Postman/
Cardiac Patient Monitoring System.postman_collection.json
```

The collection contains the API requests I used during development.

It includes tests for:

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

I explain that I used Postman throughout development to test each feature immediately after implementing it.

---

# 20. Show Testing Evidence

I show the `screenshots` folder.

The current testing evidence includes:

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

These screenshots show the results of the main API scenarios I tested.

---

# 21. Explain the Complete Request Flow

At the end of the demo, I summarize the request flow.

```text
Postman / Swagger
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

### What I Explain

When a request enters the application, it passes through the ASP.NET Core pipeline.

The middleware can inspect the request.

If the endpoint is protected, the JWT authentication system verifies the token.

The request DTO is validated.

The controller then performs the required operation using Entity Framework Core.

EF Core communicates with SQL Server.

Finally, the API returns the correct HTTP response.

---

# Demo Summary

At the current stage, the project demonstrates:

- ASP.NET Core Web API
- Controllers and Routing
- Dependency Injection
- Custom Middleware
- DTOs
- Async/Await
- LINQ
- Entity Framework Core
- SQL Server
- Code-First Migrations
- Entity Relationships
- Synthetic Seed Data
- CRUD Operations
- ASP.NET Core Identity
- Registration and Login
- JWT Authentication
- Protected Routes
- FluentValidation
- HTTP Status Codes
- Appointment Filtering
- Swagger / OpenAPI
- Postman Testing

---

# Current Remaining Work

The following project requirements have not been implemented yet because they belong to the next training topics:

- Centralized Error Handling
- xUnit Testing
- Moq

After implementing these parts, I will update this demo scenario with:

- Controlled exception-handling demonstrations
- Automated test execution
- Success and failure test scenarios

---

# Short Closing Explanation

This project helped me understand how the backend concepts I learned separately can work together inside one ASP.NET Core application.

Instead of only writing isolated examples, I practiced the complete flow from receiving an HTTP request, validating and authenticating it, accessing SQL Server through Entity Framework Core, and returning an appropriate HTTP response.

The API can currently be demonstrated independently using Swagger and Postman.