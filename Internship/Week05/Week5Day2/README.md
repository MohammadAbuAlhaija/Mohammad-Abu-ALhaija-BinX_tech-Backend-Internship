# Week 5 - Day 2: Mocking Dependencies with Moq

## Overview

Today I continued working on the **Cardiac Patient Monitoring System** and built on the xUnit testing work from Day 1.

The main focus was learning how to use **Moq** to test a service that depends on another component without using a real database.

Instead of testing the real dependency, I used a mock repository that I could control directly inside the tests.

---

## Patient Repository Interface

I added an `IPatientRepository` interface with a simple asynchronous method:

```csharp
Task<Patient?> GetByIdAsync(int id);
```

This gave `PatientService` a dependency that could be replaced with a mock during unit testing.

---

## Updating PatientService

I updated `PatientService` to receive `IPatientRepository` through its constructor.

```csharp
private readonly IPatientRepository _patientRepository;

public PatientService(IPatientRepository patientRepository)
{
    _patientRepository = patientRepository;
}
```

I also added `GetPatientNameAsync()`, which retrieves a patient through the repository and returns the patient's name.

The existing `CalculateAge()` method and its xUnit tests from Day 1 were kept.

---

## Mocking a Return Value

Using Moq, I created a mock implementation of `IPatientRepository`.

```csharp
var mockRepo = new Mock<IPatientRepository>();
```

I used `Setup()` and `ReturnsAsync()` to control what the repository returns:

```csharp
mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(patient);
```

This allowed me to test `PatientService` without connecting to the real database.

---

## Mocking an Exception

I also tested what happens when the repository fails.

Using `ThrowsAsync()`, I configured the mock repository to throw an exception:

```csharp
mockRepo
    .Setup(r => r.GetByIdAsync(1))
    .ThrowsAsync(new Exception("Database error"));
```

The service handles this scenario and returns a controlled error message.

This made it possible to test a failure scenario without intentionally causing a real database failure.

---

## Verifying Mock Interactions

I used Moq's `Verify()` to make sure the repository method was actually called by the service.

```csharp
mockRepo.Verify(
    r => r.GetByIdAsync(1),
    Times.Once
);
```

This verifies that `GetByIdAsync(1)` was called exactly once during the test.

---

## Running the Tests

I ran the automated tests using:

```bash
dotnet test CardiacPatientMonitoringSystem.Tests
```

The existing xUnit tests and the new Moq tests passed successfully.

### Moq Test Results

![Moq Tests Passed](Screenshots/moq-tests-passed.png)

---

## What I Learned

Today I learned why unit tests should isolate the code being tested from dependencies such as repositories and databases.

I practiced creating mocks with Moq, controlling return values with `Setup()` and `ReturnsAsync()`, simulating failures with `ThrowsAsync()`, and checking dependency interactions with `Verify()` and `Times.Once`.

The main idea I learned is that a mock lets me test the behavior of the service itself without depending on a real database.
