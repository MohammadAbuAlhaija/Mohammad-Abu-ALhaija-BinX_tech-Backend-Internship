using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class CreateDoctorValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage("Department ID must be greater than 0.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Doctor full name is required.");

        RuleFor(x => x.Specialization)
            .NotEmpty()
            .WithMessage("Doctor specialization is required.");

        RuleFor(x => x.SupervisorId)
            .GreaterThan(0)
            .When(x => x.SupervisorId.HasValue)
            .WithMessage("Supervisor ID must be greater than 0.");
    }
}