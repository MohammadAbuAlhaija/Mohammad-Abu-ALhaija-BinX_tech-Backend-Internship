using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class CreatePatientVisitValidator
    : AbstractValidator<CreatePatientVisitRequest>
{
    public CreatePatientVisitValidator()
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

        RuleFor(x => x.HeartRate)
            .GreaterThan(0)
            .WithMessage("Heart rate must be greater than 0.");

        RuleFor(x => x.SystolicBloodPressure)
            .GreaterThan(0)
            .WithMessage("Systolic blood pressure must be greater than 0.");

        RuleFor(x => x.DiastolicBloodPressure)
            .GreaterThan(0)
            .WithMessage("Diastolic blood pressure must be greater than 0.");

        RuleFor(x => x.MeasuredAt)
            .NotEmpty()
            .WithMessage("Measurement date is required.");
    }
}