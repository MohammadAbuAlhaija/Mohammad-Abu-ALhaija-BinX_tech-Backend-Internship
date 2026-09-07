using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Data;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Make sure roles exist
        string[] roles = { "Admin", "Doctor", "Patient" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role)
                );
            }
        }

        // Make sure departments exist
        if (!await context.Departments.AnyAsync())
        {
            context.Departments.AddRange(
                new Department
                {
                    Id = 1,
                    Name = "Cardiology",
                    Description = "Heart and cardiovascular care"
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
                    Description = "General internal medicine"
                }
            );

            await context.SaveChangesAsync();
        }

        // Seed Patients
        var patients = await context.Patients
            .OrderBy(p => p.Id)
            .ToListAsync();

        var existingPatientEmails = new HashSet<string>(
            await context.Patients
                .Where(p => p.User.Email != null)
                .Select(p => p.User.Email!)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );

        for (int i = patients.Count + 1; i <= 50; i++)
        {
            var email = $"dev.patient{i:D2}@cardiac.local";

            if (existingPatientEmails.Contains(email))
                continue;

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                user,
                "Test@12345"
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create patient user {email}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            result = await userManager.AddToRoleAsync(
                user,
                "Patient"
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to assign Patient role to {email}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            var patient = new Patient
            {
                UserId = user.Id,
                FullName = $"Development Patient {i:D2}",
                DateOfBirth = new DateTime(
                    1970 + (i % 30),
                    (i % 12) + 1,
                    (i % 27) + 1
                ),
                Gender = i % 2 == 0 ? "Male" : "Female"
            };

            context.Patients.Add(patient);
            existingPatientEmails.Add(email);

            // Save periodically so generated IDs are available
            if (i % 10 == 0)
            {
                await context.SaveChangesAsync();
            }
        }

        await context.SaveChangesAsync();

        // Reload patients after insertion
        patients = await context.Patients
            .OrderBy(p => p.Id)
            .ToListAsync();

        // Seed Doctors
        var doctors = await context.Doctors
            .OrderBy(d => d.Id)
            .ToListAsync();

        var existingDoctorEmails = new HashSet<string>(
            await context.Doctors
                .Where(d => d.User.Email != null)
                .Select(d => d.User.Email!)
                .ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );

        for (int i = doctors.Count + 1; i <= 10; i++)
        {
            var email = $"dev.doctor{i:D2}@cardiac.local";

            if (existingDoctorEmails.Contains(email))
                continue;

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                user,
                "Test@12345"
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create doctor user {email}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            result = await userManager.AddToRoleAsync(
                user,
                "Doctor"
            );

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to assign Doctor role to {email}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }

            var doctor = new Doctor
            {
                UserId = user.Id,
                DepartmentId = ((i - 1) % 3) + 1,
                FullName = $"Development Doctor {i:D2}",
                Specialization = (i % 3) switch
                {
                    0 => "Cardiology",
                    1 => "Internal Medicine",
                    _ => "Emergency Medicine"
                }
            };

            context.Doctors.Add(doctor);
            existingDoctorEmails.Add(email);

            if (i % 5 == 0)
            {
                await context.SaveChangesAsync();
            }
        }

        await context.SaveChangesAsync();

        doctors = await context.Doctors
            .OrderBy(d => d.Id)
            .ToListAsync();

        // Seed Medications
        if (await context.Medications.CountAsync() < 10)
        {
            var medicationNames = new[]
            {
                "Aspirin",
                "Atorvastatin",
                "Metoprolol",
                "Lisinopril",
                "Amlodipine",
                "Clopidogrel",
                "Losartan",
                "Warfarin",
                "Furosemide",
                "Nitroglycerin"
            };

            var existingMedicationNames = new HashSet<string>(
                await context.Medications
                    .Select(m => m.Name)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var name in medicationNames)
            {
                if (!existingMedicationNames.Contains(name))
                {
                    context.Medications.Add(
                        new Medication
                        {
                            Name = name,
                            Description =
                                $"Development test medication: {name}"
                        }
                    );
                }
            }

            await context.SaveChangesAsync();
        }

        var medications = await context.Medications
            .OrderBy(m => m.Id)
            .ToListAsync();

        // Seed Vital Signs
        if (await context.VitalSigns.CountAsync() < 200)
        {
            var existingVitalSigns = await context.VitalSigns
                .Select(v => new
                {
                    v.PatientId,
                    v.MeasuredAt
                })
                .ToListAsync();

            var existingVitalSignKeys = new HashSet<string>(
                existingVitalSigns.Select(v =>
                    $"{v.PatientId}_{v.MeasuredAt:yyyyMMddHHmmss}")
            );

            var random = new Random(42);

            foreach (var patient in patients)
            {
                for (int measurement = 0; measurement < 5; measurement++)
                {
                    var measuredAt = DateTime.Now
                        .AddDays(-(measurement + 1))
                        .AddHours(-patient.Id);

                    var key =
                        $"{patient.Id}_{measuredAt:yyyyMMddHHmmss}";

                    if (existingVitalSignKeys.Contains(key))
                        continue;

                    context.VitalSigns.Add(
                        new VitalSign
                        {
                            PatientId = patient.Id,
                            HeartRate = random.Next(60, 101),
                            SystolicBloodPressure =
                                random.Next(110, 141),
                            DiastolicBloodPressure =
                                random.Next(70, 91),
                            MeasuredAt = measuredAt
                        }
                    );

                    existingVitalSignKeys.Add(key);
                }
            }

            await context.SaveChangesAsync();
        }

        // Seed Appointments
        if (await context.Appointments.CountAsync() < 100)
        {
            var existingAppointments =
                await context.Appointments
                    .Select(a => new
                    {
                        a.PatientId,
                        a.DoctorId,
                        a.AppointmentDate
                    })
                    .ToListAsync();

            var existingAppointmentKeys =
                new HashSet<string>(
                    existingAppointments.Select(a =>
                        $"{a.PatientId}_{a.DoctorId}_{a.AppointmentDate:yyyyMMdd}")
                );

            for (int i = 0; i < 100; i++)
            {
                var patient = patients[i % patients.Count];
                var doctor = doctors[i % doctors.Count];

                var appointmentDate =
                    DateTime.Today.AddDays(i - 50);

                var key =
                    $"{patient.Id}_{doctor.Id}_{appointmentDate:yyyyMMdd}";

                if (existingAppointmentKeys.Contains(key))
                    continue;

                context.Appointments.Add(
                    new Appointment
                    {
                        PatientId = patient.Id,
                        DoctorId = doctor.Id,
                        AppointmentDate = appointmentDate,
                        Reason =
                            i % 2 == 0
                                ? "Cardiac follow-up"
                                : "Routine cardiovascular check",
                        Status =
                            (i % 3) switch
                            {
                                0 => "Scheduled",
                                1 => "Completed",
                                _ => "Cancelled"
                            }
                    }
                );

                existingAppointmentKeys.Add(key);
            }

            await context.SaveChangesAsync();
        }

        // Seed Medical Records
        if (await context.MedicalRecords.CountAsync() < 100)
        {
            var existingRecords =
                await context.MedicalRecords
                    .Select(m => new
                    {
                        m.PatientId,
                        m.DoctorId,
                        m.CreatedAt
                    })
                    .ToListAsync();

            var existingRecordKeys =
                new HashSet<string>(
                    existingRecords.Select(m =>
                        $"{m.PatientId}_{m.DoctorId}_{m.CreatedAt:yyyyMMdd}")
                );

            for (int i = 0; i < 100; i++)
            {
                var patient = patients[i % patients.Count];
                var doctor = doctors[i % doctors.Count];

                var createdAt =
                    DateTime.Now.AddDays(-(i + 1));

                var key =
                    $"{patient.Id}_{doctor.Id}_{createdAt:yyyyMMdd}";

                if (existingRecordKeys.Contains(key))
                    continue;

                context.MedicalRecords.Add(
                    new MedicalRecord
                    {
                        PatientId = patient.Id,
                        DoctorId = doctor.Id,
                        Diagnosis =
                            i % 2 == 0
                                ? "Hypertension"
                                : "Stable cardiac condition",
                        Notes =
                            "Development test medical record.",
                        CreatedAt = createdAt
                    }
                );

                existingRecordKeys.Add(key);
            }

            await context.SaveChangesAsync();
        }

        // Seed Patient Medications
        if (await context.PatientMedications.CountAsync() < 100)
        {
            var existingPatientMedications =
                await context.PatientMedications
                    .Select(pm => new
                    {
                        pm.PatientId,
                        pm.MedicationId
                    })
                    .ToListAsync();

            var existingKeys =
                new HashSet<string>(
                    existingPatientMedications.Select(pm =>
                        $"{pm.PatientId}_{pm.MedicationId}")
                );

            for (int i = 0; i < 100; i++)
            {
                var patient = patients[i % patients.Count];
                var medication = medications[i % medications.Count];

                var key =
                    $"{patient.Id}_{medication.Id}";

                if (existingKeys.Contains(key))
                    continue;

                context.PatientMedications.Add(
                    new PatientMedication
                    {
                        PatientId = patient.Id,
                        MedicationId = medication.Id,
                        Dosage = "1 tablet",
                        Frequency = "Once daily",
                        StartDate =
                            DateTime.Today.AddDays(-(i + 30)),
                        EndDate = null
                    }
                );

                existingKeys.Add(key);
            }

            await context.SaveChangesAsync();
        }

        Console.WriteLine(
            "Development test data seeding completed."
        );
    }
}

