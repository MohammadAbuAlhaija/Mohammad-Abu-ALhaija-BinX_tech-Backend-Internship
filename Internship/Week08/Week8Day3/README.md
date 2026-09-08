# Week 8 — Sprint 3 — Day 3

## Introducing Redis Caching

### Overview

Day 3 of **Sprint 3** focused on introducing distributed caching to reduce repeated database reads and improve API efficiency.

After the query investigation and optimization work completed during Day 1 and Day 2, the focus moved from improving individual database queries to reducing how often the application needs to access the database for frequently requested, relatively stable data.

The implementation used the existing **Medications** catalog as the caching target because medication records are suitable for caching compared with highly dynamic resources such as vital signs, appointments, or other frequently changing patient data.

The work completed today included:

* Identifying a suitable caching candidate.
* Installing the Redis-backed `IDistributedCache` package.
* Configuring a Redis connection string.
* Registering `IDistributedCache` in ASP.NET Core.
* Running a Redis-compatible server locally using **Memurai**.
* Implementing the **cache-aside pattern**.
* Adding cache expiration.
* Implementing cache invalidation after write operations.
* Testing cache hits and misses using Postman.
* Verifying generated SQL through EF Core logging.
* Confirming that cached requests avoid repeated database queries.
* Confirming that writes invalidate stale cached data.

---

# Day 3 Objectives

The objectives for today were:

1. Identify which application data is appropriate for caching.
2. Set up Redis with ASP.NET Core's `IDistributedCache` abstraction.
3. Implement the cache-aside pattern for a catalog-style endpoint.
4. Add cache invalidation for create, update, and delete operations.
5. Verify the cache behavior through real API requests and EF Core SQL logs.
6. Validate that updated data is returned after cache invalidation.

---

# 1. Choosing the Cache Candidate

The first step was to determine which existing endpoint in the project would benefit from caching.

The project contains several controllers:

```text
AppointmentsController
AuthController
DoctorPhonesController
DoctorsController
EmergencyContactsController
MedicalRecordsController
MedicationsController
PatientMedicationsController
PatientPhonesController
PatientsController
PatientVisitsController
VitalSignsController
```

The **MedicationsController** was selected as the caching target.

The main reason was that the medication catalog is read by multiple users and is relatively stable compared with more dynamic resources.

The existing endpoint is:

```http
GET /api/medications
```

This endpoint is accessible to:

```text
Admin
Doctor
Patient
```

while medication modifications are restricted to:

```text
Admin
```

This makes the medication catalog a good candidate for a shared cache entry.

---

# 2. What Belongs in a Cache

A useful caching candidate generally has two characteristics:

```text
Frequently Read
        +
Rarely Changed
        =
Good Cache Candidate
```

The medication catalog satisfies these characteristics better than highly dynamic data.

### Suitable for caching

```text
Medication catalog
Medication definitions
Reference data
Stable lookup data
```

### Less suitable for caching

```text
Vital signs
Appointment status
Live patient information
Frequently changing order/status data
```

Caching highly dynamic information can lead to stale results and requires more complicated invalidation logic.

Therefore, the project uses the medication catalog as the initial Redis caching example.

---

# 3. Redis Package Installation

The Redis-backed implementation for `IDistributedCache` was added using the following package:

```powershell
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

The installed package version was:

```text
Microsoft.Extensions.Caching.StackExchangeRedis 10.0.11
```

The package also installed the required Redis client dependencies, including:

```text
StackExchange.Redis 2.7.27
```

The package installation and restore completed successfully.

---

# 4. Redis Configuration

A Redis connection string was added to:

```text
appsettings.json
```

The existing SQL Server connection was preserved and the Redis connection was added:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CardiacPatientMonitoringDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

The application therefore expects the Redis-compatible server to be available at:

```text
localhost:6379
```

---

# 5. Redis-Compatible Local Server

Docker was not used for this implementation.

Instead, a Redis-compatible server was installed locally using:

```text
Memurai Developer Edition
```

Memurai was configured using the default settings and installed as a Windows service.

The service was verified through PowerShell:

```powershell
Get-Service Memurai
```

The result confirmed that the service was running:

```text
Status   Name      DisplayName
------   ----      -----------
Running  Memurai   Memurai
```

This provided the local Redis-compatible service required by the application.

---

# 6. Registering IDistributedCache

The Redis-backed `IDistributedCache` implementation was registered in `Program.cs`:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});
```

This allows controllers and other application components to request:

```csharp
IDistributedCache
```

