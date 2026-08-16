# Week 5 - Day 1: Project Selection & Unit Testing with xUnit

## Overview

Today I started Phase 3 by continuing with my existing **Cardiac Patient Monitoring System** project.

Instead of starting a new capstone project, I will continue improving this project throughout Phase 3 and apply the new testing concepts from the training to the existing API.

The main focus today was learning the basics of **unit testing with xUnit**, including `[Fact]`, `[Theory]`, and the **Arrange-Act-Assert** pattern.

---

## Project Selection

For my Phase 3 capstone, I will continue working on:

**Cardiac Patient Monitoring System**

The project already includes the main backend foundations from the previous weeks, such as:

* ASP.NET Core Web API
* Entity Framework Core with SQL Server
* Async CRUD operations
* JWT authentication
* FluentValidation
* Swagger and Postman testing

The scope is focused enough to continue adding the remaining training topics without rebuilding the project from scratch.

---

## Setting Up xUnit

I created a separate xUnit test project:

```text
CardiacPatientMonitoringSystem.Tests
```

Then I added a project reference to the main API project so the tests can access and test its classes.

I verified the setup using:

```powershell
dotnet test CardiacPatientMonitoringSystem.Tests
```

---

## First Unit Tests

To practice testing a simple method without database or external dependencies, I added a small `PatientService` with a `CalculateAge()` method.

The method calculates a patient's age using their date of birth and a reference date.

I wrote **3 `[Fact]` tests** covering:

* Birthday already passed this year
* Birthday has not occurred yet
* Birthday is today

Each test follows the **Arrange-Act-Assert** pattern:

* **Arrange** — prepare the service and test data.
* **Act** — call `CalculateAge()`.
* **Assert** — verify that the returned age is correct.

---

## Testing Multiple Cases with Theory

I also created a `[Theory]` test using multiple `[InlineData]` values.

This allowed the same test method to run against **3 different input cases** without creating separate test methods for each one.

After running the test project, all test cases passed successfully.

![xUnit Tests Passed](screenshots/xunit-tests-passed.png)

---

## What I Learned

Today I learned the basic difference between `[Fact]` and `[Theory]` in xUnit.

I also learned how the **Arrange-Act-Assert** pattern makes unit tests easier to read and understand, and how unit tests can verify a small piece of application logic without running the API or sending requests through Postman.
