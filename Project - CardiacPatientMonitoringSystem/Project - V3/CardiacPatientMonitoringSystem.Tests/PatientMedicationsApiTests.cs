using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacPatientMonitoringSystem.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace CardiacPatientMonitoringSystem.Tests;

public class PatientMedicationsApiTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PatientMedicationsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Verifies that an authenticated user can
    // access the patient medications endpoint.
    [Fact]
    public async Task GetAll_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response =
            await _client.GetAsync("/api/patientmedications");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    // Verifies that requesting a patient medication
    // that does not exist returns 404 Not Found.
    [Fact]
    public async Task GetById_WhenPatientMedicationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response =
            await _client.GetAsync(
                "/api/patientmedications/99999"
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    // Verifies that unauthenticated requests
    // are rejected by the authorization system.
    [Fact]
    public async Task GetAll_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync("/api/patientmedications");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    // Verifies that a normal authenticated user
    // cannot create a patient medication record
    // because the endpoint requires the Admin role.
    [Fact]
    public async Task Create_WhenAuthenticatedWithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateTestJwt();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new CreatePatientMedicationRequest
        {
            PatientId = 1001,
            MedicationId = 1001,
            Dosage = "81 mg",
            Frequency = "Once daily",
            StartDate = new DateTime(2026, 8, 1),
            EndDate = null
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/patientmedications",
                request
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    // Creates a JWT used only by the integration tests.
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
        "Patient"
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