without depending directly on the Redis client throughout the application.

The abstraction keeps the caching implementation isolated from the controller logic.

---

# 7. Cache-Aside Pattern

The application uses the **cache-aside** pattern for:

```http
GET /api/medications
```

The implemented flow is:

```text
                 GET /api/medications
                          │
                          ▼
                   Check Redis
                          │
              ┌───────────┴───────────┐
              │                       │
             HIT                    MISS
              │                       │
              ▼                       ▼
        Return cached          Query SQL Server
             data                     │
                                      ▼
                                Store in Redis
                                      │
                                      ▼
                                Return response
```

This means the application only queries SQL Server when the requested medication catalog is not already cached.

---

# 8. Cache Key

A dedicated cache key was defined in `MedicationsController`:

```csharp
private const string MedicationsCacheKey = "medications:all";
```

The key represents the complete medication catalog.

The cache entry therefore uses:

```text
medications:all
```

as its Redis key.

---

# 9. Implementing the Cached GET Endpoint

The original endpoint retrieved medications directly from SQL Server:

```csharp
var medications = await _context.Medications
    .ToListAsync();
```

It was changed to check Redis first.

The implemented logic is:

```csharp
var cachedData =
    await _cache.GetStringAsync(MedicationsCacheKey);

if (cachedData is not null)
{
    var cachedMedications =
        JsonSerializer.Deserialize<List<Medication>>(cachedData);

    if (cachedMedications is not null)
    {
        return Ok(cachedMedications);
    }
}
```

If the cache does not contain the data, the application queries the database:

```csharp
var medications = await _context.Medications
    .AsNoTracking()
    .ToListAsync();
```

The result is then serialized and stored in Redis:

```csharp
var serializedMedications =
    JsonSerializer.Serialize(medications);

await _cache.SetStringAsync(
    MedicationsCacheKey,
    serializedMedications,
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow =
            TimeSpan.FromMinutes(10)
    });
```

---

# 10. Cache Expiration

The medication catalog cache uses:

```text
10 minutes
```

of absolute expiration.

The configuration is:

```csharp
AbsoluteExpirationRelativeToNow =
    TimeSpan.FromMinutes(10)
```

This provides an expiration boundary for the cached data.

However, expiration is not used as the primary consistency mechanism.

The application also explicitly invalidates the cache whenever the medication catalog changes.

Therefore:

```text
Write → Immediate Invalidation
                  +
Expiration → Safety mechanism
```

---

# 11. Why AsNoTracking Was Used

The medication catalog is a read-only GET endpoint.

The query therefore uses:

```csharp
.AsNoTracking()
```

This is appropriate because the returned entities do not need to be tracked for later modifications during the request.

The change also remains consistent with the read-query optimization approach established during Day 2.

---

# 12. Serialization

Redis stores the medication list as serialized JSON.

The flow is:

```text
List<Medication>
       ↓
JsonSerializer.Serialize()
       ↓
JSON string
       ↓
Redis
```

When the cache is read:

```text
Redis
  ↓
JSON string
  ↓
JsonSerializer.Deserialize<List<Medication>>()
  ↓
Medication objects
```

This allows the existing API response structure to remain unchanged while the data is stored in Redis.

---

# 13. Cache Invalidation on Writes

Caching introduces a consistency problem when the underlying data changes.

For example:

```text
Database:
Medication Name = Old Name

Redis:
Medication Name = Old Name
```

If an administrator changes the medication:

```text
Database:
Medication Name = New Name
```

but the Redis entry is not removed, the API could continue returning:

```text
Old Name
```

Therefore, all write operations affecting the medication catalog explicitly invalidate the cache.

---

# 14. Create Invalidation

After creating a medication:

```csharp
await _context.SaveChangesAsync();
```

the cache is removed:

```csharp
await _cache.RemoveAsync(MedicationsCacheKey);
```

The flow becomes:

```text
POST /api/medications
        ↓
Insert into SQL Server
        ↓
Remove medications:all
```

The next GET will therefore reload the latest catalog from the database.

---

# 15. Update Invalidation

After updating a medication:

```csharp
await _context.SaveChangesAsync();
```

the same cache invalidation is performed:

```csharp
await _cache.RemoveAsync(MedicationsCacheKey);
```

The flow becomes:

```text
PUT /api/medications/{id}
        ↓
Update SQL Server
        ↓
Remove medications:all
```

This prevents stale cached medication data from remaining after an update.

---

# 16. Delete Invalidation

