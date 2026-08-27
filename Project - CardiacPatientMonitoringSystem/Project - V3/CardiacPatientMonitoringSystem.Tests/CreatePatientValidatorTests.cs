using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Validators;

namespace CardiacPatientMonitoringSystem.Tests;

public class CreatePatientValidatorTests
{
    // Verifies that a valid patient request passes validation
    // without returning any validation errors.
    [Fact]
    public void Validate_WhenPatientDataIsValid_ReturnsNoErrors()
    {
        // Arrange
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            UserId = "test-user-id",
            FullName = "Ahmad Khalil",
            DateOfBirth = new DateTime(1990, 5, 10),
            Gender = "Male"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // Verifies that an empty patient name is rejected
    // by the CreatePatientValidator.
    [Fact]
    public void Validate_WhenFullNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            UserId = "test-user-id",
            FullName = "",
            DateOfBirth = new DateTime(1990, 5, 10),
            Gender = "Male"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == "FullName"
        );
    }

    // Verifies that a future date of birth is rejected
    // because a patient cannot be born in the future.
    [Fact]
    public void Validate_WhenDateOfBirthIsInFuture_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreatePatientValidator();

        var request = new CreatePatientRequest
        {
            UserId = "test-user-id",
            FullName = "Ahmad Khalil",
            DateOfBirth = DateTime.Today.AddDays(1),
            Gender = "Male"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == "DateOfBirth"
        );
    }
}