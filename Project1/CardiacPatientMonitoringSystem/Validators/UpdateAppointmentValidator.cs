using CardiacPatientMonitoringSystem.DTOs;
using FluentValidation;

namespace CardiacPatientMonitoringSystem.Validators;

public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("Patient ID must be greater than 0.");

        RuleFor(x => x.AppointmentDate)
            .NotEmpty()
            .WithMessage("Appointment date is required.");

        RuleFor(x => x.DoctorName)
            .NotEmpty()
            .WithMessage("Doctor name is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Appointment reason is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Appointment status is required.");
    }
}