The delete operation also invalidates the catalog cache:

```csharp
await _context.SaveChangesAsync();

await _cache.RemoveAsync(MedicationsCacheKey);
```

The complete flow is:

```text
DELETE /api/medications/{id}
        ↓
Delete from SQL Server
        ↓
Remove medications:all
```

The next catalog request must therefore rebuild the cache from the current database state.

---

# 17. Successful Build

After the Redis package, configuration, and controller changes were applied, the project was built using:

```powershell
dotnet build
```

The build completed successfully.

This confirmed that the caching integration was correctly compiled before the runtime tests were performed.

---

# 18. Cache Testing

The API was run in the development environment:

```text
http://localhost:5075
```

The existing EF Core query logging from the previous Sprint tasks remained enabled.

The API was tested using **Postman**.

The main endpoint tested was:

```http
GET /api/medications
```

---

# 19. Cache Miss Test

The first request to:

```http
GET /api/medications
```

generated an actual SQL query:

```sql
SELECT [m].[Id], [m].[Description], [m].[Name]
FROM [Medications] AS [m]
```

This confirmed that the first request was a cache miss.

The flow was:

```text
GET /api/medications
        ↓
Redis MISS
        ↓
SQL Server
        ↓
Medication data retrieved
        ↓
Stored in Redis
        ↓
HTTP 200
```

---

# 20. Cache Hit Test

The same request was then repeated several times.

The subsequent requests returned:

```text
HTTP 200
```

without generating another SQL query for the `Medications` table.

The observed log sequence was:

```text
Request: GET /api/medications
Response Status: 200

Request: GET /api/medications
Response Status: 200

Request: GET /api/medications
Response Status: 200

Request: GET /api/medications
Response Status: 200
```

No new:

```sql
SELECT [m].[Id], [m].[Description], [m].[Name]
FROM [Medications] AS [m]
```

statement appeared for those subsequent requests.

### Result

```text
First GET:
Cache MISS → Database query → Redis

Subsequent GETs:
Cache HIT → No database query
```

This provides direct evidence that the cache-aside implementation was working.

---

# 21. Cache Invalidation Test

The next test verified that modifying medication data invalidates the cache.

An initial attempt to update:

```http
PUT /api/medications/1
```

returned:

```text
404 Not Found
```

because medication ID `1` did not exist.

A valid medication ID was then updated:

```http
PUT /api/medications/1005
```

The request returned:

```text
204 No Content
```

The SQL log showed the update:

```sql
UPDATE [Medications]
SET [Description] = @p0,
    [Name] = @p1
OUTPUT 1
WHERE [Id] = @p2;
```

The updated values included:

```text
Name:
Redis Test Medication

Description:
Updated to verify cache invalidation
```

After the database update, the controller invalidated:

```text
medications:all
```

---

# 22. GET After Invalidation

Immediately after the successful update, the catalog endpoint was requested again:

```http
GET /api/medications
```

This request generated a new SQL query:

```sql
SELECT [m].[Id], [m].[Description], [m].[Name]
FROM [Medications] AS [m]
```

The result was:

```text
HTTP 200 OK
```

This behavior confirms that the cached entry had been invalidated.

The sequence was:

```text
PUT /api/medications/1005
        ↓
Update database
        ↓
Invalidate medications:all
        ↓
GET /api/medications
        ↓
Redis MISS
        ↓
Query database
        ↓
Store updated catalog in Redis
        ↓
HTTP 200
```

---

# 23. Cache Hit After Rebuilding the Cache

After the post-update GET rebuilt the cache, the endpoint was requested again:

```http
GET /api/medications
```

This request returned:

```text
HTTP 200
```

without generating another SQL query for the medication catalog.

Therefore, the complete lifecycle was demonstrated:

```text
GET #1
→ Cache MISS
→ Database
→ Redis

GET #2
→ Cache HIT
→ No Database query

PUT
→ Database Update
→ Cache Invalidation

GET #3
→ Cache MISS
→ Database
→ Redis

GET #4
→ Cache HIT
→ No Database query
```

---

# 24. Cache Behavior Summary

| Request                       | Database Query | Cache Result | Status |
| ----------------------------- | -------------: | ------------ | ------ |
| First `GET /api/medications`  |            Yes | Miss         | 200    |
| Second `GET /api/medications` |             No | Hit          | 200    |
| `PUT /api/medications/1005`   |            Yes | Invalidate   | 204    |
| GET after PUT                 |            Yes | Miss         | 200    |
| Next GET                      |             No | Hit          | 200    |

