import { Dispatch, SetStateAction, useEffect, useMemo, useState } from 'react';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import {
  AdminAccessCatalog,
  AdminClientRoleCatalogItem,
  AdminClientRoleSelection,
  AdminUser,
  AdminUserCreateRequest,
  AdminUserRoleUpdateRequest,
  AdminUserUpdateRequest
} from '../../types/adminUser';
import {
  useAdminAccessCatalog,
  useAdminUser,
  useAdminUsers,
  useCreateAdminUser,
  useResetAdminUserPassword,
  useUpdateAdminUser,
  useUpdateAdminUserRoles
} from './hooks';
import { formatDateTime } from '../../utils/formatters';

type EditorMode = 'create' | 'edit';
type EnabledFilter = '' | 'true' | 'false';
type DraftSetter<T> = Dispatch<SetStateAction<T>>;

const emptyDraft: AdminUserCreateRequest = {
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  enabled: true,
  emailVerified: false,
  temporaryPassword: '',
  realmRoleNames: [],
  clientRoles: []
};

export const AdminUsersPage = () => {
  const [mode, setMode] = useState<EditorMode>('create');
  const [search, setSearch] = useState('');
  const [enabledFilter, setEnabledFilter] = useState<EnabledFilter>('');
  const [selectedUserId, setSelectedUserId] = useState<string>();
  const [createDraft, setCreateDraft] = useState<AdminUserCreateRequest>(emptyDraft);
  const [editDraft, setEditDraft] = useState<AdminUserUpdateRequest | null>(null);
  const [roleDraft, setRoleDraft] = useState<AdminUserRoleUpdateRequest>({ realmRoleNames: [], clientRoles: [] });
  const [revealedPassword, setRevealedPassword] = useState<{ mode: 'create' | 'reset'; userId: string; password: string } | null>(null);

  const usersQuery = useAdminUsers({
    search: search || undefined,
    enabled: enabledFilter === '' ? undefined : enabledFilter === 'true'
  });
  const accessCatalogQuery = useAdminAccessCatalog();
  const selectedUserQuery = useAdminUser(mode === 'edit' ? selectedUserId : undefined);

  const createMutation = useCreateAdminUser();
  const updateMutation = useUpdateAdminUser();
  const roleMutation = useUpdateAdminUserRoles();
  const resetPasswordMutation = useResetAdminUserPassword();

  useEffect(() => {
    if (mode !== 'edit') {
      return;
    }

    if (!selectedUserId && (usersQuery.data?.length ?? 0) > 0) {
      setSelectedUserId(usersQuery.data![0].id);
    }
  }, [mode, selectedUserId, usersQuery.data]);

  useEffect(() => {
    if (!selectedUserQuery.data) {
      return;
    }

    setEditDraft({
      username: selectedUserQuery.data.username,
      email: selectedUserQuery.data.email ?? '',
      firstName: selectedUserQuery.data.firstName,
      lastName: selectedUserQuery.data.lastName,
      enabled: selectedUserQuery.data.enabled,
      emailVerified: selectedUserQuery.data.emailVerified
    });

    setRoleDraft({
      realmRoleNames: [...selectedUserQuery.data.realmRoleNames],
      clientRoles: selectedUserQuery.data.clientRoles.map((clientRole) => ({
        clientId: clientRole.clientId,
        roleNames: [...clientRole.roleNames]
      }))
    });
  }, [selectedUserQuery.data]);

  const selectedUser = selectedUserQuery.data;
  const accessCatalog = accessCatalogQuery.data;
  const userCount = usersQuery.data?.length ?? 0;

  const activeError = [
    usersQuery.error,
    accessCatalogQuery.error,
    selectedUserQuery.error,
    createMutation.error,
    updateMutation.error,
    roleMutation.error,
    resetPasswordMutation.error
  ].find(Boolean) as Error | undefined;

  const userRows = useMemo(() => usersQuery.data ?? [], [usersQuery.data]);

  return (
    <div className="space-y-6">
      <Card title="User Management">
        <div className="flex flex-wrap items-center gap-2">
          <Badge tone="warning">Admin only</Badge>
          <Badge tone="info">Keycloak source of truth</Badge>
          <Badge tone="success">{userCount} users</Badge>
        </div>
        <p className="mt-3 text-sm text-slate-600">
          Create users, set temporary passwords, force first-login password change, and manage existing realm or client roles without exposing Keycloak admin calls to the browser.
        </p>
      </Card>

      {revealedPassword ? (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 shadow-sm">
          <div className="flex flex-wrap items-center gap-2">
            <Badge tone="warning">{revealedPassword.mode === 'create' ? 'New user password' : 'Temporary reset password'}</Badge>
            <Badge tone="info">Show once</Badge>
          </div>
          <p className="mt-2 text-sm text-slate-700">
            User `{revealedPassword.userId}` temporary password: <span className="font-semibold">{revealedPassword.password}</span>
          </p>
          <p className="mt-1 text-xs text-slate-600">Keycloak also gets required action `UPDATE_PASSWORD`.</p>
          <div className="mt-3">
            <Button className="bg-amber-600 hover:bg-amber-500" onClick={() => setRevealedPassword(null)}>
              Hide password
            </Button>
          </div>
        </div>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
        <Card title="Users">
          <div className="space-y-4">
            <div className="flex flex-wrap items-end gap-3 rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <FormInput label="Search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Username, email, name" />
              <SelectDropdown
                label="Enabled"
                value={enabledFilter}
                options={[
                  { label: 'All', value: '' },
                  { label: 'Enabled only', value: 'true' },
                  { label: 'Disabled only', value: 'false' }
                ]}
                onChange={(event) => setEnabledFilter(event.target.value as EnabledFilter)}
              />
              <div className="flex gap-2">
                <Button
                  onClick={() => {
                    setMode('create');
                    setRevealedPassword(null);
                  }}
                >
                  New user
                </Button>
                <Button
                  className="bg-slate-600 hover:bg-slate-500"
                  onClick={() => {
                    setMode('edit');
                    if (!selectedUserId && userRows.length > 0) {
                      setSelectedUserId(userRows[0].id);
                    }
                  }}
                >
                  Edit selected
                </Button>
              </div>
            </div>

            {usersQuery.isLoading ? <p className="text-sm text-slate-500">Loading users...</p> : null}
            {userRows.length === 0 && !usersQuery.isLoading ? <p className="text-sm text-slate-500">No users match current filters.</p> : null}

            {userRows.length > 0 ? (
              <Table headers={['User', 'Status', 'Email', 'Created', 'Actions']}>
                {userRows.map((user) => {
                  const isSelected = mode === 'edit' && selectedUserId === user.id;

                  return (
                    <tr key={user.id} className={`border-t border-slate-200 ${isSelected ? 'bg-amber-50/60' : ''}`}>
                      <td className="px-3 py-3">
                        <p className="font-medium text-slate-900">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-xs text-slate-500">{user.username}</p>
                      </td>
                      <td className="px-3 py-3">
                        <div className="flex flex-wrap gap-1">
                          <Badge tone={user.enabled ? 'success' : 'danger'}>{user.enabled ? 'Enabled' : 'Disabled'}</Badge>
                          <Badge tone={user.emailVerified ? 'info' : 'warning'}>{user.emailVerified ? 'Email verified' : 'Email unverified'}</Badge>
                        </div>
                      </td>
                      <td className="px-3 py-3 text-slate-700">{user.email ?? '-'}</td>
                      <td className="px-3 py-3 text-slate-700">{formatDateTime(user.createdAt ?? undefined)}</td>
                      <td className="px-3 py-3">
                        <Button
                          className={isSelected ? 'bg-amber-600 hover:bg-amber-500' : 'bg-slate-900 hover:bg-slate-700'}
                          onClick={() => {
                            setMode('edit');
                            setSelectedUserId(user.id);
                            setRevealedPassword(null);
                          }}
                        >
                          Open
                        </Button>
                      </td>
                    </tr>
                  );
                })}
              </Table>
            ) : null}
          </div>
        </Card>

        <div className="space-y-6">
          {mode === 'create' ? (
            <Card title="Create User">
              <div className="space-y-4">
                <UserCoreFields draft={createDraft} onChange={setCreateDraft} includeTemporaryPassword />
                <RoleAssignmentEditor draft={createDraft} onChange={setCreateDraft} accessCatalog={accessCatalog} />
                <div className="flex gap-2">
                  <Button
                    isLoading={createMutation.isPending}
                    onClick={async () => {
                      const result = await createMutation.mutateAsync(createDraft);
                      setCreateDraft(emptyDraft);
                      setMode('edit');
                      setSelectedUserId(result.user.id);
                      setRevealedPassword({ mode: 'create', userId: result.user.id, password: result.temporaryPassword });
                    }}
                  >
                    Create user
                  </Button>
                </div>
              </div>
            </Card>
          ) : (
            <Card title="Edit User">
              {!selectedUserId ? <p className="text-sm text-slate-500">Select a user from the table.</p> : null}
              {selectedUserQuery.isLoading ? <p className="text-sm text-slate-500">Loading user details...</p> : null}
              {selectedUser ? (
                <div className="space-y-6">
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge tone="info">{selectedUser.username}</Badge>
                    {selectedUser.requiredActions.map((action) => (
                      <Badge key={action} tone="warning">
                        {action}
                      </Badge>
                    ))}
                  </div>

                  {editDraft ? (
                    <div className="space-y-4">
                      <UserCoreFields draft={editDraft} onChange={setEditDraft} />
                      <div className="flex flex-wrap gap-2">
                        <Button
                          isLoading={updateMutation.isPending}
                          onClick={async () => {
                            const updated = await updateMutation.mutateAsync({ userId: selectedUser.id, payload: editDraft });
                            setEditDraft({
                              username: updated.username,
                              email: updated.email ?? '',
                              firstName: updated.firstName,
                              lastName: updated.lastName,
                              enabled: updated.enabled,
                              emailVerified: updated.emailVerified
                            });
                          }}
                        >
                          Save profile
                        </Button>
                        <Button
                          className="bg-slate-600 hover:bg-slate-500"
                          isLoading={resetPasswordMutation.isPending}
                          onClick={async () => {
                            const result = await resetPasswordMutation.mutateAsync({ userId: selectedUser.id, payload: {} });
                            setRevealedPassword({ mode: 'reset', userId: result.userId, password: result.temporaryPassword });
                          }}
                        >
                          Reset temp password
                        </Button>
                      </div>
                    </div>
                  ) : null}

                  <div className="border-t border-slate-200 pt-4">
                    <RoleAssignmentEditor draft={roleDraft} onChange={setRoleDraft} accessCatalog={accessCatalog} />
                    <div className="mt-4">
                      <Button
                        isLoading={roleMutation.isPending}
                        onClick={async () => {
                          const updated = await roleMutation.mutateAsync({ userId: selectedUser.id, payload: roleDraft });
                          setRoleDraft({
                            realmRoleNames: [...updated.realmRoleNames],
                            clientRoles: updated.clientRoles.map((clientRole) => ({
                              clientId: clientRole.clientId,
                              roleNames: [...clientRole.roleNames]
                            }))
                          });
                        }}
                      >
                        Save access
                      </Button>
                    </div>
                  </div>
                </div>
              ) : null}
            </Card>
          )}

          {activeError ? (
            <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 shadow-sm">
              <p className="text-sm font-medium text-rose-700">{activeError.message}</p>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
};

type CoreDraft = AdminUserCreateRequest | AdminUserUpdateRequest;

const UserCoreFields = ({
  draft,
  onChange,
  includeTemporaryPassword = false
}: {
  draft: CoreDraft;
  onChange: DraftSetter<any>;
  includeTemporaryPassword?: boolean;
}) => (
  <div className="grid gap-3 md:grid-cols-2">
    <FormInput label="Username" required value={draft.username} onChange={(event) => onChange((current: CoreDraft) => ({ ...current, username: event.target.value }))} />
    <FormInput label="Email" required type="email" value={draft.email} onChange={(event) => onChange((current: CoreDraft) => ({ ...current, email: event.target.value }))} />
    <FormInput label="First Name" required value={draft.firstName} onChange={(event) => onChange((current: CoreDraft) => ({ ...current, firstName: event.target.value }))} />
    <FormInput label="Last Name" required value={draft.lastName} onChange={(event) => onChange((current: CoreDraft) => ({ ...current, lastName: event.target.value }))} />
    {'enabled' in draft ? (
      <label className="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700">
        <input type="checkbox" checked={draft.enabled} onChange={(event) => onChange((current: CoreDraft) => ({ ...current, enabled: event.target.checked }))} />
        Enabled
      </label>
    ) : null}
    {'emailVerified' in draft ? (
      <label className="flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700">
        <input
          type="checkbox"
          checked={draft.emailVerified}
          onChange={(event) => onChange((current: CoreDraft) => ({ ...current, emailVerified: event.target.checked }))}
        />
        Email verified
      </label>
    ) : null}
    {includeTemporaryPassword && 'temporaryPassword' in draft ? (
      <div className="md:col-span-2">
        <FormInput
          label="Temporary Password"
          value={draft.temporaryPassword ?? ''}
          onChange={(event) => onChange((current: AdminUserCreateRequest) => ({ ...current, temporaryPassword: event.target.value }))}
          placeholder="Leave blank to auto-generate"
        />
      </div>
    ) : null}
  </div>
);

const RoleAssignmentEditor = ({
  draft,
  onChange,
  accessCatalog
}: {
  draft: Pick<AdminUserCreateRequest, 'realmRoleNames' | 'clientRoles'>;
  onChange: DraftSetter<any>;
  accessCatalog?: AdminAccessCatalog;
}) => {
  if (!accessCatalog) {
    return <p className="text-sm text-slate-500">Loading access catalog...</p>;
  }

  return (
    <div className="space-y-4">
      <div>
        <div className="mb-2 flex items-center gap-2">
          <h3 className="text-sm font-semibold text-slate-900">Realm roles</h3>
          <Badge tone="info">{draft.realmRoleNames.length} selected</Badge>
        </div>
        <div className="grid gap-2">
          {accessCatalog.realmRoles.map((role) => (
            <label key={role.name} className="flex items-start gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={draft.realmRoleNames.includes(role.name)}
                onChange={() =>
                  onChange((current: Pick<AdminUserCreateRequest, 'realmRoleNames' | 'clientRoles'>) => ({
                    ...current,
                    realmRoleNames: toggleValue(current.realmRoleNames, role.name)
                  }))
                }
              />
              <span>
                <span className="block font-medium text-slate-900">{role.name}</span>
                {role.description ? <span className="block text-xs text-slate-500">{role.description}</span> : null}
              </span>
            </label>
          ))}
        </div>
      </div>

      {accessCatalog.clientRoles.map((clientRoleCatalog) => (
        <ClientRoleEditor key={clientRoleCatalog.clientId} catalogItem={clientRoleCatalog} draft={draft.clientRoles} onChange={onChange} />
      ))}
    </div>
  );
};

const ClientRoleEditor = ({
  catalogItem,
  draft,
  onChange
}: {
  catalogItem: AdminClientRoleCatalogItem;
  draft: AdminClientRoleSelection[];
  onChange: DraftSetter<any>;
}) => {
  const selected = draft.find((item) => item.clientId === catalogItem.clientId);
  const selectedCount = selected?.roleNames.length ?? 0;

  return (
    <div>
      <div className="mb-2 flex items-center gap-2">
        <h3 className="text-sm font-semibold text-slate-900">{catalogItem.displayName}</h3>
        <Badge tone="info">{selectedCount} selected</Badge>
      </div>
      <div className="grid gap-2">
        {catalogItem.roles.map((role) => (
          <label key={`${catalogItem.clientId}-${role.name}`} className="flex items-start gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700">
            <input
              type="checkbox"
              checked={selected?.roleNames.includes(role.name) ?? false}
              onChange={() =>
                onChange((current: Pick<AdminUserCreateRequest, 'realmRoleNames' | 'clientRoles'>) => ({
                  ...current,
                  clientRoles: toggleClientRole(current.clientRoles, catalogItem.clientId, role.name)
                }))
              }
            />
            <span>
              <span className="block font-medium text-slate-900">{role.name}</span>
              {role.description ? <span className="block text-xs text-slate-500">{role.description}</span> : null}
            </span>
          </label>
        ))}
      </div>
    </div>
  );
};

const toggleValue = (values: string[], value: string) =>
  values.includes(value) ? values.filter((item) => item !== value) : [...values, value];

const toggleClientRole = (clientRoles: AdminClientRoleSelection[], clientId: string, roleName: string) => {
  const existing = clientRoles.find((item) => item.clientId === clientId);

  if (!existing) {
    return [...clientRoles, { clientId, roleNames: [roleName] }];
  }

  const roleNames = toggleValue(existing.roleNames, roleName);
  const next = clientRoles.filter((item) => item.clientId !== clientId);

  return roleNames.length === 0 ? next : [...next, { clientId, roleNames }];
};
