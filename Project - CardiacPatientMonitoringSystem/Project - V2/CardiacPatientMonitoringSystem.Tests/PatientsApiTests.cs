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
        // Creates an HTTP client connected to the test version
        // of the ASP.NET Core application.
        _client = factory.CreateClient();
    }

    // Verifies that requesting an existing patient
    // returns 200 OK with the expected patient data.
    [Fact]
    public async Task GetPatientById_WhenPatientExists_ReturnsFullPatient()
    {
        // Arrange

        // Generate a valid JWT because the PatientsController
        // is protected with the [Authorize] attribute.
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act

        // Patient 1001 is part of the test data.
        var response = await _client.GetAsync("/api/patients/1001");

        // Assert

        // The endpoint should successfully find the patient.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patient =
            await response.Content.ReadFromJsonAsync<Patient>();

        Assert.NotNull(patient);

        // Verify that the returned patient contains
        // the expected data from the test database.
        Assert.Equal(1001, patient.Id);
        Assert.Equal("Ahmad Khalil", patient.FullName);
        Assert.Equal(new DateTime(1985, 6, 15), patient.DateOfBirth);
        Assert.Equal("Male", patient.Gender);
    }

    // Verifies that requesting a patient that does not exist
    // returns the correct 404 Not Found response.
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

    // Creates a JWT used only by the integration tests
    // so protected API endpoints can be tested.
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
            ),

            new Claim(
                ClaimTypes.Role,
                "Admin"
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