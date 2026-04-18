using System.Net;
using System.Net.Http.Json;
using TodoApi.Tests.Helpers;

namespace TodoApi.Tests;

public class TodosTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task AuthenticateAsync(string username = "todouser")
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(_client, username);
        _client.SetBearerToken(token);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/todos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAuth_ReturnsEmptyList()
    {
        await AuthenticateAsync("emptylistuser");
        var response = await _client.GetAsync("/api/todos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Create_WithValidTitle_ReturnsCreated()
    {
        await AuthenticateAsync("createuser");
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.Equal("Buy milk", todo!.Title);
        Assert.False(todo.IsCompleted);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsBadRequest()
    {
        await AuthenticateAsync("emptytitleuser");
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTitleExceedingMaxLength_ReturnsBadRequest()
    {
        await AuthenticateAsync("longtitleuser");
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = new string('a', 501) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingTodo_ReturnsUpdatedTodo()
    {
        await AuthenticateAsync("updateuser");
        var created = await (await _client.PostAsJsonAsync("/api/todos", new { title = "Original" }))
            .Content.ReadFromJsonAsync<TodoItem>();

        var response = await _client.PatchAsJsonAsync($"/api/todos/{created!.Id}", new { title = "Updated", isCompleted = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TodoItem>();
        Assert.Equal("Updated", updated!.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task Delete_ExistingTodo_ReturnsNoContent()
    {
        await AuthenticateAsync("deleteuser");
        var created = await (await _client.PostAsJsonAsync("/api/todos", new { title = "To delete" }))
            .Content.ReadFromJsonAsync<TodoItem>();

        var response = await _client.DeleteAsync($"/api/todos/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentTodo_ReturnsNotFound()
    {
        await AuthenticateAsync("deletenotfounduser");
        var response = await _client.DeleteAsync("/api/todos/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserIsolation_CannotSeeOtherUsersTodos()
    {
        var client1 = factory.CreateClient();
        var token1 = await AuthHelper.RegisterAndGetTokenAsync(client1, "isolationuser1");
        client1.SetBearerToken(token1);
        await client1.PostAsJsonAsync("/api/todos", new { title = "User1 task" });

        var client2 = factory.CreateClient();
        var token2 = await AuthHelper.RegisterAndGetTokenAsync(client2, "isolationuser2");
        client2.SetBearerToken(token2);

        var response = await client2.GetAsync("/api/todos");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();

        Assert.Empty(todos!);
    }

    [Fact]
    public async Task UserIsolation_CannotUpdateOtherUsersTodo()
    {
        var client1 = factory.CreateClient();
        var token1 = await AuthHelper.RegisterAndGetTokenAsync(client1, "xupdateuser1");
        client1.SetBearerToken(token1);
        var created = await (await client1.PostAsJsonAsync("/api/todos", new { title = "Private task" }))
            .Content.ReadFromJsonAsync<TodoItem>();

        var client2 = factory.CreateClient();
        var token2 = await AuthHelper.RegisterAndGetTokenAsync(client2, "xupdateuser2");
        client2.SetBearerToken(token2);

        var response = await client2.PatchAsJsonAsync($"/api/todos/{created!.Id}", new { title = "Hacked" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserIsolation_CannotDeleteOtherUsersTodo()
    {
        var client1 = factory.CreateClient();
        var token1 = await AuthHelper.RegisterAndGetTokenAsync(client1, "xdeleteuser1");
        client1.SetBearerToken(token1);
        var created = await (await client1.PostAsJsonAsync("/api/todos", new { title = "Private task" }))
            .Content.ReadFromJsonAsync<TodoItem>();

        var client2 = factory.CreateClient();
        var token2 = await AuthHelper.RegisterAndGetTokenAsync(client2, "xdeleteuser2");
        client2.SetBearerToken(token2);

        var response = await client2.DeleteAsync($"/api/todos/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record TodoItem(int Id, string Title, bool IsCompleted, int SortOrder);
}