This confirms both the cache-aside pattern and write invalidation behavior.

---

# 25. Before vs After Caching

### Before Caching

Every request accessed SQL Server:

```text
GET /api/medications
        ↓
SQL Server
        ↓
Return data
```

Repeated requests resulted in repeated database access.

### After Caching

The first request loads the database:

```text
GET /api/medications
        ↓
Redis MISS
        ↓
SQL Server
        ↓
Redis
        ↓
Response
```

Subsequent requests use Redis:

```text
GET /api/medications
        ↓
Redis HIT
        ↓
Response
```

This reduces repeated database reads for the same catalog data.

---

# 26. Cache Invalidation Strategy

The invalidation strategy implemented in the controller is:

```text
POST /api/medications
        ↓
SaveChangesAsync()
        ↓
Remove medications:all


PUT /api/medications/{id}
        ↓
SaveChangesAsync()
        ↓
Remove medications:all


DELETE /api/medications/{id}
        ↓
SaveChangesAsync()
        ↓
Remove medications:all
```

This keeps the cache aligned with database changes.

---

# 27. Why GET by ID Was Not Cached

The Day 3 implementation intentionally focused on:

```http
GET /api/medications
```

rather than introducing separate cache entries for:

```http
GET /api/medications/{id}
```

The objective was to demonstrate the catalog caching use case from the hands-on exercise without unnecessarily expanding the caching scope.

This keeps the initial implementation simple and makes the invalidation strategy straightforward:

```text
Any medication write
        ↓
Invalidate one catalog cache key
```

---

# 28. Redis vs Database Responsibility

The implementation maintains a clear separation of responsibilities:

```text
SQL Server
    ↓
System of record
    ↓
Authoritative medication data

Redis
    ↓
Performance layer
    ↓
Cached copy of frequently requested data
```

Redis is not treated as the primary data source.

The database remains the authoritative source, while Redis reduces unnecessary repeated reads.

---

# 29. Performance Measurement Note

The Day 3 hands-on exercise also included measuring response-time differences between cache misses and cache hits.

The cache behavior was successfully verified through:

* EF Core SQL logs.
* Repeated Postman requests.
* Presence or absence of SQL queries.
* Cache invalidation after updates.

However, exact response-time measurements were **not recorded as a formal benchmark during this session**.

Therefore, no fabricated timing values are included in this report.

The next performance measurement should use repeated requests and record actual Postman response times for:

```text
Cache Miss
Cache Hit
```

rather than assuming a specific timing improvement.

---

# 30. Files Changed

The main application changes for Day 3 were made to:

```text
appsettings.json
Program.cs
Controllers/MedicationsController.cs
```

### `appsettings.json`

Added:

```json
"Redis": "localhost:6379"
```

### `Program.cs`

Added Redis-backed `IDistributedCache` registration:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});
```

### `MedicationsController.cs`

Added:

* `IDistributedCache`.
* Redis cache key.
* Cache lookup.
* JSON serialization/deserialization.
* 10-minute expiration.
* Cache invalidation after create.
* Cache invalidation after update.
* Cache invalidation after delete.
* `AsNoTracking()` for the catalog read.

---

# 31. Day 3 Checklist

* [x] Identify suitable data for caching.
* [x] Select the medication catalog as the caching candidate.
* [x] Install `Microsoft.Extensions.Caching.StackExchangeRedis`.
* [x] Configure the Redis connection string.
* [x] Register Redis-backed `IDistributedCache`.
* [x] Install and run a Redis-compatible local server using Memurai.
* [x] Implement cache-aside for `GET /api/medications`.
* [x] Add a dedicated cache key.
* [x] Add a 10-minute absolute expiration.
* [x] Use `AsNoTracking()` for the read-only catalog query.
* [x] Serialize and store the medication list in Redis.
* [x] Return cached data when available.
* [x] Invalidate the catalog cache after medication creation.
* [x] Invalidate the catalog cache after medication updates.
* [x] Invalidate the catalog cache after medication deletion.
* [x] Build the project successfully.
* [x] Test the first GET as a cache miss.
* [x] Verify subsequent GET requests do not generate SQL queries.
* [x] Update a medication successfully.
* [x] Verify the update invalidates the catalog cache.
* [x] Verify the next GET queries the database again.
* [x] Verify the following GET is served from cache.
* [ ] Record formal response-time measurements for cache miss vs cache hit.

---

# Day Result

Day 3 successfully introduced **distributed caching** into the Cardiac Patient Monitoring System.

The medication catalog was selected as the initial caching target because it is frequently read and relatively stable compared with more dynamic clinical resources.

A Redis-backed implementation of `IDistributedCache` was configured using:

```text
Microsoft.Extensions.Caching.StackExchangeRedis
```

A Redis-compatible local server was provided through:

```text
Memurai
```

The `GET /api/medications` endpoint now follows the cache-aside pattern:

```text
Cache Hit
→ Return from Redis

