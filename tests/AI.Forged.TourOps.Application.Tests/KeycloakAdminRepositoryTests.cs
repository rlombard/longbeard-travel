using System.Net;
using System.Text;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Infrastructure.Configuration;
using AI.Forged.TourOps.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class KeycloakAdminRepositoryTests
{
    [Fact]
    public async Task CreateUserAsync_SendsTemporaryPasswordAndUpdatePasswordRequiredAction()
    {
        var handler = new RecordingHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""{"access_token":"token-1","expires_in":300}""")
            },
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Headers =
                {
                    Location = new Uri("http://keycloak:8080/admin/realms/tourops/users/user-123")
                }
            }
        ]);

        var repository = CreateRepository(handler);

        var userId = await repository.CreateUserAsync(new KeycloakAdminCreateUserInput
        {
            Username = "new.user",
            Email = "new.user@example.com",
            FirstName = "New",
            LastName = "User",
            Enabled = true,
            EmailVerified = false,
            TemporaryPassword = "Temp!23456"
        });

        Assert.Equal("user-123", userId);
        Assert.Equal(2, handler.Requests.Count);
        var payload = await handler.Requests[1].Content!.ReadAsStringAsync();
        Assert.Contains("UPDATE_PASSWORD", payload);
        Assert.Contains("\"temporary\":true", payload);
        Assert.Contains("Temp!23456", payload);
    }

    [Fact]
    public async Task ResetTemporaryPasswordAsync_KeepsUpdatePasswordRequiredAction()
    {
        var handler = new RecordingHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""{"access_token":"token-1","expires_in":300}""")
            },
            new HttpResponseMessage(HttpStatusCode.NoContent),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    """
                    {
                      "id": "user-123",
                      "username": "new.user",
                      "email": "new.user@example.com",
                      "firstName": "New",
                      "lastName": "User",
                      "enabled": true,
                      "emailVerified": false,
                      "requiredActions": ["CONFIGURE_TOTP"]
                    }
                    """)
            },
            new HttpResponseMessage(HttpStatusCode.NoContent)
        ]);

        var repository = CreateRepository(handler);

        await repository.ResetTemporaryPasswordAsync("user-123", "Temp!98765");

        Assert.Equal(4, handler.Requests.Count);
        var resetPayload = await handler.Requests[1].Content!.ReadAsStringAsync();
        Assert.Contains("\"temporary\":true", resetPayload);
        Assert.Contains("Temp!98765", resetPayload);

        var updatePayload = await handler.Requests[3].Content!.ReadAsStringAsync();
        Assert.Contains("UPDATE_PASSWORD", updatePayload);
        Assert.Contains("CONFIGURE_TOTP", updatePayload);
    }

    private static KeycloakAdminRepository CreateRepository(RecordingHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new KeycloakAdminSettings
        {
            BaseUrl = "http://keycloak:8080",
            Realm = "tourops",
            ClientId = "tourops-admin-api",
            ClientSecret = "secret"
        });

        return new KeycloakAdminRepository(httpClient, options);
    }

    private static StringContent JsonContent(string body) =>
        new(body, Encoding.UTF8, "application/json");

    private sealed class RecordingHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));
            return Task.FromResult(this.responses.Dequeue());
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content is not null)
            {
                var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                clone.Content = new StringContent(body, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType);
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
