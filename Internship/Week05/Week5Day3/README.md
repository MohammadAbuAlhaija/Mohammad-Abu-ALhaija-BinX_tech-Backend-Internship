# Week 5 - Day 3: Integration Testing with WebApplicationFactory

## Overview

Today I moved from unit testing individual pieces of the application to integration testing the API as a whole.

The main goal was to use `WebApplicationFactory` to run the Cardiac Patient Monitoring System in a test environment and send real HTTP requests to the API without starting the application manually.

This helped verify that different parts of the application work correctly together, including routing, authentication, controllers, and the database layer.

---

## WebApplicationFactory Setup

I created a custom `WebApplicationFactory<Program>` for the integration tests.

Instead of connecting the tests to the real SQL Server database, I configured the factory to use an isolated EF Core In-Memory database.

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(
        "CardiacPatientMonitoringTestDb"
    );
});
```

This keeps integration tests separate from the real application database and prevents test data from affecting development data.

The test database is recreated before the integration tests run:

```csharp
context.Database.EnsureDeleted();
context.Database.EnsureCreated();
```

---

## Testing a Real API Endpoint

I added integration tests for the Patients API using an `HttpClient` created by `WebApplicationFactory`.

The happy-path test sends a request to:

```http
GET /api/patients/1001
```

The test verifies that the endpoint returns `200 OK` and checks the returned patient's information, including:

- ID
- Full name
- Date of birth
- Gender
- Phone number
- Address

This tests more than just a single method because the request goes through the actual ASP.NET Core request pipeline.

---

## Testing the Not Found Case

I also tested the same endpoint with a patient ID that does not exist:

```http
GET /api/patients/99999
```

The expected result is:

```text
404 Not Found
```

This verifies that the API handles missing resources correctly during an integration test.

---

## Testing Authentication

The `PatientsController` is protected with:

```csharp
[Authorize]
```

Because of this, the integration tests generate a valid test JWT and attach it to the HTTP request:

```csharp
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

The token uses the same issuer, audience, and signing key expected by the API.

This allowed the integration test to verify a protected endpoint while still passing through the real JWT authentication middleware.

---

## Final Test Result

After completing the integration tests, I ran:

```powershell
dotnet test CardiacPatientMonitoringSystem.Tests/CardiacPatientMonitoringSystem.Tests.csproj
```

All tests passed successfully:

```text
Total tests: 10
Passed: 10
Failed: 0
```

The test suite now includes the previous xUnit and Moq tests together with the new API integration tests.

### Test Result

![Integration Tests Passed](Screenshots/integration-tests-passed.png)

---

## What I Learned

Today helped me understand the difference between unit tests and integration tests more clearly.

With unit testing, I was testing a specific method or service in isolation. With `WebApplicationFactory`, I was able to send HTTP requests through the application and test how multiple parts work together.

I also learned why integration tests should use a separate test database instead of the application's real database, and how authenticated endpoints can be tested by attaching a valid JWT.

Overall, this gave me a more realistic way to verify that the API behaves correctly from the client's point of view.