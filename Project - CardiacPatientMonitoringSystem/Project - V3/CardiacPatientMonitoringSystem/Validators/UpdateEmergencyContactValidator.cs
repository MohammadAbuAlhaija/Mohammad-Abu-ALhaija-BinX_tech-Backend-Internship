using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdateEmergencyContactValidator
    : AbstractValidator<UpdateEmergencyContactRequest>
{
    public UpdateEmergencyContactValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Emergency contact name is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Emergency contact phone number is required.");

        RuleFor(x => x.Relationship)
            .NotEmpty()
            .WithMessage("Emergency contact relationship is required.");
    }
}