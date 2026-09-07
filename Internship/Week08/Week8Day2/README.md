# Week 8 — Sprint 3 — Day 2

## Query Optimization, Projection & N+1 Review

### Overview

Day 2 of **Sprint 3** continued the database performance work started on Day 1.

The main focus was to review the query patterns identified during the previous day's investigation and apply appropriate Entity Framework Core optimization techniques where they actually made sense for the current project.

One important finding from Day 1 was that the tested endpoints did **not** contain a genuine N+1 query problem. Therefore, no artificial N+1 scenario was introduced just to satisfy the exercise.

Instead, the work focused on:

* Reviewing the existing `Include` usage.
* Confirming that related data was already being loaded using a single database query.
* Applying projection to a list endpoint.
* Reducing the amount of data materialized by EF Core.
* Avoiding unnecessary entity graphs in API responses.
* Testing the optimized endpoint and inspecting the generated SQL.
* Evaluating whether `AsSplitQuery` was actually needed.
* Comparing the query behavior before and after the optimization.

---

# Day 2 Objectives

The objectives for today were:

1. Apply `Include` / `ThenInclude` where necessary to avoid inefficient relationship loading.
2. Re-run the endpoint with EF Core logging enabled.
3. Compare the number of generated SQL queries.
4. Convert one list-style endpoint to projection.
5. Evaluate `AsSplitQuery` for endpoints loading multiple collection navigations.
6. Document the actual performance results.

Because the Day 1 investigation did not identify a genuine N+1 problem, the optimization work was performed based on the actual structure and behavior of the project.

---

# N+1 Finding From Day 1

The most important starting point for Day 2 was the result of the Day 1 investigation.

The tested endpoints showed predictable query behavior:

| Endpoint                | Queries | N+1 |
| ----------------------- | ------: | --- |
| `GET /api/patients`     |       2 | No  |
| `GET /api/appointments` |       2 | No  |
| `GET /api/vitalsigns`   |       1 | No  |

The additional queries in the first two endpoints were expected operations such as counting records or identifying the authenticated patient.

The query count did **not** increase once for every returned record.

Therefore:

> **No genuine N+1 query problem was identified in the tested endpoints.**

For this reason, an artificial N+1 problem was not introduced into the project.

---

# Requirement 1 — Include / ThenInclude Review

The `PatientMedicationsController` already contained eager loading for the related `Patient` and `Medication` entities:

```csharp
var patientMedicationsQuery = _context.PatientMedications
    .Include(pm => pm.Patient)
    .Include(pm => pm.Medication)
    .AsQueryable();
```

This was reviewed as part of the Day 2 work.

The generated SQL showed that EF Core retrieved the patient medication records together with the related patient and medication data using `INNER JOIN` operations.

The important part of the generated query was:

```sql
FROM [PatientMedications] AS [p]
INNER JOIN [Patients] AS [p0]
    ON [p].[PatientId] = [p0].[Id]
INNER JOIN [Medications] AS [m]
    ON [p].[MedicationId] = [m].[Id]
```

The endpoint therefore did **not** generate one additional query for every patient medication record.

### Result

```text
N+1 detected: No
Query count: 1
```

The existing `Include` implementation was already performing eager loading correctly.

Therefore, there was no reason to change it simply to simulate an N+1 fix.

---

# Issue Found During Testing

Although there was no N+1 problem, testing the `GET /api/patientmedications` endpoint exposed another issue.

The endpoint originally returned the full `PatientMedication` entity together with its related entities:

```text
PatientMedication
       │
       ├── Patient
       │      │
       │      └── PatientMedications
       │               │
       │               └── Patient
       │                    ...
       │
       └── Medication
```

This created a circular object graph.

When ASP.NET Core attempted to serialize the entity graph into JSON, the request failed with an object-cycle exception:

```text
A possible object cycle was detected
```

The problem was caused by returning tracked entities with bidirectional navigation properties directly from the API.

This was not an N+1 problem, but it provided a useful opportunity to improve the endpoint.

---

# Requirement 3 — Projection

To avoid returning the complete entity graph, the `GET /api/patientmedications` list endpoint was changed to use **projection**.

The query now selects only the fields required by the API response:

```csharp
var patientMedications =
    await patientMedicationsQuery
        .Select(pm => new
        {
            pm.Id,
            pm.PatientId,
            PatientName = pm.Patient.FullName,
            pm.MedicationId,
            MedicationName = pm.Medication.Name,
            MedicationDescription = pm.Medication.Description,
            pm.Dosage,
            pm.Frequency,
            pm.StartDate,
            pm.EndDate
        })
        .ToListAsync();
```

`AsNoTracking()` was also added because this is a read-only list endpoint:

```csharp
.AsNoTracking()
```

The resulting approach can be summarized as:

