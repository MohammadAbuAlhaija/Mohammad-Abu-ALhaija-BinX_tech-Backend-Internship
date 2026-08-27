using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class CreatePatientPhoneValidator
    : AbstractValidator<CreatePatientPhoneRequest>
{
    public CreatePatientPhoneValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.");

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .WithMessage("Phone type must not exceed 50 characters.");
    }
}