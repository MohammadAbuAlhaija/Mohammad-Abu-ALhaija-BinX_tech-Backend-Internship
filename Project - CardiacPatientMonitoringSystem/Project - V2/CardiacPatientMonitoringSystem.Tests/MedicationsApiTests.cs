using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace CardiacPatientMonitoringSystem.Tests;

public class MedicationsApiTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MedicationsApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenAuthorized_ReturnsOk()
    {
        // Arrange
        AddTestJwt();

        // Act
        var response =
            await _client.GetAsync("/api/medications");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetById_WhenMedicationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        AddTestJwt();

        // Act
        var response =
            await _client.GetAsync("/api/medications/99999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

   [Fact]
public async Task GetById_WhenMedicationExists_ReturnsOk()
{
    // Arrange
    AddTestJwt();

    // Act
    var response =
        await _client.GetAsync("/api/medications/1001");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode
    );

    var medication =
        await response.Content.ReadFromJsonAsync<Medication>();

    Assert.NotNull(medication);

    Assert.Equal(1001, medication.Id);
    Assert.Equal("Aspirin", medication.Name);
    Assert.Equal(
        "Low-dose aspirin",
        medication.Description
    );
}

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response =
            await _client.GetAsync("/api/medications");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    private void AddTestJwt()
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

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                tokenString
            );
    }
}