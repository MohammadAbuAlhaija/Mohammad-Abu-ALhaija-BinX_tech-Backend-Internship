# Week 5 - Automated Testing & Error Handling

## Overview

Week 5 was mainly focused on making my **Cardiac Patient Monitoring System** more reliable through automated testing and centralized error handling.

Instead of only testing the API manually with Swagger and Postman, I started building an automated test suite that can verify important parts of the application whenever the code changes.

Throughout the week, I worked with **xUnit**, **Moq**, **WebApplicationFactory**, an **EF Core In-Memory database**, and ASP.NET Core middleware.

By the end of the week, the project had **13 passing automated tests** covering unit, mocked dependency, validation, and integration scenarios.

---

# Day 1 - Unit Testing with xUnit

I started the week by setting up a separate xUnit test project:

```text
CardiacPatientMonitoringSystem.Tests
```

The test project references the main API project so I can test application logic without mixing test code with the API itself.

For the first tests, I created a simple `PatientService` method:

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

I used a `referenceDate` instead of `DateTime.Today` so the tests stay predictable.

I practiced both:

```text
[Fact]
[Theory]
```

The tests covered different birthday scenarios and followed the **Arrange - Act - Assert** pattern.

At the end of Day 1, the test suite contained **6 passing test cases**.

### Day 1 Test Result

![xUnit Tests Passed](./Week5Day1/Screenshots/xunit-tests-passed.png)

---

# Day 2 - Mocking Dependencies with Moq

On Day 2, I moved from testing pure logic to testing a service that depends on another component.

I created:

```text
IPatientRepository
```

with:

```csharp
Task<Patient?> GetByIdAsync(int id);
```

`PatientService` receives this dependency through constructor injection.

Instead of connecting to a real database during the unit test, I used **Moq** to create a controlled version of the repository.

Example:

```csharp
var mockRepo = new Mock<IPatientRepository>();

mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(patient);
```

I tested the successful scenario and also simulated a repository failure using:

```csharp
.ThrowsAsync(new Exception("Database error"));
```

I also used:

```csharp
mockRepo.Verify(
    r => r.GetByIdAsync(1),
    Times.Once
);
```

to confirm that the expected repository method was called exactly once.

At the end of Day 2, the complete suite contained **8 passing tests**.

### Day 2 Test Result

![Moq Tests Passed](./Week5Day2/Screenshots/moq-tests-passed.png)

---

# Day 3 - Integration Testing with WebApplicationFactory

Day 3 moved testing from individual methods to the complete HTTP request flow.

I used:

```text
Microsoft.AspNetCore.Mvc.Testing
WebApplicationFactory
HttpClient
```

to run the API inside the test environment.

I created:

```text
CustomWebApplicationFactory.cs
PatientsApiTests.cs
```

The integration tests cover the important endpoint:

```http
GET /api/patients/{id}
```

Two main scenarios were tested:

```text
Existing patient → 200 OK
Missing patient  → 404 Not Found
```

For the successful request, I also verify the returned patient data instead of checking only the status code.

Because `PatientsController` is protected with `[Authorize]`, the integration tests generate and send a valid test JWT.

The request therefore goes through a realistic application flow:

```text
Integration Test
      ↓
HttpClient
      ↓
ASP.NET Core Pipeline
      ↓
JWT Authentication
      ↓
PatientsController
      ↓
Entity Framework Core
      ↓
Test Database
      ↓
HTTP Response
```

## Isolated Test Database

The integration tests do not use the normal SQL Server development database.

I configured an **EF Core In-Memory database** specifically for the test environment.

This keeps automated tests isolated and prevents them from changing the development database.

At the end of Day 3, the complete test suite contained **10 passing tests**.

### Day 3 Test Result

![Integration Tests Passed](./Week5Day3/Screenshots/integration-tests-passed.png)

---

# Day 4 - Global Exception Handling

On Day 4, I focused on handling unexpected application errors in one central place.

Instead of repeating `try/catch` blocks inside different API endpoints, I created:

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
    // Log the real exception
    // Return a safe response
}
```

For unexpected errors, the API returns a standardized **ProblemDetails** response.

Example:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "instance": "/api/patients/test-error"
}
```

The actual exception message and stack trace are not returned to the client.

## Structured Logging

I used `ILogger` to keep the complete exception information on the server:

```csharp
_logger.LogError(
    ex,
    "Unhandled exception occurred for request {Method} {Path}",
    context.Request.Method,
    context.Request.Path
);
```

This gives useful debugging information while keeping the client response safe.

I temporarily created an endpoint that deliberately throws an exception to verify the middleware.

After confirming that the client received a safe `500 Internal Server Error` response and the complete exception was logged on the server, I removed the temporary endpoint.

