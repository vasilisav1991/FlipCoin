using System.Net;
using System.Net.Http.Json;

namespace Flipcoin.IntegrationTests;

public class ValidationTests : IClassFixture<FlipcoinApiFactory>
{
    private readonly FlipcoinApiFactory _factory;

    public ValidationTests(FlipcoinApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_with_invalid_email_returns_400_with_field_errors()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "not-an-email", password = "Password123!" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemWithErrors>();
        Assert.NotNull(problem!.Errors);
        Assert.Contains("Email", problem.Errors!.Keys);
    }

    private record ProblemWithErrors(Dictionary<string, string[]>? Errors);
}
