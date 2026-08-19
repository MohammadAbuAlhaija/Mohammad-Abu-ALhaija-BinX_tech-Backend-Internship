using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Validators;

namespace CardiacPatientMonitoringSystem.Tests;

public class CreatePatientValidatorTests
{
    [Fact]
    public void Validate_WhenPatientDataIsValid_ReturnsNoErrors()
    {
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            FullName = "Ahmad Khalil",
            DateOfBirth = new DateTime(1990, 5, 10),
            Gender = "Male",
            PhoneNumber = "0599123456",
            Address = "Jenin"
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenFullNameIsEmpty_ReturnsValidationError()
    {
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            FullName = "",
            DateOfBirth = new DateTime(1990, 5, 10),
            Gender = "Male",
            PhoneNumber = "0599123456",
            Address = "Jenin"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == "FullName"
        );
    }

    [Fact]
    public void Validate_WhenDateOfBirthIsInFuture_ReturnsValidationError()
    {
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            FullName = "Ahmad Khalil",
            DateOfBirth = DateTime.Today.AddDays(1),
            Gender = "Male",
            PhoneNumber = "0599123456",
            Address = "Jenin"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == "DateOfBirth"
        );
    }
}