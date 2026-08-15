using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardiacPatientMonitoringSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "DateOfBirth", "FullName", "Gender", "PhoneNumber" },
                values: new object[] { 1001, "Jenin", new DateTime(1985, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ahmad Khalil", "Male", "0599123456" });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "DoctorName", "PatientId", "Reason", "Status" },
                values: new object[] { 1001, new DateTime(2026, 8, 20, 10, 30, 0, 0, DateTimeKind.Unspecified), "Dr. Omar Khalil", 1001, "Routine cardiac checkup", "Scheduled" });

            migrationBuilder.InsertData(
                table: "Medications",
                columns: new[] { "Id", "Dosage", "EndDate", "Frequency", "Name", "PatientId", "StartDate" },
                values: new object[] { 1001, "81 mg", null, "Once daily", "Aspirin", 1001, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "VitalSigns",
                columns: new[] { "Id", "DiastolicBloodPressure", "HeartRate", "MeasuredAt", "PatientId", "SystolicBloodPressure" },
                values: new object[] { 1001, 80, 78, new DateTime(2026, 8, 10, 10, 30, 0, 0, DateTimeKind.Unspecified), 1001, 120 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Medications",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "VitalSigns",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1001);
        }
    }
}