### Safe ProblemDetails Response

![Global Exception Response](./Week5Day4/Screenshots/global-exception-500.png)

### Server-Side Exception Log

![Global Exception Log](./Week5Day4/Screenshots/global-exception-log.png)

After the changes, I ran the existing automated tests again:

```text
Total tests: 10
Passed: 10
Failed: 0
```

---

# Day 5 - Applying Testing to the Project

The final day focused on applying everything from Week 5 to the actual capstone project and deciding what should be tested first.

The main lesson was that testing should be prioritized based on **risk and complexity**, not simply by trying to test every line of code.

For the Cardiac Patient Monitoring System, I identified three important areas:

| Area | Why It Matters |
| --- | --- |
| `CalculateAge()` | Contains branching and date calculation logic |
| `GetPatientNameAsync()` | Depends on a repository and contains success/failure paths |
| `CreatePatientValidator` | Prevents invalid patient information from entering the API |

The first two were already covered by the xUnit and Moq work from earlier in the week.

For the third area, I added new unit tests for:

```text
Valid patient data
Empty patient name
Future date of birth
```

After adding these tests, I ran the complete test suite again:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

Final result:

```text
Total tests: 13
Passed: 13
Failed: 0
Skipped: 0
```

The existing integration tests also satisfy the requirement of testing the project's important Patient endpoint through both successful and not-found scenarios.

---

# Week 5 Test Suite

By the end of the week, the project contains several types of automated tests.

```text
Automated Test Suite
│
├── Unit Tests
│   └── PatientService.CalculateAge()
│
├── Moq Tests
│   └── PatientService + IPatientRepository
│
├── Validation Tests
│   └── CreatePatientValidator
│
└── Integration Tests
    └── GET /api/patients/{id}
        ├── 200 OK
        └── 404 Not Found
```

The tests cover different levels of the application instead of testing everything in the same way.

---

# Error Handling Setup

The project now also has centralized handling for unexpected exceptions.

```text
HTTP Request
     ↓
GlobalExceptionMiddleware
     ↓
Request Logging Middleware
     ↓
Authentication / Authorization
     ↓
Validation
     ↓
Controller
     ↓
Entity Framework Core
```

If an unexpected exception occurs later in the request pipeline, it can return to the global handler.

The middleware:

- Logs the real exception using `ILogger`.
- Includes useful request context in the server log.
- Returns `500 Internal Server Error`.
- Uses `ProblemDetails`.
- Does not expose the real exception message or stack trace to the client.

Expected API responses such as validation errors and `404 Not Found` continue to be handled normally.

---

# Final Week 5 Result

At the end of Week 5:

```text
Test Summary

Total:   13
Passed:  13
Failed:  0
Skipped: 0
```

The project now has a testing foundation that includes:

- Unit testing with xUnit
- `[Fact]` and `[Theory]`
- Arrange-Act-Assert
- Dependency mocking with Moq
- Mock setup and verification
- Integration testing with WebApplicationFactory
- HttpClient-based API testing
- EF Core In-Memory testing
- Protected endpoint testing with JWT
- FluentValidation unit testing
- Centralized exception handling
- ProblemDetails
- Structured logging with ILogger

---

# What I Learned This Week

Week 5 changed the way I think about testing.

Before this week, most of my API verification was done manually using Postman and Swagger. These tools are still useful, but automated tests provide another level of confidence because I can run the complete suite again whenever the project changes.

I learned that different tests have different purposes.

**Unit tests** are useful for testing application logic in isolation.

**Moq** allows dependencies to be replaced with controlled behavior so that a service can be tested without connecting to the real dependency.

**Integration tests** verify that multiple parts of the application work together through real HTTP requests.

I also learned that good testing does not mean trying to reach 100% coverage at any cost. It is more useful to focus first on logic with real branching, important failure scenarios, authentication, validation, and other areas where a bug could have a larger impact.

Finally, centralized exception handling showed me how middleware can solve an application-wide problem without repeating the same error-handling code in every controller.

---

# Moving Forward

Week 5 created the testing and error-handling foundation for the next phase of the project.

As new endpoints and features are added during the upcoming sprints, the goal is to continue using the same approach:

```text
Build the feature
       ↓
Identify important risks
       ↓
Add meaningful tests
       ↓
Run the full test suite
       ↓
Confirm existing behavior still works
```

This should make it easier to catch problems earlier instead of discovering them near the end of the project.

---

## Tools Used

- ASP.NET Core Web API
- xUnit
- Moq
- WebApplicationFactory
- HttpClient
- Entity Framework Core In-Memory
- JWT Authentication
- FluentValidation
- ProblemDetails
- ILogger
- Postman
- Git / GitHub