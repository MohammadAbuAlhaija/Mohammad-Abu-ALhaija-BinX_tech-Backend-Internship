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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Patient -> VitalSigns
        builder.Entity<Patient>()
            .HasMany(p => p.VitalSigns)
            .WithOne(v => v.Patient)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Department -> Doctors
        builder.Entity<Department>()
            .HasMany(d => d.Doctors)
            .WithOne(d => d.Department)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor -> Supervisor (self-referencing relationship)
        builder.Entity<Doctor>()
            .HasOne(d => d.Supervisor)
            .WithMany(d => d.Subordinates)
            .HasForeignKey(d => d.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Patient -> Identity User
        builder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor -> Identity User
        builder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial reference data
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
    }
}