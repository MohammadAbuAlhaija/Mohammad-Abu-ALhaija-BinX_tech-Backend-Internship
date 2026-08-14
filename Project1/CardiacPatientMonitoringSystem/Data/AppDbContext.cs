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
}