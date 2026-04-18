using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace TodoApi.Tests.Helpers;

public static class AuthHelper
{
    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string username = "testuser", string password = "password123")
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { username, password });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }

    public static void SetBearerToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private record AuthResponse(string Token, string Username);
}
