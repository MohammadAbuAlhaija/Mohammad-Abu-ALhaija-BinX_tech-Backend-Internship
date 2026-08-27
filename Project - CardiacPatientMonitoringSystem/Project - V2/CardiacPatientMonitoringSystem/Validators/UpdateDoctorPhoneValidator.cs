using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdateDoctorPhoneValidator
    : AbstractValidator<UpdateDoctorPhoneRequest>
{
    public UpdateDoctorPhoneValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0)
            .WithMessage("Doctor ID must be greater than 0.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.");

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .WithMessage("Phone type must not exceed 50 characters.");
    }
}