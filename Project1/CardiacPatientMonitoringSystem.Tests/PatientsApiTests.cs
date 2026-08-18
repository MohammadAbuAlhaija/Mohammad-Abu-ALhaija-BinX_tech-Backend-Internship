using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace CardiacPatientMonitoringSystem.Tests;

public class PatientsApiTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PatientsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPatientById_WhenPatientExists_ReturnsFullPatient()
    {
        // Arrange
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/patients/1001");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patient =
            await response.Content.ReadFromJsonAsync<Patient>();

        Assert.NotNull(patient);

        Assert.Equal(1001, patient.Id);
        Assert.Equal("Ahmad Khalil", patient.FullName);
        Assert.Equal(new DateTime(1985, 6, 15), patient.DateOfBirth);
        Assert.Equal("Male", patient.Gender);
        Assert.Equal("0599123456", patient.PhoneNumber);
        Assert.Equal("Jenin", patient.Address);
    }

    [Fact]
    public async Task GetPatientById_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/patients/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static string GenerateTestJwt()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                "CardiacPatientMonitoringSystem-SuperSecretKey-2026"
            )
        );

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "integration-test-user"
            ),

            new Claim(
                ClaimTypes.Email,
                "integrationtest@example.com"
            )
        };

        var token = new JwtSecurityToken(
            issuer: "CardiacPatientMonitoringSystem",
            audience: "CardiacPatientMonitoringSystemUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}