using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdateVitalSignValidator : AbstractValidator<UpdateVitalSignRequest>
{
    public UpdateVitalSignValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

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