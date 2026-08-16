using CardiacPatientMonitoringSystem.Services;

namespace CardiacPatientMonitoringSystem.Tests;

public class PatientServiceTests
{
    [Fact]
    public void CalculateAge_WhenBirthdayAlreadyPassed_ReturnsCorrectAge()
    {
        // Arrange
        var service = new PatientService();
        var dateOfBirth = new DateTime(2000, 5, 10);
        var referenceDate = new DateTime(2026, 8, 16);

        // Act
        var result = service.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public void CalculateAge_WhenBirthdayHasNotOccurredYet_ReturnsCorrectAge()
    {
        // Arrange
        var service = new PatientService();
        var dateOfBirth = new DateTime(2000, 12, 10);
        var referenceDate = new DateTime(2026, 8, 16);

        // Act
        var result = service.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public void CalculateAge_WhenBirthdayIsToday_ReturnsCorrectAge()
    {
        // Arrange
        var service = new PatientService();
        var dateOfBirth = new DateTime(2000, 8, 16);
        var referenceDate = new DateTime(2026, 8, 16);

        // Act
        var result = service.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        Assert.Equal(26, result);
    }

    [Theory]
[InlineData(2000, 5, 10, 2026, 8, 16, 26)]
[InlineData(2000, 12, 10, 2026, 8, 16, 25)]
[InlineData(2000, 8, 16, 2026, 8, 16, 26)]
public void CalculateAge_WithDifferentDates_ReturnsExpectedAge(
    int birthYear,
    int birthMonth,
    int birthDay,
    int referenceYear,
    int referenceMonth,
    int referenceDay,
    int expectedAge)
{
    // Arrange
    var service = new PatientService();
    var dateOfBirth = new DateTime(birthYear, birthMonth, birthDay);
    var referenceDate = new DateTime(referenceYear, referenceMonth, referenceDay);

    // Act
    var result = service.CalculateAge(dateOfBirth, referenceDate);

    // Assert
    Assert.Equal(expectedAge, result);
}
}