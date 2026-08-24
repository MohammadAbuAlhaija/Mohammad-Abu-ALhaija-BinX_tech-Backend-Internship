# Week 6 - Day 2: Building the EF Core Data Model & Migrations

## Overview

Today I continued working on the **Cardiac Patient Monitoring System**, using the ERD designed on Day 1 as the main reference for the database structure.

The goal was to move from the database design into a complete EF Core model by creating and updating the entity classes, defining their relationships, adding reference seed data, and generating a migration that represents the new schema.

Since the project already had an existing database model from previous weeks, part of today's work also involved updating the existing code so it remained compatible with the new ERD.

---

## 1. Building the Full EF Core Model

The existing project originally focused on four main entities:

- Patients
- Vital Signs
- Medications
- Appointments

The Day 1 ERD expanded the system, so I updated the model layer and added the remaining entities needed to represent the full design.

The current model includes:

- `Patient`
- `Doctor`
- `Department`
- `Appointment`
- `VitalSign`
- `Medication`
- `PatientMedication`
- `PatientPhone`
- `DoctorPhone`
- `EmergencyContact`
- `MedicalRecord`

ASP.NET Core Identity is also still part of the project through the existing `AspNetUsers` table.

---

## 2. Navigation Properties

Navigation properties were added to represent the relationships between the entities in code.

For example, a patient can have multiple vital sign records:

```csharp
public ICollection<VitalSign> VitalSigns { get; set; }
    = new List<VitalSign>();
```

Each vital sign also references its patient:

```csharp
public int PatientId { get; set; }

public Patient Patient { get; set; } = null!;
```

The same approach was used for the other relationships in the ERD.

This keeps the EF Core model aligned with the foreign-key relationships designed in the database.

---

## 3. Separating Medications from Patient Medications

One important change from the previous version of the project was the medication model.

Previously, the `Medication` entity directly contained patient-specific information such as:

- PatientId
- Dosage
- Frequency
- StartDate
- EndDate

In the new ERD, these responsibilities are separated.

`Medication` now represents the medication itself:

```text
Medications
- Id
- Name
- Description
```

The relationship between a patient and a medication is stored separately:

```text
PatientMedications
- Id
- PatientId
- MedicationId
- Dosage
- Frequency
- StartDate
- EndDate
```

This produces a cleaner relational model and allows the same medication to be associated with different patients without duplicating the medication information.

---

## 4. Updating Appointments

Appointments were also updated to match the new ERD.

Previously, the appointment stored the doctor's name directly:

```csharp
DoctorName
```

The new model uses:

```csharp
public int DoctorId { get; set; }
```

with navigation to the `Doctor` entity.

This means appointments now reference actual doctors stored in the database instead of storing the doctor's name as plain text.

The appointment relationship now follows:

```text
Patient -> Appointment <- Doctor
```

---

## 5. Patient and Doctor Identity Relationships

Both patients and doctors are connected to ASP.NET Core Identity users through `UserId`.

Navigation properties were added so EF Core can represent these relationships directly.

For example:

```csharp
public string UserId { get; set; } = string.Empty;

public IdentityUser User { get; set; } = null!;
```

These relationships were also configured explicitly in `AppDbContext`.

---

## 6. Updating AppDbContext

`AppDbContext` was expanded with a `DbSet` for every entity in the new model.

```csharp
public DbSet<Patient> Patients { get; set; }
public DbSet<Doctor> Doctors { get; set; }
public DbSet<Department> Departments { get; set; }

public DbSet<VitalSign> VitalSigns { get; set; }

public DbSet<Medication> Medications { get; set; }
public DbSet<PatientMedication> PatientMedications { get; set; }

public DbSet<Appointment> Appointments { get; set; }

public DbSet<MedicalRecord> MedicalRecords { get; set; }

public DbSet<PatientPhone> PatientPhones { get; set; }
public DbSet<DoctorPhone> DoctorPhones { get; set; }

public DbSet<EmergencyContact> EmergencyContacts { get; set; }
```

