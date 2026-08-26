# Week 6 - Day 4: Write Operations, Business Logic & Transactions

## Overview

Today I continued developing **Version 2 of the Cardiac Patient Monitoring System**, with the main focus on moving beyond simple CRUD operations and implementing a write operation that contains real business logic.

Instead of using the generic order and stock example from the lesson, I adapted the same concepts to the medical domain of my project.

I implemented a **Patient Visit** operation that records both a medical record and the patient's vital signs as part of one operation. I also used an EF Core transaction to keep the database consistent if something fails during the process.

The work was implemented on a separate Git branch:

```text
feature/patient-visit-business-logic
```

---

## Patient Visit Business Operation

The main feature implemented today was a new patient visit operation.

A visit contains information needed for both the patient's medical record and the vital signs measured during the visit.

I created a dedicated request DTO:

```text
CreatePatientVisitRequest
```

It contains:

- Patient ID
- Doctor ID
- Diagnosis
- Notes
- Heart rate
- Systolic blood pressure
- Diastolic blood pressure
- Measurement date

This keeps the API request focused on the business operation rather than exposing the database entities directly.

---

## Input Validation

I added `CreatePatientVisitValidator` using FluentValidation.

The validator checks basic input requirements such as:

- Patient ID must be greater than zero
- Doctor ID must be greater than zero
- Diagnosis is required
- Heart rate must be greater than zero
- Blood pressure values must be greater than zero
- Measurement date is required

The more contextual checks were kept inside the service as business rules rather than simple request validation.

---

## Moving Business Logic into a Service

Instead of putting all the logic directly inside the controller, I created:

```text
PatientVisitService
```

The service is responsible for the complete patient visit workflow.

Before writing anything to the database, it verifies that:

1. The patient exists.
2. The doctor exists.
3. The vital-sign measurement is not dated in the future.

The general flow is:

```text
Create Patient Visit
        |
        v
Check Patient
        |
        v
Check Doctor
        |
        v
Check Business Rules
        |
        v
Begin Transaction
        |
        +---- Create MedicalRecord
        |
        +---- Create VitalSign
        |
        v
Save Changes
        |
        v
Commit
```

If an exception happens while processing the database operation, the transaction is rolled back instead of leaving the operation partially completed.

---

## Database Transaction

The visit performs more than one database write.

A single API request creates:

```text
MedicalRecords
VitalSigns
```

I wrapped these changes in an explicit EF Core transaction:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

try
{
    _context.MedicalRecords.Add(medicalRecord);
    _context.VitalSigns.Add(vitalSign);

    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

This introduced an explicit transaction boundary around the business operation and keeps the two related writes together.

---

## Patient Visits Controller

I added:

```text
PatientVisitsController
```

The controller stays intentionally small.

Its responsibility is mainly to receive the HTTP request and pass it to `PatientVisitService`, while the actual business logic remains in the service layer.

The new endpoint is:

```http
POST /api/patientvisits
```

This helped separate HTTP concerns from the application's business logic.

---

## Successful Patient Visit Test

I tested the new endpoint in Postman using an existing patient and doctor.

The request successfully passed the business rules and returned:

```text
200 OK
```

![Successful Patient Visit](./Screenshots/week6-day4-patient-visit-success.png)

This confirmed that the new patient visit endpoint could execute the complete operation successfully.

---

## Verifying the Vital Sign Write

After creating the visit, I used the Vital Signs API to verify that the vital-sign data from the same operation was actually stored.

The created record contained the expected heart rate and blood pressure measurements.

![Created Vital Sign](./Screenshots/week6-day4-visit-vitalsign-created.png)

This provided an additional check that the patient visit operation was performing a real database write rather than only returning a successful response.

---

## Testing a Business Rule

I also tested the visit endpoint with a vital-sign measurement date in the future.

The service rejected the operation and returned:

```text
400 Bad Request
```

with the message:

```text
Measurement date cannot be in the future.
```

![Future Measurement Rejected](./Screenshots/week6-day4-future-measurement-rejected.png)

This test demonstrates that the endpoint contains actual business logic beyond simple entity creation.

---

## Doctor Setup for the New Workflow

The expanded V2 database model includes doctors, but the API did not yet provide a way to create one.

To support the patient visit workflow and its testing, I added the first doctor API operations using:

```text
DoctorsController
CreateDoctorRequest
CreateDoctorValidator
```

Before creating a doctor, the API checks that:

- The Identity user exists
- The selected department exists
- The supervisor exists when one is provided
- The Identity user has not already been assigned to another doctor record

The seeded **Cardiology** department was used while preparing the doctor for the patient visit tests.

This also started closing the gap between the expanded V2 database model and the API layer.

---

## Identity Registration Improvement

I made a small improvement to the existing registration endpoint.

Previously, registration returned only a success message.

It now also returns the generated Identity user ID:

```json
{
  "message": "User registered successfully.",
  "userId": "..."
}
```

This made it possible to connect newly registered Identity users to domain entities such as doctors and patients without manually retrieving their IDs from the database.

---

## Updating the Existing Tests for V2

While preparing the branch for review, I ran the existing automated test suite.

Some tests were still based on the older V1 patient model and referenced:

```text
PhoneNumber
Address
```

In V2, the patient model was redesigned and phone numbers are represented separately as part of the normalized database structure.

I updated the affected validator and integration tests so they match the current V2 model.

I also updated the integration-test database setup to explicitly seed the patient required by `PatientsApiTests`.

This keeps the integration tests isolated from the real SQL Server database and gives them predictable test data.

---

## Automated Test Result

After updating the tests, I ran:

```bash
dotnet test
```

The final result was:

```text
Total: 13
Succeeded: 13
Failed: 0
Skipped: 0
```

![All Tests Passed](./Screenshots/week6-day4-all-tests-passed.png)

This confirmed that the existing automated tests still pass after today's V2 changes.

---

## Postman Testing

I created a separate Postman collection for today's work:

```text
Week 6 Day 4 - Write Operations & Business Logic
```

The collection includes the requests used to prepare and test the new workflow, including authentication, doctor and patient setup, the successful patient visit operation, and the business-rule rejection case.

The exported collection is stored inside the Day 4 folder.

---

## Day 4 Result

By the end of the day, I had:

- Implemented a write operation with business logic beyond simple CRUD
- Created a dedicated patient visit DTO and validator
- Moved the main workflow into a service class
- Added patient and doctor existence checks
- Added a business rule preventing future vital-sign measurements
- Created a medical record and vital sign as part of one operation
- Wrapped the database operation in an EF Core transaction
- Added the Patient Visits endpoint
- Added the initial Doctor API support needed by the V2 workflow
- Tested the workflow in Postman
- Documented successful and rejected requests with screenshots
- Updated older automated tests to match the V2 data model
- Verified all 13 automated tests pass
- Worked on a dedicated feature branch in preparation for code review

The next step is to finalize the branch, push it to GitHub, open a clean pull request, and request mentor review.