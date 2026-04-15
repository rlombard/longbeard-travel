using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Application.Services;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class AdminUserServiceTests
{
    [Fact]
    public async Task CreateUserAsync_GeneratesTemporaryPasswordAndNormalizesAssignments()
    {
        var repository = new FakeKeycloakAdminRepository();
        var service = new AdminUserService(repository);

        var result = await service.CreateUserAsync(new AdminUserCreateModel
        {
            Username = "operator.one",
            Email = "operator.one@example.com",
            FirstName = "Operator",
            LastName = "One",
            RealmRoleNames = ["operator", "operator"],
            ClientRoles =
            [
                new AdminClientRoleSelectionModel
                {
                    ClientId = "frontend",
                    RoleNames = ["trip-builder", "trip-builder"]
                }
            ]
        });

        Assert.False(string.IsNullOrWhiteSpace(result.TemporaryPassword));
        Assert.Equal(result.TemporaryPassword, repository.LastCreateInput?.TemporaryPassword);
        Assert.Contains("UPDATE_PASSWORD", result.User.RequiredActions);
        Assert.Equal(["operator"], repository.LastRoleUpdate?.RealmRoleNames);
        Assert.Single(repository.LastRoleUpdate?.ClientRoles ?? []);
        Assert.Equal(["trip-builder"], repository.LastRoleUpdate?.ClientRoles[0].RoleNames);
    }

    [Fact]
    public async Task UpdateRolesAsync_RejectsUnknownRole()
    {
        var repository = new FakeKeycloakAdminRepository();
        var service = new AdminUserService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateRolesAsync("existing-user", new AdminUserRoleUpdateModel
            {
                RealmRoleNames = ["missing-role"]
            }));

        Assert.Contains("Unknown realm roles", ex.Message);
    }

    private sealed class FakeKeycloakAdminRepository : IKeycloakAdminRepository
    {
        private readonly Dictionary<string, KeycloakAdminUserRecord> users = [];
        private readonly Dictionary<string, KeycloakAdminUserRoleAssignments> assignments = [];

        public FakeKeycloakAdminRepository()
        {
            users["existing-user"] = new KeycloakAdminUserRecord
            {
                Id = "existing-user",
                Username = "existing-user",
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                Enabled = true,
                EmailVerified = true,
                RequiredActions = ["UPDATE_PASSWORD"],
                CreatedAt = DateTime.UtcNow
            };

            assignments["existing-user"] = new KeycloakAdminUserRoleAssignments
            {
                RealmRoleNames = ["operator"],
                ClientRoles = []
            };
        }

        public KeycloakAdminCreateUserInput? LastCreateInput { get; private set; }
        public AdminUserRoleUpdateModel? LastRoleUpdate { get; private set; }

        public Task<IReadOnlyList<KeycloakAdminUserRecord>> SearchUsersAsync(AdminUserSearchQueryModel query, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<KeycloakAdminUserRecord> result = users.Values.ToList();
            return Task.FromResult(result);
        }

        public Task<KeycloakAdminUserRecord?> GetUserAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(users.TryGetValue(userId, out var user) ? user : null);

        public Task<string> CreateUserAsync(KeycloakAdminCreateUserInput input, CancellationToken cancellationToken = default)
        {
            LastCreateInput = input;
            var userId = "created-user";
            users[userId] = new KeycloakAdminUserRecord
            {
                Id = userId,
                Username = input.Username,
                Email = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Enabled = input.Enabled,
                EmailVerified = input.EmailVerified,
                RequiredActions = ["UPDATE_PASSWORD"],
                CreatedAt = DateTime.UtcNow
            };

            assignments[userId] = new KeycloakAdminUserRoleAssignments
            {
                RealmRoleNames = [],
                ClientRoles = []
            };

            return Task.FromResult(userId);
        }

        public Task UpdateUserAsync(string userId, KeycloakAdminUpdateUserInput input, CancellationToken cancellationToken = default)
        {
            var current = users[userId];
            users[userId] = new KeycloakAdminUserRecord
            {
                Id = current.Id,
                Username = input.Username,
                Email = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Enabled = input.Enabled,
                EmailVerified = input.EmailVerified,
                RequiredActions = current.RequiredActions,
                CreatedAt = current.CreatedAt
            };

            return Task.CompletedTask;
        }

        public Task ResetTemporaryPasswordAsync(string userId, string temporaryPassword, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<KeycloakAdminRoleCatalog> GetAccessCatalogAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new KeycloakAdminRoleCatalog
            {
                RealmRoles =
                [
                    new KeycloakAdminRoleRecord { Name = "admin" },
                    new KeycloakAdminRoleRecord { Name = "operator" }
                ],
                ClientRoles =
                [
                    new KeycloakAdminClientRoleRecord
                    {
                        ClientId = "frontend",
                        ClientInternalId = "frontend-id",
                        DisplayName = "Frontend",
                        Roles =
                        [
                            new KeycloakAdminRoleRecord { Name = "trip-builder" }
                        ]
                    }
                ]
            });

        public Task<KeycloakAdminUserRoleAssignments> GetUserRoleAssignmentsAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(assignments.TryGetValue(userId, out var result)
                ? result
                : new KeycloakAdminUserRoleAssignments());

        public Task ReplaceUserRoleAssignmentsAsync(string userId, AdminUserRoleUpdateModel model, CancellationToken cancellationToken = default)
        {
            LastRoleUpdate = model;
            assignments[userId] = new KeycloakAdminUserRoleAssignments
            {
                RealmRoleNames = model.RealmRoleNames,
                ClientRoles = model.ClientRoles
            };

            return Task.CompletedTask;
        }
    }
}