Cache Miss
→ Query SQL Server
→ Store in Redis
→ Return response
```

The implementation was tested through Postman and EF Core SQL logging.

The first catalog request generated a SQL query, while repeated requests returned successfully without another SQL query for the medication catalog.

Cache invalidation was also verified by updating medication `1005`. After the update, the next catalog request generated a new SQL query, proving that the cached catalog had been removed and rebuilt using the updated database data.

The final demonstrated behavior was:

```text
GET
 ↓
Cache MISS
 ↓
Database
 ↓
Redis

GET
 ↓
Cache HIT
 ↓
No Database Query

PUT
 ↓
Database Update
 ↓
Cache Invalidation

GET
 ↓
Cache MISS
 ↓
Database
 ↓
Redis

GET
 ↓
Cache HIT
 ↓
No Database Query
```

---

# Key Learning

The main lesson from Day 3 was:

> **Caching should reduce unnecessary database reads without becoming a source of stale or incorrect data.**

A cache is useful when data is frequently requested but does not change constantly.

The implementation demonstrated that:

```text
Database
    ↓
Source of truth

Redis
    ↓
Fast cached copy
```

The cache-aside pattern provides a simple approach:

```text
Check Cache
     ↓
 HIT → Return
     ↓
MISS
     ↓
Database
     ↓
Populate Cache
     ↓
Return
```

However, caching cannot be treated as complete without an invalidation strategy.

For this project:

```text
Create
Update
Delete
   ↓
Invalidate medications:all
```

This ensures that the next read retrieves the latest database state instead of returning stale cached data.

Another important lesson was that **cache behavior should be verified through actual application evidence**. The SQL logs clearly demonstrated when the database was accessed and when subsequent requests were served without another SQL query.

---

# Sprint 3 Progress

Sprint 3 has now progressed through three stages:

### Day 1 — Query Diagnosis

```text
Measure SQL behavior
        ↓
Inspect generated queries
        ↓
Investigate N+1
        ↓
No genuine N+1 found
```

### Day 2 — Query Optimization

```text
Review relationship loading
        ↓
Identify object graph issue
        ↓
Apply projection
        ↓
Add AsNoTracking()
        ↓
Verify SQL and response
```

### Day 3 — Distributed Caching

```text
Identify stable read-heavy data
        ↓
Configure Redis
        ↓
Add IDistributedCache
        ↓
Implement Cache-Aside
        ↓
Add cache invalidation
        ↓
Verify Cache HIT / MISS
        ↓
Verify updated data after writes
```

This represents a progression from:

```text
Database query diagnosis
        ↓
Database query optimization
        ↓
Reducing repeated database access
```

---

# Tools & Technologies Used

* **C#**
* **.NET 10**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server / LocalDB**
* **Redis**
* **Memurai**
* **StackExchange.Redis**
* **Microsoft.Extensions.Caching.StackExchangeRedis**
* **IDistributedCache**
* **EF Core Query Logging**
* **Swagger**
* **Postman**
* **Visual Studio Code / Terminal**
* **Git & GitHub**

---

# Week 8 Status

### Sprint 3 — Day 3

**Status: Completed ✅**

Day 3 successfully introduced Redis-backed distributed caching into the Cardiac Patient Monitoring System.

The `GET /api/medications` catalog endpoint now uses the cache-aside pattern with a 10-minute expiration.

Testing confirmed that:

```text
First GET  → Database query + Redis population
Next GETs  → Redis cache hit + no medication SQL query
```

A medication update was then performed successfully, after which the cache was invalidated.

The following GET request queried the database again and returned the updated medication data, while the next GET was served from cache.

Therefore, both major Day 3 caching behaviors were demonstrated successfully:

```text
Cache-Aside ✅

Cache Invalidation on Writes ✅
```

The only remaining measurement item is a formal numeric comparison of Postman response times between cache miss and cache hit. No timing values were invented because they were not formally recorded during today's testing.

**Sprint 3 can now continue to the next performance task.**
