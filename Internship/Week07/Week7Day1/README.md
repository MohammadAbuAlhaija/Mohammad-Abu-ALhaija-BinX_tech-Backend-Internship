# Week 7 - Day 1: Sprint 2 Development & Role-Based Access Control

## Overview

Today I continued developing the **Cardiac Patient Monitoring System** as part of **Sprint 2**.

The main goal was to improve the existing project based on the requirements defined for the next stage of development. Some of the required concepts had already been introduced during the final day of **Week 6**, so instead of rebuilding existing functionality, I focused on extending the project, improving its structure, and making the authorization rules more realistic and secure.

The main focus today was on **role-based access control** and making sure that each type of user can access only the data that belongs to their responsibilities.

---

## Sprint 2 Development

The system is designed around three main user roles:

* `Admin`
* `Doctor`
* `Patient`

Each role has different responsibilities and permissions.

Authentication is handled using **ASP.NET Core Identity and JWT**, while authorization is handled using role-based rules and additional data-level checks.

The goal is to make the authorization reflect a realistic medical system rather than simply allowing or blocking users based only on their role.

---

## User Roles

### Admin

The **Admin** is responsible for managing the overall system.

The Admin has access to the system's complete data where required, including:

* Patients
* Doctors
* Appointments
* Medical Records
* Medications
* Vital Signs

The Admin can also perform administrative operations such as creating, updating, and deleting resources according to the endpoint permissions.

The Admin therefore represents the system-management level.

---

### Doctor

The **Doctor** has access to information related to the patients they are responsible for.

A Doctor can:

* View their doctor profile.
* View connected patients.
* View their patients' medical records.
* Create medical records for connected patients.
* Update medical records according to their permissions.
* View appointments assigned to them.
* Manage relevant vital-sign information for their connected patients.

The important part is that the `Doctor` role does not provide unrestricted access to every patient's medical information.

The system checks the relationship between the Doctor and the Patient before allowing access.

A Doctor can be connected to a Patient through:

* Appointments
* Medical Records

This provides an additional level of authorization beyond simply checking the user's role.

---

### Patient

The **Patient** is restricted to their own personal and medical information.

A Patient can:

* View their own patient profile.
* Update their own profile.
* View their own appointments.
* View their own vital signs.
* View their own medical records.

The Patient cannot access another patient's information.

The system identifies the authenticated user through the JWT token and uses the linked `UserId` to determine which Patient record belongs to that account.

---

## Role-Based Access Control

The authorization structure can be summarized as:

```text
                         Authenticated User
                                |
                                v
                           JWT Token
                                |
                                v
                             Role
                                |
              +-----------------+-----------------+
              |                 |                 |
              v                 v                 v
            Admin            Doctor            Patient
              |                 |                 |
              v                 v                 v
        Full Access       Connected Patients    Own Data
```

The system uses ASP.NET Core authorization attributes such as:

```csharp
[Authorize(Roles = "Admin,Doctor,Patient")]
```

and more restrictive rules such as:

```csharp
[Authorize(Roles = "Admin")]
```

For resources containing sensitive patient information, the system also performs additional checks to determine whether the authenticated user is actually allowed to access the requested data.

This creates two levels of authorization:

1. **Role-based authorization**
2. **Data-level authorization**

---

## Medical Records Authorization

The **Medical Records** resource was used to verify the implemented authorization rules because it contains sensitive patient information.

The expected access rules are:

```text
Admin
  └── Can access all medical records

Doctor
  └── Can access medical records of their connected patients only

Patient
  └── Can access their own medical records only
```

This ensures that knowing a patient's ID is not enough to access their medical information.

The API determines access based on the authenticated user's role and their relationship with the requested patient.

---

## Admin Access Test

The Medical Records endpoint was tested using an authenticated **Admin JWT token**:

```http
GET http://localhost:5075/api/medicalrecords
```

The Admin successfully received the available medical records.

This confirms that the Admin has the required full access to the Medical Records resource.

![Medical Records - Admin Access](./Screenshots/medical-records-admin-access.png)

---

## Doctor Access Test

The same endpoint was tested using an authenticated **Doctor JWT token**:

```http
GET http://localhost:5075/api/medicalrecords
```

The Doctor successfully received the medical records associated with their connected patients.

This confirms that Doctor access is restricted to the patients they are authorized to work with.

![Medical Records - Doctor Access](./Screenshots/medical-records-doctor-access.png)

---

## Patient Access Test

The Medical Records endpoint was also tested using an authenticated **Patient JWT token**:

```http
GET http://localhost:5075/api/medicalrecords
```

The Patient successfully received only their own medical records.

This confirms that Patient access is restricted to the authenticated patient's own data.

![Medical Records - Patient Access](./Screenshots/medical-records-patient-access.png)

---

## Authentication Structure

The project uses **ASP.NET Core Identity** for managing application users.

Identity users are connected to their corresponding application records through `UserId`.

The structure is:

```text
AspNetUsers
     |
     +---- UserId ----> Patient
     |
     +---- UserId ----> Doctor
```

When a user logs in, the API generates a JWT containing the user's identity information and roles.

The JWT is then used on protected API requests.

```text
Login
  |
  v
Identity User
  |
  v
User Role
  |
  v
JWT Token
  |
  v
Protected API Request
  |
  v
Authorization
```

This allows the API to identify both **who the user is** and **what role they have**.

---

## Project Structure

The project continues to use a structured ASP.NET Core Web API architecture:

```text
CardiacPatientMonitoringSystem/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── PatientsController.cs
│   ├── DoctorsController.cs
│   ├── AppointmentsController.cs
│   ├── MedicalRecordsController.cs
│   ├── VitalSignsController.cs
│   └── MedicationsController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── DTOs/
│
├── Models/
│
├── Services/
│
├── Validators/
│
├── Middleware/
│
├── Migrations/
│
├── Program.cs
└── appsettings.json
```

The controllers handle API requests and authorization rules, the models represent the database entities, and DTOs control the data exchanged through the API.

---

## Sprint 2 Result

By the end of today's development, I completed:

* Continued development of the project based on **Sprint 2** requirements.
* Improved the existing authentication and authorization structure.
* Defined clear responsibilities for `Admin`, `Doctor`, and `Patient`.
* Strengthened role-based authorization across the API.
* Added data-level access control where role-based authorization alone was not sufficient.
* Restricted Doctors to their connected patients.
* Restricted Patients to their own data.
* Maintained full administrative access for the Admin role.
* Applied the authorization rules to the Medical Records resource.
* Tested the Medical Records endpoint using Admin, Doctor, and Patient JWT tokens.
* Documented the authorization results using Postman screenshots.

---

## Final Result

The project now provides a clearer and more realistic authorization structure:

```text
                 Cardiac Patient Monitoring System
                              |
             +----------------+----------------+
             |                |                |
             v                v                v
           Admin            Doctor           Patient
             |                |                |
             v                v                v
        Full Access     Their Patients     Own Data
```

The main improvement is that the API does not rely only on the user's role.

It also considers the user's relationship with the requested data.

This makes the **Cardiac Patient Monitoring System** more secure, more organized, and closer to the authorization model expected in a real-world healthcare application.

The work completed today continues the development of **Sprint 2** and builds directly on the foundation established during the previous weeks of the internship.
