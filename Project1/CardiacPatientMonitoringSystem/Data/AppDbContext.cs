using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<VitalSign> VitalSigns { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Patient>().HasData(
            new Patient
            {
                Id = 1001,
                FullName = "Ahmad Khalil",
                DateOfBirth = new DateTime(1985, 6, 15),
                Gender = "Male",
                PhoneNumber = "0599123456",
                Address = "Jenin"
            }
        );

        builder.Entity<VitalSign>().HasData(
            new VitalSign
            {
                Id = 1001,
                PatientId = 1001,
                HeartRate = 78,
                SystolicBloodPressure = 120,
                DiastolicBloodPressure = 80,
                MeasuredAt = new DateTime(2026, 8, 10, 10, 30, 0)
            }
        );

        builder.Entity<Medication>().HasData(
            new Medication
            {
                Id = 1001,
                PatientId = 1001,
                Name = "Aspirin",
                Dosage = "81 mg",
                Frequency = "Once daily",
                StartDate = new DateTime(2026, 8, 1),
                EndDate = null
            }
        );

        builder.Entity<Appointment>().HasData(
            new Appointment
            {
                Id = 1001,
                PatientId = 1001,
                AppointmentDate = new DateTime(2026, 8, 20, 10, 30, 0),
                DoctorName = "Dr. Omar Khalil",
                Reason = "Routine cardiac checkup",
                Status = "Scheduled"
            }
        );
    }
}