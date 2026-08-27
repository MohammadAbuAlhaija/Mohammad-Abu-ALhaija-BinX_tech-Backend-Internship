using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdatePatientMedicationValidator
    : AbstractValidator<UpdatePatientMedicationRequest>
{
    public UpdatePatientMedicationValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.MedicationId)
            .GreaterThan(0)
            .WithMessage("Medication ID must be greater than 0.");

        RuleFor(x => x.Dosage)
            .NotEmpty()
            .WithMessage("Dosage is required.");

        RuleFor(x => x.Frequency)
            .NotEmpty()
            .WithMessage("Frequency is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");
    }
}