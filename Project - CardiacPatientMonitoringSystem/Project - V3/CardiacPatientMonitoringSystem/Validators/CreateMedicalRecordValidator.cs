using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class CreateMedicalRecordValidator
    : AbstractValidator<CreateMedicalRecordRequest>
{
    public CreateMedicalRecordValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.DoctorId)
            .GreaterThan(0)
            .WithMessage("Doctor ID must be greater than 0.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty()
            .WithMessage("Diagnosis is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}