```text
Before:

Database
   ↓
PatientMedication entity
   ↓
Patient entity
   ↓
PatientMedications collection
   ↓
Patient entity
   ↓
Circular object graph
   ↓
JSON serialization problem


After:

Database
   ↓
Required fields only
   ↓
Simple response objects
   ↓
JSON response
```

---

# Before vs After

### Before

The endpoint used:

```csharp
.Include(pm => pm.Patient)
.Include(pm => pm.Medication)
.ToListAsync();
```

This loaded the related entities into a complete object graph.

The SQL query itself was already efficient and used joins, but the returned entity graph caused a JSON serialization cycle.

### After

The endpoint uses:

```csharp
.AsNoTracking()
.Select(pm => new
{
    pm.Id,
    pm.PatientId,
    PatientName = pm.Patient.FullName,
    pm.MedicationId,
    MedicationName = pm.Medication.Name,
    MedicationDescription = pm.Medication.Description,
    pm.Dosage,
    pm.Frequency,
    pm.StartDate,
    pm.EndDate
})
.ToListAsync();
```

This approach:

* Selects only required fields.
* Avoids materializing the complete navigation graph.
* Avoids the JSON object-cycle problem.
* Uses `AsNoTracking()` for a read-only operation.
* Produces a cleaner API response.
* Keeps the database operation set-based.

---

# Projection Test Result

The endpoint was tested after the modification:

```text
GET /api/patientmedications
```

The request returned successfully:

```text
HTTP 200 OK
```

The response contained a simple object structure:

```json
[
    {
        "id": 4,
        "patientId": 1005,
        "patientName": "Week 7 Patient",
        "medicationId": 1004,
        "medicationName": "Metoprolol",
        "medicationDescription": "Development test medication: Metoprolol",
        "dosage": "1 tablet",
        "frequency": "Once daily",
        "startDate": "2026-08-06T00:00:00",
        "endDate": null
    }
]
```

The previous object-cycle serialization error no longer occurred.

---

# Generated SQL After Projection

EF Core generated a single SQL query:

```sql
SELECT [p].[Id],
       [p].[PatientId],
       [p0].[FullName] AS [PatientName],
       [p].[MedicationId],
       [m].[Name] AS [MedicationName],
       [m].[Description] AS [MedicationDescription],
       [p].[Dosage],
       [p].[Frequency],
       [p].[StartDate],
       [p].[EndDate]
FROM [PatientMedications] AS [p]
INNER JOIN [Patients] AS [p0]
    ON [p].[PatientId] = [p0].[Id]
INNER JOIN [Medications] AS [m]
    ON [p].[MedicationId] = [m].[Id]
WHERE [p].[PatientId] = @patientId;
```

### Result

```text
Database queries: 1
HTTP status: 200 OK
N+1: No
Object cycle: Resolved
```

The two similar SQL log entries shown by EF Core logging represent the same database command reported through different logging events. They were therefore counted as **one actual SQL query**, not two separate database operations.

---

# Query Count Comparison

The important point is that projection did **not** reduce the number of SQL queries from multiple queries to one.

The endpoint already executed one SQL query before the change.

The improvement was in **what was retrieved and materialized**, not in the number of round trips.

| Version           | SQL Queries | Main Result                           |
| ----------------- | ----------: | ------------------------------------- |
| Before Projection |           1 | Full entity graph + JSON cycle        |
| After Projection  |           1 | Required fields only + clean response |

Therefore:

> **Query count: 1 → 1**

This is an important distinction.

The optimization should not be described as an N+1 fix because there was no N+1 problem and the number of queries did not decrease.

The improvement came from using projection to make the read operation more focused and API-friendly.

---

# Requirement 4 — AsSplitQuery Evaluation

`AsSplitQuery()` was also evaluated as part of today's work.

The current `PatientMedication` query uses:

```text
PatientMedication
 ├── Patient
 └── Medication
```

Both `Patient` and `Medication` are **reference navigations**, not collection navigations.

The current endpoint therefore does not have the type of multiple collection relationship where `AsSplitQuery()` would provide a meaningful benefit.

The reviewed endpoints also did not contain a real list query that loaded two or more collection navigations together.

Therefore, `AsSplitQuery()` was **not added artificially**.

### Result

```text
AsSplitQuery: Not required for the current endpoints
```

This decision keeps the implementation based on the actual data model rather than modifying the project only to satisfy a theoretical scenario.

---

# Day 2 Performance Findings

The work completed today produced the following findings:

| Area                          | Result                        |
| ----------------------------- | ----------------------------- |
| Genuine N+1 problem           | Not found                     |
| Existing Include usage        | Already uses eager loading    |
| PatientMedication query       | 1 SQL query                   |
| Projection                    | Implemented                   |
| AsNoTracking                  | Added to read-only list query |
| JSON object cycle             | Resolved                      |
| Query count before projection | 1                             |
| Query count after projection  | 1                             |
| AsSplitQuery                  | Not required                  |
| Artificial N+1 introduced     | No                            |

