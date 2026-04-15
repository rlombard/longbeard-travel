using AI.Forged.TourOps.Application.Models.AdminUsers;

namespace AI.Forged.TourOps.Api.Models;

public static class AdminUserMappings
{
    public static AdminUserCreateModel ToModel(this AdminUserCreateRequest request) => new()
    {
        Username = request.Username,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Enabled = request.Enabled,
        EmailVerified = request.EmailVerified,
        TemporaryPassword = request.TemporaryPassword,
        RealmRoleNames = request.RealmRoleNames,
        ClientRoles = request.ClientRoles.Select(x => x.ToModel()).ToList()
    };

    public static AdminUserUpdateModel ToModel(this AdminUserUpdateRequest request) => new()
    {
        Username = request.Username,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Enabled = request.Enabled,
        EmailVerified = request.EmailVerified
    };

    public static AdminUserRoleUpdateModel ToModel(this AdminUserRoleUpdateRequest request) => new()
    {
        RealmRoleNames = request.RealmRoleNames,
        ClientRoles = request.ClientRoles.Select(x => x.ToModel()).ToList()
    };

    public static AdminClientRoleSelectionModel ToModel(this AdminClientRoleSelectionRequest request) => new()
    {
        ClientId = request.ClientId,
        RoleNames = request.RoleNames
    };

    public static AdminUserResetPasswordModel ToModel(this AdminResetPasswordRequest request) => new()
    {
        TemporaryPassword = request.TemporaryPassword
    };

    public static AdminUserListResponse ToResponse(this AdminUserListItemModel model) => new()
    {
        Id = model.Id,
        Username = model.Username,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        Enabled = model.Enabled,
        EmailVerified = model.EmailVerified,
        CreatedAt = model.CreatedAt
    };

    public static AdminUserResponse ToResponse(this AdminUserModel model) => new()
    {
        Id = model.Id,
        Username = model.Username,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        Enabled = model.Enabled,
        EmailVerified = model.EmailVerified,
        RequiredActions = model.RequiredActions.ToList(),
        RealmRoleNames = model.RealmRoleNames.ToList(),
        ClientRoles = model.ClientRoles.Select(x => new AdminClientRoleSelectionResponse
        {
            ClientId = x.ClientId,
            RoleNames = x.RoleNames.ToList()
        }).ToList(),
        CreatedAt = model.CreatedAt
    };

    public static AdminUserCreateResponse ToResponse(this AdminUserCreateResultModel model) => new()
    {
        User = model.User.ToResponse(),
        TemporaryPassword = model.TemporaryPassword
    };

    public static AdminResetPasswordResponse ToResponse(this AdminUserResetPasswordResultModel model) => new()
    {
        UserId = model.UserId,
        TemporaryPassword = model.TemporaryPassword
    };

    public static AdminAccessCatalogResponse ToResponse(this AdminAccessCatalogModel model) => new()
    {
        RealmRoles = model.RealmRoles.Select(x => new AdminRoleCatalogResponse
        {
            Name = x.Name,
            Description = x.Description
        }).ToList(),
        ClientRoles = model.ClientRoles.Select(x => new AdminClientRoleCatalogResponse
        {
            ClientId = x.ClientId,
            DisplayName = x.DisplayName,
            Roles = x.Roles.Select(y => new AdminRoleCatalogResponse
            {
                Name = y.Name,
                Description = y.Description
            }).ToList()
        }).ToList()
    };
}
