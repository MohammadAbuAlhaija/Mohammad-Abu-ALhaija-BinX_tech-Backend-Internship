using FluentValidation;
using MyFirstApi.DTOs;

namespace MyFirstApi.Validators;

public class CreateCarValidator : AbstractValidator<CreateCarRequest>
{
    public CreateCarValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
            .WithMessage("Car make is required.");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Car model is required.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1950, DateTime.Now.Year + 1)
            .WithMessage($"Car year must be between 1950 and {DateTime.Now.Year + 1}.");

        RuleFor(x => x.VIN)
            .NotEmpty()
            .WithMessage("VIN is required.")
            .Length(17)
            .WithMessage("VIN must be exactly 17 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Car price must be greater than 0.");
    }
}