---

# What Changed in the Code

The main code change was made to:

```text
PatientMedicationsController.cs
```

The `GetAll()` endpoint was changed from loading the full entity graph with `Include` to selecting the required response fields using projection.

The endpoint now uses:

```csharp
.AsNoTracking()
```

and:

```csharp
.Select(pm => new
{
    pm.Id,
    pm.PatientId,
    PatientName = pm.Patient.FullName,
    pm.MedicationId,
    MedicationName = pm.Medication.Name,
    MedicationDescription = pm.Medication.Description,
    pm.Dosage,
    pm.Frequency,
    pm.StartDate,
    pm.EndDate
})
```

The remaining CRUD operations were left unchanged because the Day 2 optimization target was the list endpoint.

---

# Day 2 Checklist

* [x] Reviewed the N+1 findings from Day 1.
* [x] Confirmed that no genuine N+1 problem exists in the tested endpoints.
* [x] Reviewed existing `Include` usage.
* [x] Confirmed that related data was already loaded using a set-based query.
* [x] Tested `GET /api/patientmedications`.
* [x] Identified the JSON object-cycle issue.
* [x] Replaced the list response with a projection.
* [x] Added `AsNoTracking()` to the read-only query.
* [x] Re-ran the endpoint after the change.
* [x] Confirmed `HTTP 200 OK`.
* [x] Confirmed that the object-cycle error was resolved.
* [x] Inspected the generated SQL.
* [x] Confirmed that the endpoint still uses one actual SQL query.
* [x] Evaluated whether `AsSplitQuery()` was appropriate.
* [x] Avoided introducing artificial relationships or an artificial N+1 problem.
* [x] Documented the before/after query behavior.

---

# Day Result

Day 2 successfully continued the performance investigation without introducing an artificial problem into the application.

The original Day 1 investigation showed that the tested endpoints did not contain a genuine N+1 query pattern. Therefore, an N+1 fix was not required.

Instead, a real issue was identified while testing the `PatientMedications` list endpoint: returning the complete entity graph caused a JSON object-cycle during serialization.

The endpoint was improved using **projection** and **`AsNoTracking()`**.

The final result was:

```text
Before:

1 SQL query
+
Full entity graph
+
JSON object cycle


After:

1 SQL query
+
Required fields only
+
Clean JSON response
+
HTTP 200 OK
```

The query count remained the same, but the shape of the database operation and API response became more efficient and predictable.

---

# Key Learning

The main lesson from Day 2 was:

> **Performance optimization should be based on actual application behavior, not on forcing a specific problem into the code.**

An N+1 problem is important to understand and prevent, but if the application's SQL logs show that the problem does not exist, creating one artificially would not provide a meaningful optimization.

Today's work also demonstrated that reducing query count is not the only form of database optimization.

Projection can improve a read endpoint by:

* Selecting only the required columns.
* Avoiding unnecessary entity materialization.
* Avoiding large navigation graphs.
* Producing cleaner API responses.
* Preventing serialization problems caused by bidirectional relationships.

Therefore, efficient database access should be evaluated through both **query count** and **query shape**.

---

# Sprint 3 Progress

The Sprint 3 work has now progressed from identifying and measuring query behavior to applying targeted optimization.

### Day 1

```text
Measure
   ↓
Inspect SQL
   ↓
Investigate N+1
   ↓
No genuine N+1 found
```

### Day 2

```text
Review existing loading strategies
   ↓
Test relationship-heavy endpoint
   ↓
Identify object graph problem
   ↓
Apply projection
   ↓
Verify generated SQL
   ↓
Confirm clean API response
```

The next Sprint work can continue with further query analysis, performance validation, and review of other relationship-heavy endpoints where optimization provides a measurable benefit.

---

# Tools & Technologies Used

* **C#**
* **.NET 10**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server**
* **EF Core Query Logging**
* **Swagger**
* **Postman**
* **Visual Studio Code / Terminal**
* **Git & GitHub**

---

# Week 8 Status

### Sprint 3 — Day 2

**Status: Completed ✅**

Day 2 successfully completed the applicable database optimization work.

No genuine N+1 query problem was found, so no artificial N+1 scenario was introduced.

The existing `Include` behavior was reviewed and confirmed to execute the relationship query as a single SQL operation.

The `GET /api/patientmedications` list endpoint was then optimized using projection and `AsNoTracking()`. Testing confirmed that the endpoint now returns a clean `HTTP 200 OK` response without the previous JSON object-cycle error.

The final query measurement remained:

```text
Before Projection: 1 query
After Projection:  1 query
```

The improvement was therefore not a reduction in query count, but a more focused query and response shape with less unnecessary entity materialization.

**Sprint 3 can now continue to the next day's performance task.**
