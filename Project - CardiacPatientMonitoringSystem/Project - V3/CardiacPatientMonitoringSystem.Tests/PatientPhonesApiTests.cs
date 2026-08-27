using System.Net;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;

namespace CardiacPatientMonitoringSystem.Tests;

public class PatientPhonesApiTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PatientPhonesApiTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenAuthorized_ReturnsOk()
    {
        AddTestJwt();

        var response =
            await _client.GetAsync("/api/patientphones");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetById_WhenPhoneDoesNotExist_ReturnsNotFound()
    {
        AddTestJwt();

        var response =
            await _client.GetAsync("/api/patientphones/99999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response =
            await _client.GetAsync("/api/patientphones");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Create_WhenNotAdmin_ReturnsForbidden()
    {
        AddTestJwt("Patient");

        var request = new
        {
            PatientId = 1001,
            PhoneNumber = "0599000000",
            Type = "Mobile"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/patientphones",
                request
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    private void AddTestJwt(string role = "Admin")
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
