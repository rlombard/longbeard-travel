export interface AdminUserSearchQuery {
  search?: string;
  enabled?: boolean;
}

export interface AdminClientRoleSelection {
  clientId: string;
  roleNames: string[];
}

export interface AdminUserListItem {
  id: string;
  username: string;
  email?: string | null;
  firstName: string;
  lastName: string;
  enabled: boolean;
  emailVerified: boolean;
  createdAt?: string | null;
}

export interface AdminUser {
  id: string;
  username: string;
  email?: string | null;
  firstName: string;
  lastName: string;
  enabled: boolean;
  emailVerified: boolean;
  requiredActions: string[];
  realmRoleNames: string[];
  clientRoles: AdminClientRoleSelection[];
  createdAt?: string | null;
}

export interface AdminUserCreateRequest {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  enabled: boolean;
  emailVerified: boolean;
  temporaryPassword?: string | null;
  realmRoleNames: string[];
  clientRoles: AdminClientRoleSelection[];
}

export interface AdminUserUpdateRequest {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  enabled: boolean;
  emailVerified: boolean;
}

export interface AdminUserRoleUpdateRequest {
  realmRoleNames: string[];
  clientRoles: AdminClientRoleSelection[];
}

export interface AdminUserCreateResponse {
  user: AdminUser;
  temporaryPassword: string;
}

export interface AdminResetPasswordRequest {
  temporaryPassword?: string | null;
}

export interface AdminResetPasswordResponse {
  userId: string;
  temporaryPassword: string;
}

export interface AdminRoleCatalogItem {
  name: string;
  description?: string | null;
}

export interface AdminClientRoleCatalogItem {
  clientId: string;
  displayName: string;
  roles: AdminRoleCatalogItem[];
}

export interface AdminAccessCatalog {
  realmRoles: AdminRoleCatalogItem[];
  clientRoles: AdminClientRoleCatalogItem[];
}
