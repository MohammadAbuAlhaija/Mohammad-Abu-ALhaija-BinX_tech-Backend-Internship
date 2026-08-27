using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdatePatientValidator : AbstractValidator<UpdatePatientRequest>
{
    public UpdatePatientValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required.")
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender is required.");
    }
}