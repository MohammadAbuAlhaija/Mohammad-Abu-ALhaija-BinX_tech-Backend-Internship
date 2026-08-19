# Week 5 - Day 5: Applying Testing & Week 5 Synthesis

## Overview

Today I wrapped up Week 5 by applying the testing concepts from this week to my **Cardiac Patient Monitoring System**.

The main focus was not to add a large number of tests, but to identify the parts of the project that are more important or more likely to fail and make sure they are covered properly.

---

## Testing Priorities

I focused on three important areas of the project:

- `PatientService.CalculateAge()` — contains branching logic based on the patient's date of birth.
- `PatientService.GetPatientNameAsync()` — depends on a repository and includes success and failure scenarios.
- `CreatePatientValidator` — protects the API from invalid patient data.

The first two areas were already covered by the xUnit and Moq tests created earlier this week.

Today I added unit tests for `CreatePatientValidator`.

The new tests cover:

- Valid patient data.
- Empty patient name.
- Date of birth in the future.

---

## Integration Testing

The project already contains two integration tests for the important endpoint:

```http
GET /api/patients/{id}
```

The tests verify:

- An existing patient returns `200 OK` with the expected patient data.
- A missing patient returns `404 Not Found`.

The tests also use a valid test JWT because the Patients API is protected.

---

## Full Test Suite

After adding the new validation tests, I ran:

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

This confirmed that the existing unit tests, Moq tests, integration tests, and the new validator tests all pass together.

---

## Week 5 Wrap-Up

During Week 5, I practiced different levels of automated testing:

- Unit testing with xUnit.
- Mocking dependencies with Moq.
- Integration testing with WebApplicationFactory.
- Testing with an isolated EF Core In-Memory database.
- Testing protected endpoints using JWT.
- Centralized exception handling with `ProblemDetails` and `ILogger`.
- Choosing tests based on risk and important application logic.

The main lesson from this week was that good testing is not about testing every line of code. It is more useful to focus on important logic, failure scenarios, and parts of the application where a bug could have a larger impact.

Week 5 gave me a testing and error-handling foundation that I can continue using during the upcoming project sprints.