using FluentValidation;
using MyFirstApi.DTOs;

namespace MyFirstApi.Validators;

public class UpdateCarValidator : AbstractValidator<UpdateCarRequest>
{
    public UpdateCarValidator()
    {
        // Make must not be empty.
        RuleFor(x => x.Make)
            .NotEmpty()
            .WithMessage("Car make is required.");

        // Model must not be empty.
        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Car model is required.");

        // Year must be within a realistic range.
        RuleFor(x => x.Year)
            .InclusiveBetween(1950, DateTime.Now.Year + 1)
            .WithMessage($"Car year must be between 1950 and {DateTime.Now.Year + 1}.");

        // VIN must be exactly 17 characters.
        RuleFor(x => x.VIN)
            .NotEmpty()
            .WithMessage("VIN is required.")
            .Length(17)
            .WithMessage("VIN must be exactly 17 characters.");

        // Price must be positive.
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Car price must be greater than 0.");
    }
}