This allows EF Core to include all of these entities in the database model.

---

## 7. Configuring Relationships with Fluent API

Although EF Core can discover many relationships automatically through conventions, several important relationships were configured explicitly using the Fluent API inside `OnModelCreating`.

### Patient and Vital Signs

```csharp
builder.Entity<Patient>()
    .HasMany(p => p.VitalSigns)
    .WithOne(v => v.Patient)
    .HasForeignKey(v => v.PatientId)
    .OnDelete(DeleteBehavior.Cascade);
```

This represents:

```text
Patient 1 ---- * VitalSigns
```

`Cascade` was selected because a vital sign record belongs to a specific patient and should not remain without its parent patient.

---

### Department and Doctors

```csharp
builder.Entity<Department>()
    .HasMany(d => d.Doctors)
    .WithOne(d => d.Department)
    .HasForeignKey(d => d.DepartmentId)
    .OnDelete(DeleteBehavior.Restrict);
```

This represents:

```text
Department 1 ---- * Doctors
```

`Restrict` was selected to prevent deleting a department while doctors are still assigned to it.

This makes the delete behavior explicit instead of relying only on EF Core conventions.

---

### Doctor Supervisor Relationship

Doctors can also reference another doctor as their supervisor.

```csharp
builder.Entity<Doctor>()
    .HasOne(d => d.Supervisor)
    .WithMany(d => d.Subordinates)
    .HasForeignKey(d => d.SupervisorId)
    .OnDelete(DeleteBehavior.Restrict);
```

This creates a self-referencing relationship:

```text
Doctor -> Supervisor
```

The supervisor is optional, while a doctor can supervise multiple other doctors.

---

### Identity Relationships

The relationships between patients/doctors and ASP.NET Identity users were also configured explicitly.

```csharp
builder.Entity<Patient>()
    .HasOne(p => p.User)
    .WithOne()
    .HasForeignKey<Patient>(p => p.UserId)
    .OnDelete(DeleteBehavior.Restrict);
```

and:

```csharp
builder.Entity<Doctor>()
    .HasOne(d => d.User)
    .WithOne()
    .HasForeignKey<Doctor>(d => d.UserId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 8. Adding Seed Data

I added initial reference data for the `Departments` table using EF Core's `HasData()` method.

```csharp
builder.Entity<Department>().HasData(
    new Department
    {
        Id = 1,
        Name = "Cardiology",
        Description = "Diagnosis and treatment of heart conditions"
    },
    new Department
    {
        Id = 2,
        Name = "Emergency",
        Description = "Emergency medical care"
    },
    new Department
    {
        Id = 3,
        Name = "Internal Medicine",
        Description = "General internal medical care"
    }
);
```

This ensures that these basic departments are automatically available when the database is created.

The seeded departments are:

| Id | Department |
| --- | --- |
| 1 | Cardiology |
| 2 | Emergency |
| 3 | Internal Medicine |

---

## 9. Updating Existing DTOs and Controllers

Changing the database model also affected some of the existing API code.

Instead of leaving the old controllers referencing properties that no longer existed, I updated the affected DTOs and controllers.

### Patients

The previous patient model contained:

```text
PhoneNumber
Address
```

These were removed from the patient DTO/controller logic because phone numbers are now represented by the separate `PatientPhones` entity, while the new ERD does not include `Address` in the `Patients` table.

`UserId` was added when creating a patient to support the Identity relationship.

### Appointments

The previous:

```text
DoctorName
```

was replaced by:

```text
DoctorId
```

The controller now also checks whether both the requested patient and doctor exist before creating or updating an appointment.

### Medications

The medication controller was updated to work with medication reference data:

```text
Name
Description
```

Patient-specific medication details now belong to `PatientMedication`.

### Vital Signs

The existing Vital Signs controller, DTOs, and validators were reviewed against the new ERD.

They were already compatible with the updated model, so unnecessary changes were avoided.

---

## 10. Updating FluentValidation

The existing validators were also reviewed because some of them referenced properties that were removed during the model update.

The validators for:

- Patients
- Appointments
- Medications

were updated to match their new DTOs.

Validation messages were kept clear and user-friendly.

Example:

```csharp
RuleFor(x => x.FullName)
    .NotEmpty()
    .WithMessage("Full name is required.");
