using System.Net;
using System.Net.Http.Json;
using TodoApi.Tests.Helpers;

namespace TodoApi.Tests;

public class AuthTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "newuser",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body?.Token);
        Assert.Equal("newuser", body!.Username);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        await AuthHelper.RegisterAndGetTokenAsync(_client, "dupeuser");

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "dupeuser",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortUsername_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "ab",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "validuser",
            password = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        await AuthHelper.RegisterAndGetTokenAsync(_client, "loginuser");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "loginuser",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body?.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await AuthHelper.RegisterAndGetTokenAsync(_client, "wrongpwuser");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "wrongpwuser",
            password = "wrongpassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "nobody",
            password = "password123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record AuthResponse(string Token, string Username);
}
