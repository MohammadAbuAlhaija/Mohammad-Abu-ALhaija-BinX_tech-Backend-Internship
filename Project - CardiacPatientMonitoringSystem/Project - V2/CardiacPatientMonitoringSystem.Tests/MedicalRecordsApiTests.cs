using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacPatientMonitoringSystem.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace CardiacPatientMonitoringSystem.Tests;

public class MedicalRecordsApiTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MedicalRecordsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Verifies that an authenticated Patient
    // can access the medical records endpoint.
    [Fact]
    public async Task GetAll_WhenAuthenticatedAsPatient_ReturnsOk()
    {
        // Arrange
        var token = GenerateTestJwt("Patient");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response =
            await _client.GetAsync("/api/medicalrecords");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    // Verifies that requesting a medical record
    // that does not exist returns 404 Not Found.
    [Fact]
    public async Task GetById_WhenMedicalRecordDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var token = GenerateTestJwt("Patient");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response =
            await _client.GetAsync(
                "/api/medicalrecords/99999"
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
            await _client.GetAsync("/api/medicalrecords");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    // Verifies that a Patient cannot create
    // a medical record because creation is restricted
    // to Admin and Doctor roles.
    [Fact]
    public async Task Create_WhenAuthenticatedAsPatient_ReturnsForbidden()
    {
        // Arrange
        var token = GenerateTestJwt("Patient");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateMedicalRecordRequest
        {
            PatientId = 1001,
            DoctorId = 1,
            Diagnosis = "Test diagnosis",
            Notes = "Integration test"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/medicalrecords",
                request
            );

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    // Creates a JWT used only by the integration tests.
    private static string GenerateTestJwt(string role)
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
                role
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