```

The existing Vital Sign validators already matched the new model and were kept as they were.

---

## 11. Verifying the Project Build

After updating the entities, DTOs, controllers, validators, and `AppDbContext`, I verified that the project compiled successfully.

```bash
dotnet build
```

Result:

```text
Build succeeded
```

This confirmed that the existing project code was compatible with the updated EF Core model before generating the migration.

---

## 12. Generating the Migration

Because this capstone project already contained migrations from previous work, I kept the existing migration history instead of deleting it.

A new migration was generated for the expanded data model:

```bash
dotnet ef migrations add BuildFullDataModel
```

The migration was generated successfully.

EF Core also displayed a warning that some operations could result in data loss because the new model removes or replaces some columns from the previous schema.

Instead of immediately applying the migration, I reviewed the generated migration first.

---

## 13. Reviewing the Generated Migration

Before updating the database, I checked the generated migration to make sure the expected changes were present.

The migration included:

- New tables
- New columns
- Foreign keys
- Indexes
- Updated relationships
- Department seed data
- Removal of fields that belonged to the old model

This review was important because the existing database contained the older version of the project schema.

---

## 14. Handling the Existing Test Database

The first database update encountered a foreign-key conflict caused by old test data.

The existing appointments were created before `DoctorId` became part of the model, so those records could not satisfy the new foreign-key relationship with the `Doctors` table.

Since the existing records were only test data from previous development and were not production data, I removed the old local database:

```bash
dotnet ef database drop
```

After confirming the database deletion, I recreated the database by applying the migration history again:

```bash
dotnet ef database update
```

This allowed EF Core to build the database cleanly using the current model.

---

## 15. Verifying the Database in SSMS

After applying the migrations successfully, I opened the database using **SQL Server Management Studio (SSMS)**.

The database used by the project is:

```text
CardiacPatientMonitoringDb
```

running on:

```text
(localdb)\MSSQLLocalDB
```

I confirmed that the expected tables were created, including:

```text
Appointments
Departments
DoctorPhones
Doctors
EmergencyContacts
MedicalRecords
Medications
PatientMedications
PatientPhones
Patients
VitalSigns
```

The ASP.NET Core Identity tables were also present.

---

## 16. Verifying Seed Data

Finally, I queried the `Departments` table in SSMS.

```sql
SELECT TOP (1000)
    [Id],
    [Name],
    [Description]
FROM [CardiacPatientMonitoringDb].[dbo].[Departments];
```

The database returned the three seeded departments:

```text
1   Cardiology         Diagnosis and treatment of heart conditions
2   Emergency          Emergency medical care
3   Internal Medicine  General internal medical care
```

This confirmed that `HasData()` was included in the migration and applied successfully.

---

## Final Result

By the end of Day 2, the database layer of the **Cardiac Patient Monitoring System** was expanded to match the ERD designed on Day 1.

The full EF Core model is now represented in code, navigation properties are in place, important relationships and delete behaviors are explicitly configured through the Fluent API, reference data is seeded automatically, and the new schema has been successfully migrated to SQL Server.

The final database structure was also verified directly through SSMS.

The new entities are currently part of the EF Core data model. Their API controllers will be implemented when the upcoming project tasks require CRUD operations for those resources.

---

## Tools Used

- ASP.NET Core Web API
- C#
- Entity Framework Core
- EF Core Code-First Migrations
- Fluent API
- ASP.NET Core Identity
- FluentValidation
- SQL Server LocalDB
- SQL Server Management Studio (SSMS)
- Visual Studio Code / Terminal