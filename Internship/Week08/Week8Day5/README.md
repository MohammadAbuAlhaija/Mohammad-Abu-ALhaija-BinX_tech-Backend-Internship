# Week 8 — Sprint 3 — Day 5

## Sprint Review, Benchmark Demo & Retrospective

### Overview

Day 5 was used to review the Sprint 3 performance work and confirm that the completed changes were tested and documented. The review covered query behavior, query projection, Redis caching, cache invalidation, and database indexing.

The Sprint work was reviewed against the evidence collected during Days 1–4. The completed performance changes are working as expected in the local development environment.

---

## Sprint 3 Review Summary

| Area | Result | Evidence |
| --- | --- | --- |
| N+1 query investigation | No N+1 issue found in the tested endpoints | `GET /api/patients` used 2 queries, `GET /api/appointments` used 2 queries, and `GET /api/vitalsigns` used 1 query. |
| Query projection | Completed | `GET /api/patientmedications` was changed to use projection and `AsNoTracking()`, resolving the JSON object-cycle issue while keeping the query count at 1. |
| Redis caching | Completed | The first `GET /api/medications` was a cache miss and executed SQL. Repeated requests were cache hits and did not execute another medication query. |
| Cache invalidation | Completed and tested | Creating, updating, or deleting a medication removes `medications:all`. A GET after an update returned the latest data and rebuilt the cache. |
| Composite database index | Completed | The appointments query was measured at 54ms before the `(PatientId, Status)` index and 18ms after it. |

---

## Before/After Benchmark Evidence

### Appointments Composite Index

Endpoint:

```http
GET /api/appointments?status=Scheduled
```

| Measurement | Recorded Query Time |
| --- | ---: |
| Before `IX_Appointments_PatientId_Status` | 54ms |
| After `IX_Appointments_PatientId_Status` | 18ms |
| Improvement | ~66.7% |

The composite index matches the endpoint's filtering pattern: `PatientId` and `Status`. These timings were recorded locally using the same request conditions, so they are treated as local profiling evidence rather than a production guarantee.

### Medications Redis Cache

```text
First GET  -> Cache miss -> SQL Server query -> Store result in Redis
Later GETs -> Cache hit  -> Return cached result -> No new medication SQL query
```

The cache behavior and invalidation flow were verified through Postman and EF Core SQL logging. Formal response-time values for cache miss versus cache hit were not recorded during this sprint.

---

## Cache Invalidation Strategy

The medication catalog uses the cache key:

```text
medications:all
```

The cache uses a 10-minute absolute expiration as a safety boundary. Consistency is maintained by immediately removing this key after a successful medication create, update, or delete operation.

```text
Medication write
      ↓
Save changes to SQL Server
      ↓
Remove medications:all from Redis
      ↓
Next GET reads current data and rebuilds the cache
```

---

## Sprint 4 Backlog

* Add an automated regression test for the medication cache invalidation flow.
* Add an automated test for the `PatientMedications` projection endpoint to prevent the JSON object-cycle issue from returning.
* Re-check performance measurements with a larger development dataset when needed.

---

## Sprint 3 Retrospective

### What went well

* Performance decisions were based on EF Core SQL logs and real endpoint tests rather than assumptions.
* Redis caching was implemented with tested invalidation, so stale medication data is not returned after writes.
* The composite index produced a measurable local improvement for the appointments filter query.

### What can improve

* Capture formal cache miss and cache hit timings earlier in the performance workflow.
* Turn manual performance checks into automated regression tests where practical.

### Sprint 4 Action

Add an automated regression test that verifies the medications cache is invalidated after a medication write and that the next GET returns current data.

---

## Day Result

Sprint 3 was reviewed successfully. The query investigation confirmed predictable query behavior in the tested endpoints, Redis caching and invalidation were verified, and the appointments composite index showed a measurable improvement from **54ms to 18ms** in the local profiling run.

The remaining follow-up work is recorded for Sprint 4, with the main action being automated regression protection for the caching behavior.
