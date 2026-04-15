using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Application.Services.Email;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class EmailIntegrationServiceTests
{
    [Fact]
    public async Task CreateConnectionAsync_EncryptsSecretsAndStoresActiveConnection()
    {
        var fixture = new EmailIntegrationFixture();
        fixture.RegisterProvider(new FakeSendProvider(EmailIntegrationProviderType.SmtpDirect));
        var service = fixture.CreateService();

        var result = await service.CreateConnectionAsync(new CreateEmailProviderConnectionModel
        {
            ConnectionName = "Acme SMTP",
            ProviderType = EmailIntegrationProviderType.SmtpDirect,
            AuthMethod = EmailIntegrationAuthMethod.Password,
            MailboxAddress = "ops@acme.test",
            AllowSend = true,
            Settings = new EmailConnectionSettingsInputModel
            {
                OutgoingHost = "smtp.acme.test",
                OutgoingPort = 587,
                OutgoingUseSsl = true,
                OutgoingUsername = "ops@acme.test"
            },
            Secrets = new EmailConnectionSecretInputModel
            {
                OutgoingPassword = "secret-123"
            }
        });

        var stored = fixture.EmailIntegrationRepository.Connections.Single();
        Assert.Equal(EmailIntegrationStatus.Active, result.Status);
        Assert.NotNull(stored.EncryptedCredentialsJson);
        Assert.DoesNotContain("secret-123", stored.EncryptedCredentialsJson!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StartOAuthAsync_CreatesPendingConnectionAndReturnsAuthorizationUrl()
    {
        var fixture = new EmailIntegrationFixture();
        fixture.RegisterProvider(new FakeOAuthProvider(EmailIntegrationProviderType.Gmail));
        var service = fixture.CreateService();

        var result = await service.StartOAuthAsync(new StartEmailProviderOAuthModel
        {
            ConnectionName = "Google Ops",
            ProviderType = EmailIntegrationProviderType.Gmail,
            MailboxAddress = "ops@gmail.test",
            AllowSend = true,
            AllowSync = true,
            ReturnUrl = "https://ui.test/settings/email"
        });

        var stored = fixture.EmailIntegrationRepository.Connections.Single();
        Assert.Equal(EmailIntegrationStatus.PendingAuthorization, stored.Status);
        Assert.Equal(EmailIntegrationAuthMethod.OAuth2, stored.AuthMethod);
        Assert.Equal(stored.Id, result.ConnectionId);
        Assert.Contains(stored.OAuthState!, result.AuthorizationUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SyncConnectionAsync_ImportsThenDeduplicatesMessages()
    {
        var fixture = new EmailIntegrationFixture();
        fixture.RegisterProvider(new FakeSyncProvider(EmailIntegrationProviderType.GenericImapSmtp));
        var connection = fixture.AddConnection(new EmailProviderConnection
        {
            Id = Guid.NewGuid(),
            OwnerUserId = fixture.CurrentUserContext.UserId,
            ConnectionName = "Inbox",
            ProviderType = EmailIntegrationProviderType.GenericImapSmtp,
            AuthMethod = EmailIntegrationAuthMethod.Password,
            Status = EmailIntegrationStatus.Active,
            MailboxAddress = "ops@acme.test",
            AllowSync = true,
            AllowSend = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var service = fixture.CreateService();

        var first = await service.SyncConnectionAsync(connection.Id);
        var second = await service.SyncConnectionAsync(connection.Id);

        Assert.Equal(1, first.MessagesImported);
        Assert.Equal(0, first.MessagesSkipped);
        Assert.Equal(0, second.MessagesImported);
        Assert.Equal(1, second.MessagesSkipped);
        Assert.Single(fixture.EmailRepository.Threads);
        Assert.Single(fixture.EmailRepository.Messages);
        Assert.Single(fixture.EmailIntegrationRepository.MessageLinks);
    }

    [Fact]
    public async Task SendMessageAsync_UsesProviderAndUpdatesConnectionState()
    {
        var fixture = new EmailIntegrationFixture();
        fixture.RegisterProvider(new FakeSendProvider(EmailIntegrationProviderType.SendGrid));
        var connection = fixture.AddConnection(new EmailProviderConnection
        {
            Id = Guid.NewGuid(),
            OwnerUserId = fixture.CurrentUserContext.UserId,
            ConnectionName = "Outbound",
            ProviderType = EmailIntegrationProviderType.SendGrid,
            AuthMethod = EmailIntegrationAuthMethod.ApiKey,
            Status = EmailIntegrationStatus.Active,
            MailboxAddress = "sales@acme.test",
            AllowSend = true,
            AllowSync = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var service = fixture.CreateService();

        var result = await service.SendMessageAsync(connection.Id, new SendConnectedEmailMessageModel
        {
            Subject = "Welcome",
            BodyText = "hello",
            ToAddresses = ["guest@example.com"]
        });

        var stored = fixture.EmailIntegrationRepository.Connections.Single();
        Assert.Equal("provider-msg-1", result.ProviderMessageId);
        Assert.NotNull(stored.LastSuccessfulSendAt);
        Assert.Equal(EmailIntegrationStatus.Active, stored.Status);
    }

    private sealed class EmailIntegrationFixture
    {
        public FakeEmailIntegrationRepository EmailIntegrationRepository { get; } = new();
        public FakeEmailRepository EmailRepository { get; } = new();
        public FakeCurrentUserContext CurrentUserContext { get; } = new();

        private readonly Dictionary<EmailIntegrationProviderType, IEmailIntegrationProvider> providers = [];

        public EmailIntegrationService CreateService() =>
            new(
                EmailIntegrationRepository,
                EmailRepository,
                CurrentUserContext,
                new FakeSecretProtector(),
                new FakeProviderResolver(providers));

        public void RegisterProvider(IEmailIntegrationProvider provider) => providers[provider.ProviderType] = provider;

        public EmailProviderConnection AddConnection(EmailProviderConnection connection)
        {
            EmailIntegrationRepository.Connections.Add(connection);
            return connection;
        }
    }

    private sealed class FakeEmailIntegrationRepository : IEmailIntegrationRepository
    {
        public List<EmailProviderConnection> Connections { get; } = [];
        public List<EmailProviderMessageLink> MessageLinks { get; } = [];

        public Task<EmailProviderConnection> AddConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default)
        {
            Connections.Add(connection);
            if (connection.IsDefaultConnection)
            {
                ClearOtherDefaults(connection);
            }

            return Task.FromResult(connection);
        }

        public Task<EmailProviderMessageLink> AddMessageLinkAsync(EmailProviderMessageLink link, CancellationToken cancellationToken = default)
        {
            MessageLinks.Add(link);
            return Task.FromResult(link);
        }

        public Task<EmailProviderConnection?> GetByOAuthStateAsync(string oauthState, CancellationToken cancellationToken = default) =>
            Task.FromResult(Connections.FirstOrDefault(x => x.OAuthState == oauthState));

        public Task<IReadOnlyList<EmailProviderConnection>> GetByWebhookSubscriptionIdsAsync(IEnumerable<string> subscriptionIds, CancellationToken cancellationToken = default)
        {
            var ids = subscriptionIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<EmailProviderConnection>>(Connections.Where(x => x.WebhookSubscriptionId is not null && ids.Contains(x.WebhookSubscriptionId)).ToList());
        }

        public Task<EmailProviderConnection?> GetConnectionByIdAsync(Guid connectionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Connections.FirstOrDefault(x => x.Id == connectionId));

        public Task<IReadOnlyList<EmailProviderConnection>> GetConnectionsByOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmailProviderConnection>>(Connections.Where(x => x.OwnerUserId == ownerUserId).ToList());

        public Task<EmailProviderConnection?> GetDefaultSendConnectionAsync(string ownerUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Connections.Where(x => x.OwnerUserId == ownerUserId && x.AllowSend && x.Status == EmailIntegrationStatus.Active)
                .OrderByDescending(x => x.IsDefaultConnection)
                .ThenByDescending(x => x.UpdatedAt)
                .FirstOrDefault());

        public Task<IReadOnlyList<EmailProviderConnection>> GetDueForSyncAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmailProviderConnection>>(Connections
                .Where(x => x.AllowSync && x.Status == EmailIntegrationStatus.Active && x.NextSyncAt.HasValue && x.NextSyncAt <= utcNow)
                .ToList());

        public Task<EmailProviderMessageLink?> GetMessageLinkAsync(Guid connectionId, string providerMessageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(MessageLinks.FirstOrDefault(x => x.EmailProviderConnectionId == connectionId && x.ProviderMessageId == providerMessageId));

        public Task UpdateConnectionAsync(EmailProviderConnection connection, CancellationToken cancellationToken = default)
        {
            var index = Connections.FindIndex(x => x.Id == connection.Id);
            if (index >= 0)
            {
                Connections[index] = connection;
            }
            else
            {
                Connections.Add(connection);
            }

            if (connection.IsDefaultConnection)
            {
                ClearOtherDefaults(connection);
            }

            return Task.CompletedTask;
        }

        private void ClearOtherDefaults(EmailProviderConnection connection)
        {
            foreach (var sibling in Connections.Where(x => x.OwnerUserId == connection.OwnerUserId && x.Id != connection.Id))
            {
                sibling.IsDefaultConnection = false;
            }
        }
    }

    private sealed class FakeEmailRepository : IEmailRepository
    {
        public List<EmailThread> Threads { get; } = [];
        public List<EmailMessage> Messages { get; } = [];

        public Task<EmailDraft> AddDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default) => Task.FromResult(draft);

        public Task<EmailMessage> AddMessageAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.FromResult(message);
        }

        public Task<EmailThread> AddThreadAsync(EmailThread thread, CancellationToken cancellationToken = default)
        {
            Threads.Add(thread);
            return Task.FromResult(thread);
        }

        public Task<EmailDraft?> GetDraftByIdAsync(Guid draftId, CancellationToken cancellationToken = default) => Task.FromResult<EmailDraft?>(null);

        public Task<EmailMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Messages.FirstOrDefault(x => x.Id == messageId));

        public Task<EmailThread?> GetThreadByExternalThreadIdAsync(string externalThreadId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Threads.FirstOrDefault(x => x.ExternalThreadId == externalThreadId));

        public Task<EmailThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Threads.FirstOrDefault(x => x.Id == threadId));

        public Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmailThread>>(Threads.ToList());

        public Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmailThread>>(Threads.Where(x => x.BookingId == bookingId).ToList());

        public Task UpdateDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateMessageAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateThreadAsync(EmailThread thread, CancellationToken cancellationToken = default)
        {
            var index = Threads.FindIndex(x => x.Id == thread.Id);
            if (index >= 0)
            {
                Threads[index] = thread;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public string UserId { get; } = "user-1";
        public string GetRequiredUserId() => UserId;
    }

    private sealed class FakeSecretProtector : IEmailConnectionSecretProtector
    {
        public string Protect(string plaintext) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plaintext));
        public string Unprotect(string protectedValue) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(protectedValue));
    }

    private sealed class FakeProviderResolver(Dictionary<EmailIntegrationProviderType, IEmailIntegrationProvider> providers) : IEmailIntegrationProviderResolver
    {
        public IEmailIntegrationProvider GetRequiredProvider(EmailIntegrationProviderType providerType) =>
            providers.TryGetValue(providerType, out var provider)
                ? provider
                : throw new InvalidOperationException($"Provider '{providerType}' not registered.");
    }

    private sealed class FakeOAuthProvider(EmailIntegrationProviderType providerType) : IEmailOAuthProvider
    {
        public EmailIntegrationProviderType ProviderType => providerType;
        public bool SupportsOAuth => true;
        public bool SupportsSend => false;
        public bool SupportsSync => false;
        public bool SupportsWebhook => false;

        public Task<EmailAuthorizationCompletionResult> CompleteAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmailAuthorizationCompletionResult
            {
                MailboxAddress = connection.MailboxAddress,
                DisplayName = connection.DisplayName,
                ExternalAccountId = "external-1",
                Secrets = new EmailConnectionResolvedSecrets
                {
                    AccessToken = "token-1",
                    RefreshToken = "refresh-1",
                    TokenType = "Bearer"
                },
                AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        public Task<EmailOAuthStartResultModel> StartAuthorizationAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmailOAuthStartResultModel
            {
                ConnectionId = connection.Id,
                State = connection.OAuthState ?? string.Empty,
                AuthorizationUrl = $"https://oauth.test/authorize?state={connection.OAuthState}"
            });
    }

    private sealed class FakeSendProvider(EmailIntegrationProviderType providerType) : IEmailSendProvider
    {
        public EmailIntegrationProviderType ProviderType => providerType;
        public bool SupportsOAuth => false;
        public bool SupportsSend => true;
        public bool SupportsSync => false;
        public bool SupportsWebhook => false;

        public Task<EmailProviderSendResult> SendAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, EmailProviderSendRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmailProviderSendResult
            {
                ProviderMessageId = "provider-msg-1",
                ProviderThreadId = "provider-thread-1",
                MetadataJson = "{\"provider\":\"fake\"}"
            });

        public Task TestSendCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeSyncProvider(EmailIntegrationProviderType providerType) : IEmailSyncProvider
    {
        public EmailIntegrationProviderType ProviderType => providerType;
        public bool SupportsOAuth => false;
        public bool SupportsSend => false;
        public bool SupportsSync => true;
        public bool SupportsWebhook => false;

        public Task<EmailSyncEnvelope> SyncAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmailSyncEnvelope
            {
                Messages =
                [
                    new EmailSyncedMessageModel
                    {
                        ProviderMessageId = "provider-in-1",
                        ProviderThreadId = "thread-1",
                        FolderName = "Inbox",
                        Subject = "Supplier reply",
                        BodyText = "confirmed",
                        Sender = "supplier@example.com",
                        Recipients = "ops@acme.test",
                        SentAtUtc = new DateTime(2026, 4, 15, 8, 0, 0, DateTimeKind.Utc)
                    }
                ],
                NextCursorJson = "{\"cursor\":\"next-1\"}"
            });

        public Task TestSyncCapabilityAsync(EmailProviderConnection connection, EmailConnectionResolvedSettings settings, EmailConnectionResolvedSecrets secrets, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
