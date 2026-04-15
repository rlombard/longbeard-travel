import { useEffect, useMemo, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import {
  useCreateTenantWorkspaceUser,
  useDisconnectTenantEmailConnection,
  useResetTenantWorkspaceUserPassword,
  useStartTenantEmailOAuth,
  useSyncTenantEmailConnection,
  useTenantAdminWorkspace,
  useTestTenantEmailConnection,
  useUpdateTenantAdminWorkspace,
  useUpdateTenantWorkspaceUser,
  useUpsertTenantAdminConfig
} from './hooks';
import { TenantUserRole } from '../../types/platform';
import { CreateTenantWorkspaceUserRequest, UpdateTenantWorkspaceProfileRequest, UpdateTenantWorkspaceUserRequest } from '../../types/tenantAdmin';
import { EmailIntegrationProviderType, StartEmailProviderOAuthRequest } from '../../types/emailIntegration';
import { formatDateTime } from '../../utils/formatters';

const createUserDefaults: CreateTenantWorkspaceUserRequest = {
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  role: 'Operator',
  temporaryPassword: ''
};

const emailConnectDefaults: StartEmailProviderOAuthRequest = {
  connectionName: '',
  providerType: 'Microsoft365',
  mailboxAddress: '',
  displayName: '',
  allowSend: true,
  allowSync: true,
  isDefaultConnection: true,
  settings: {}
};

const advancedConfigDefaults = {
  configDomain: 'email',
  configKey: 'defaultSenderName',
  jsonValue: '"Operations Desk"',
  isEncrypted: false
};

const profileDefaults: UpdateTenantWorkspaceProfileRequest = {
  tenantName: '',
  legalName: '',
  defaultCurrency: 'USD',
  timeZone: 'UTC',
  billingEmail: '',
  notes: ''
};

export const TenantSettingsPage = () => {
  const location = useLocation();
  const workspaceQuery = useTenantAdminWorkspace();
  const updateWorkspaceMutation = useUpdateTenantAdminWorkspace();
  const configMutation = useUpsertTenantAdminConfig();
  const createUserMutation = useCreateTenantWorkspaceUser();
  const updateUserMutation = useUpdateTenantWorkspaceUser();
  const resetPasswordMutation = useResetTenantWorkspaceUserPassword();
  const startOAuthMutation = useStartTenantEmailOAuth();
  const testEmailMutation = useTestTenantEmailConnection();
  const syncEmailMutation = useSyncTenantEmailConnection();
  const disconnectEmailMutation = useDisconnectTenantEmailConnection();

  const [workspaceDraft, setWorkspaceDraft] = useState({
    supportEmail: '',
    defaultSenderName: '',
    replyToAddress: '',
    signatureHtml: ''
  });
  const [profileDraft, setProfileDraft] = useState<UpdateTenantWorkspaceProfileRequest>(profileDefaults);
  const [emailConnectDraft, setEmailConnectDraft] = useState<StartEmailProviderOAuthRequest>(emailConnectDefaults);
  const [createUserDraft, setCreateUserDraft] = useState<CreateTenantWorkspaceUserRequest>(createUserDefaults);
  const [selectedUserId, setSelectedUserId] = useState<string>();
  const [editUserDraft, setEditUserDraft] = useState<UpdateTenantWorkspaceUserRequest | null>(null);
  const [resetPasswordDraft, setResetPasswordDraft] = useState('');
  const [advancedConfigDraft, setAdvancedConfigDraft] = useState(advancedConfigDefaults);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [revealedPassword, setRevealedPassword] = useState<{ userId: string; password: string } | null>(null);

  useEffect(() => {
    const workspace = workspaceQuery.data;
    if (!workspace) {
      return;
    }

    setWorkspaceDraft({
      supportEmail: readStringConfig(workspace.configEntries, 'branding', 'supportEmail') || workspace.billingEmail || '',
      defaultSenderName: readStringConfig(workspace.configEntries, 'email', 'defaultSenderName'),
      replyToAddress: readStringConfig(workspace.configEntries, 'email', 'replyToAddress'),
      signatureHtml: readStringConfig(workspace.configEntries, 'email', 'signatureHtml')
    });
    setProfileDraft({
      tenantName: workspace.tenantName,
      legalName: workspace.legalName || '',
      defaultCurrency: workspace.defaultCurrency,
      timeZone: workspace.timeZone,
      billingEmail: workspace.billingEmail || '',
      notes: workspace.notes || ''
    });

    setEmailConnectDraft((current) => ({
      ...current,
      connectionName: current.connectionName || `${workspace.tenantName} Mailbox`,
      mailboxAddress: current.mailboxAddress || workspace.billingEmail || '',
      displayName: current.displayName || workspace.tenantName
    }));

    if (!selectedUserId && workspace.users.length > 0) {
      setSelectedUserId(workspace.users[0].userId);
    }
  }, [selectedUserId, workspaceQuery.data]);

  const selectedUser = useMemo(
    () => workspaceQuery.data?.users.find((user) => user.userId === selectedUserId),
    [selectedUserId, workspaceQuery.data?.users]
  );

  useEffect(() => {
    if (!selectedUser) {
      setEditUserDraft(null);
      return;
    }

    setEditUserDraft({
      email: selectedUser.email,
      firstName: selectedUser.firstName,
      lastName: selectedUser.lastName,
      role: selectedUser.role,
      enabled: selectedUser.enabled
    });
  }, [selectedUser]);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    if (params.get('status') === 'connected') {
      setStatusMessage('Email account connected. Tenant mailbox is ready.');
    }
  }, [location.search]);

  const activeError = [
    workspaceQuery.error,
    updateWorkspaceMutation.error,
    configMutation.error,
    createUserMutation.error,
    updateUserMutation.error,
    resetPasswordMutation.error,
    startOAuthMutation.error,
    testEmailMutation.error,
    syncEmailMutation.error,
    disconnectEmailMutation.error
  ].find(Boolean) as Error | undefined;

  if (workspaceQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading tenant settings...</p>;
  }

  if (workspaceQuery.isError) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 shadow-sm">
        <p className="text-sm font-semibold text-rose-700">Tenant settings failed to load</p>
        <p className="mt-2 text-sm text-rose-600">{(workspaceQuery.error as Error).message}</p>
      </div>
    );
  }

  if (!workspaceQuery.data) {
    return <p className="text-sm text-slate-500">Tenant settings not available.</p>;
  }

  const workspace = workspaceQuery.data;

  return (
    <div className="space-y-6">
      <Card title="Tenant Settings">
        <div className="flex flex-wrap items-center gap-2">
          <Badge tone="warning">Tenant admin</Badge>
          <Badge tone="info">{workspace.tenantName}</Badge>
          <Badge tone="neutral">{workspace.realmName}</Badge>
        </div>
        <p className="mt-3 text-sm text-slate-600">
          Manage tenant email defaults, connected mailboxes, and tenant users. Normal operators do not see this area.
        </p>
      </Card>

      {statusMessage ? (
        <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 shadow-sm">
          <p className="text-sm font-medium text-emerald-700">{statusMessage}</p>
        </div>
      ) : null}

      {revealedPassword ? (
        <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 shadow-sm">
          <div className="flex flex-wrap items-center gap-2">
            <Badge tone="warning">Temporary password</Badge>
            <Badge tone="info">Show once</Badge>
          </div>
          <p className="mt-2 text-sm text-slate-700">
            User `{revealedPassword.userId}` temporary password: <span className="font-semibold">{revealedPassword.password}</span>
          </p>
          <div className="mt-3">
            <Button className="bg-amber-600 hover:bg-amber-500" onClick={() => setRevealedPassword(null)}>
              Hide password
            </Button>
          </div>
        </div>
      ) : null}

      {activeError ? (
        <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 shadow-sm">
          <p className="text-sm font-medium text-rose-700">{activeError.message}</p>
        </div>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
        <Card title="Workspace Profile">
          <div className="grid gap-4 md:grid-cols-2">
            <Info label="Tenant">{workspace.tenantName}</Info>
            <Info label="Tenant Code">{workspace.tenantSlug}</Info>
            <Info label="Default Currency">{workspace.defaultCurrency}</Info>
            <Info label="Time Zone">{workspace.timeZone}</Info>
            <Info label="Billing Email">{workspace.billingEmail || '-'}</Info>
            <Info label="Identity Realm">{workspace.realmName}</Info>
          </div>

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <FormInput
              label="Tenant Name"
              value={profileDraft.tenantName}
              onChange={(event) => setProfileDraft((current) => ({ ...current, tenantName: event.target.value }))}
            />
            <FormInput
              label="Legal Name"
              value={profileDraft.legalName ?? ''}
              onChange={(event) => setProfileDraft((current) => ({ ...current, legalName: event.target.value }))}
            />
            <FormInput
              label="Default Currency"
              value={profileDraft.defaultCurrency}
              onChange={(event) => setProfileDraft((current) => ({ ...current, defaultCurrency: event.target.value.toUpperCase() }))}
            />
            <FormInput
              label="Time Zone"
              value={profileDraft.timeZone}
              onChange={(event) => setProfileDraft((current) => ({ ...current, timeZone: event.target.value }))}
            />
            <FormInput
              label="Billing Email"
              value={profileDraft.billingEmail ?? ''}
              onChange={(event) => setProfileDraft((current) => ({ ...current, billingEmail: event.target.value }))}
            />
            <div />
            <div className="md:col-span-2">
              <TextAreaField
                label="Internal Notes"
                rows={3}
                value={profileDraft.notes ?? ''}
                onChange={(event) => setProfileDraft((current) => ({ ...current, notes: event.target.value }))}
              />
            </div>
          </div>

          <div className="mt-4">
            <Button
              isLoading={updateWorkspaceMutation.isPending}
              onClick={async () => {
                await updateWorkspaceMutation.mutateAsync(profileDraft);
                setStatusMessage('Tenant profile saved.');
              }}
            >
              Save tenant profile
            </Button>
          </div>

          <div className="mt-8 grid gap-4 md:grid-cols-2">
            <FormInput
              label="Support Email"
              value={workspaceDraft.supportEmail}
              onChange={(event) => setWorkspaceDraft((current) => ({ ...current, supportEmail: event.target.value }))}
            />
            <FormInput
              label="Default Sender Name"
              value={workspaceDraft.defaultSenderName}
              onChange={(event) => setWorkspaceDraft((current) => ({ ...current, defaultSenderName: event.target.value }))}
            />
            <FormInput
              label="Reply-To Address"
              value={workspaceDraft.replyToAddress}
              onChange={(event) => setWorkspaceDraft((current) => ({ ...current, replyToAddress: event.target.value }))}
            />
            <div />
            <div className="md:col-span-2">
              <TextAreaField
                label="Email Signature"
                rows={4}
                value={workspaceDraft.signatureHtml}
                onChange={(event) => setWorkspaceDraft((current) => ({ ...current, signatureHtml: event.target.value }))}
              />
            </div>
          </div>

          <div className="mt-4">
            <Button
              isLoading={configMutation.isPending}
              onClick={async () => {
                await Promise.all([
                  configMutation.mutateAsync({ configDomain: 'branding', configKey: 'supportEmail', jsonValue: JSON.stringify(workspaceDraft.supportEmail), isEncrypted: false }),
                  configMutation.mutateAsync({ configDomain: 'email', configKey: 'defaultSenderName', jsonValue: JSON.stringify(workspaceDraft.defaultSenderName), isEncrypted: false }),
                  configMutation.mutateAsync({ configDomain: 'email', configKey: 'replyToAddress', jsonValue: JSON.stringify(workspaceDraft.replyToAddress), isEncrypted: false }),
                  configMutation.mutateAsync({ configDomain: 'email', configKey: 'signatureHtml', jsonValue: JSON.stringify(workspaceDraft.signatureHtml), isEncrypted: false })
                ]);
                setStatusMessage('Tenant email defaults saved.');
              }}
            >
              Save workspace config
            </Button>
          </div>
        </Card>

        <Card title="Connect Email System">
          <div className="grid gap-4 md:grid-cols-2">
            <FormInput
              label="Connection Name"
              value={emailConnectDraft.connectionName}
              onChange={(event) => setEmailConnectDraft((current) => ({ ...current, connectionName: event.target.value }))}
            />
            <SelectDropdown
              label="Provider"
              value={emailConnectDraft.providerType}
              options={[
                { label: 'Microsoft 365', value: 'Microsoft365' },
                { label: 'Gmail', value: 'Gmail' }
              ]}
              onChange={(event) => setEmailConnectDraft((current) => ({ ...current, providerType: event.target.value as EmailIntegrationProviderType }))}
            />
            <FormInput
              label="Mailbox Address"
              value={emailConnectDraft.mailboxAddress}
              onChange={(event) => setEmailConnectDraft((current) => ({ ...current, mailboxAddress: event.target.value }))}
            />
            <FormInput
              label="Display Name"
              value={emailConnectDraft.displayName ?? ''}
              onChange={(event) => setEmailConnectDraft((current) => ({ ...current, displayName: event.target.value }))}
            />
          </div>

          <div className="mt-4 flex flex-wrap gap-4">
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={emailConnectDraft.allowSend}
                onChange={(event) => setEmailConnectDraft((current) => ({ ...current, allowSend: event.target.checked }))}
              />
              Allow send
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={emailConnectDraft.allowSync}
                onChange={(event) => setEmailConnectDraft((current) => ({ ...current, allowSync: event.target.checked }))}
              />
              Allow sync
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={emailConnectDraft.isDefaultConnection}
                onChange={(event) => setEmailConnectDraft((current) => ({ ...current, isDefaultConnection: event.target.checked }))}
              />
              Default mailbox
            </label>
          </div>

          <div className="mt-4">
            <Button
              isLoading={startOAuthMutation.isPending}
              onClick={async () => {
                const result = await startOAuthMutation.mutateAsync({
                  ...emailConnectDraft,
                  returnUrl: `${window.location.origin}/app/settings`
                });
                window.location.assign(result.authorizationUrl);
              }}
            >
              Start OAuth connect
            </Button>
          </div>
        </Card>
      </div>

      <Card title="Connected Mailboxes">
        {workspace.emailConnections.length === 0 ? <p className="text-sm text-slate-500">No tenant mailboxes connected yet.</p> : null}
        {workspace.emailConnections.length > 0 ? (
          <Table headers={['Mailbox', 'Provider', 'Status', 'Last Activity', 'Actions']}>
            {workspace.emailConnections.map((connection) => (
              <tr key={connection.id} className="border-t border-slate-200">
                <td className="px-3 py-3">
                  <p className="font-medium text-slate-900">{connection.connectionName}</p>
                  <p className="text-xs text-slate-500">{connection.mailboxAddress}</p>
                </td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-1">
                    <Badge tone="info">{connection.providerType}</Badge>
                    {connection.isDefaultConnection ? <Badge tone="success">Default</Badge> : null}
                  </div>
                </td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-1">
                    <StatusBadge status={connection.status} />
                    {connection.allowSync ? <Badge tone="neutral">Sync</Badge> : null}
                    {connection.allowSend ? <Badge tone="neutral">Send</Badge> : null}
                  </div>
                  {connection.lastError ? <p className="mt-1 text-xs text-rose-600">{connection.lastError}</p> : null}
                </td>
                <td className="px-3 py-3 text-slate-700">
                  <p>Tested: {formatDateTime(connection.lastTestedAt)}</p>
                  <p>Synced: {formatDateTime(connection.lastSyncedAt)}</p>
                </td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-2">
                    <Button className="bg-sky-700 hover:bg-sky-600" onClick={() => testEmailMutation.mutate(connection.id)}>
                      Test
                    </Button>
                    <Button className="bg-emerald-700 hover:bg-emerald-600" onClick={() => syncEmailMutation.mutate(connection.id)}>
                      Sync
                    </Button>
                    <Button className="bg-rose-700 hover:bg-rose-600" onClick={() => disconnectEmailMutation.mutate(connection.id)}>
                      Disconnect
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </Table>
        ) : null}
      </Card>

      <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
        <Card title="Tenant Users">
          {workspace.users.length === 0 ? <p className="text-sm text-slate-500">No tenant users found.</p> : null}
          {workspace.users.length > 0 ? (
            <Table headers={['User', 'Role', 'Status', 'Last Seen', 'Actions']}>
              {workspace.users.map((user) => (
                <tr key={user.userId} className={`border-t border-slate-200 ${selectedUserId === user.userId ? 'bg-amber-50/60' : ''}`}>
                  <td className="px-3 py-3">
                    <p className="font-medium text-slate-900">{user.displayName}</p>
                    <p className="text-xs text-slate-500">{user.email}</p>
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex flex-wrap gap-1">
                      <RoleBadge role={user.role} />
                    </div>
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex flex-wrap gap-1">
                      <Badge tone={user.enabled ? 'success' : 'danger'}>{user.enabled ? 'Enabled' : 'Disabled'}</Badge>
                      <StatusBadge status={user.status} />
                    </div>
                  </td>
                  <td className="px-3 py-3 text-slate-700">{formatDateTime(user.lastSeenAt)}</td>
                  <td className="px-3 py-3">
                    <Button
                      className={selectedUserId === user.userId ? 'bg-amber-600 hover:bg-amber-500' : 'bg-slate-900 hover:bg-slate-700'}
                      onClick={() => setSelectedUserId(user.userId)}
                    >
                      Manage
                    </Button>
                  </td>
                </tr>
              ))}
            </Table>
          ) : null}
        </Card>

        <div className="space-y-6">
          <Card title="Create Tenant User">
            <div className="grid gap-4 md:grid-cols-2">
              <FormInput label="Username" value={createUserDraft.username} onChange={(event) => setCreateUserDraft((current) => ({ ...current, username: event.target.value }))} />
              <FormInput label="Email" value={createUserDraft.email} onChange={(event) => setCreateUserDraft((current) => ({ ...current, email: event.target.value }))} />
              <FormInput label="First Name" value={createUserDraft.firstName} onChange={(event) => setCreateUserDraft((current) => ({ ...current, firstName: event.target.value }))} />
              <FormInput label="Last Name" value={createUserDraft.lastName} onChange={(event) => setCreateUserDraft((current) => ({ ...current, lastName: event.target.value }))} />
              <SelectDropdown
                label="Role"
                value={createUserDraft.role}
                options={[
                  { label: 'Operator', value: 'Operator' },
                  { label: 'Tenant Admin', value: 'TenantAdmin' }
                ]}
                onChange={(event) => setCreateUserDraft((current) => ({ ...current, role: event.target.value as TenantUserRole }))}
              />
              <FormInput
                label="Temporary Password"
                value={createUserDraft.temporaryPassword ?? ''}
                onChange={(event) => setCreateUserDraft((current) => ({ ...current, temporaryPassword: event.target.value }))}
                placeholder="Leave blank to auto-generate"
              />
            </div>
            <div className="mt-4">
              <Button
                isLoading={createUserMutation.isPending}
                onClick={async () => {
                  const result = await createUserMutation.mutateAsync(createUserDraft);
                  setRevealedPassword({ userId: result.user.userId, password: result.temporaryPassword });
                  setCreateUserDraft(createUserDefaults);
                  setSelectedUserId(result.user.userId);
                  setStatusMessage('Tenant user created.');
                }}
              >
                Create user
              </Button>
            </div>
          </Card>

          <Card title="Manage Selected User">
            {!selectedUser || !editUserDraft ? <p className="text-sm text-slate-500">Select a tenant user from the list.</p> : null}
            {selectedUser && editUserDraft ? (
              <>
                <div className="grid gap-4 md:grid-cols-2">
                  <FormInput label="Username" value={selectedUser.username} disabled />
                  <FormInput label="Email" value={editUserDraft.email} onChange={(event) => setEditUserDraft((current) => current ? { ...current, email: event.target.value } : current)} />
                  <FormInput label="First Name" value={editUserDraft.firstName} onChange={(event) => setEditUserDraft((current) => current ? { ...current, firstName: event.target.value } : current)} />
                  <FormInput label="Last Name" value={editUserDraft.lastName} onChange={(event) => setEditUserDraft((current) => current ? { ...current, lastName: event.target.value } : current)} />
                  <SelectDropdown
                    label="Role"
                    value={editUserDraft.role}
                    options={[
                      { label: 'Operator', value: 'Operator' },
                      { label: 'Tenant Admin', value: 'TenantAdmin' }
                    ]}
                    onChange={(event) => setEditUserDraft((current) => current ? { ...current, role: event.target.value as TenantUserRole } : current)}
                  />
                  <label className="block text-sm">
                    <span className="mb-1 block font-medium text-slate-700">Enabled</span>
                    <div className="rounded border border-slate-300 px-3 py-2">
                      <input
                        type="checkbox"
                        checked={editUserDraft.enabled}
                        onChange={(event) => setEditUserDraft((current) => current ? { ...current, enabled: event.target.checked } : current)}
                      />
                    </div>
                  </label>
                </div>

                <div className="mt-4 flex flex-wrap gap-2">
                  <Button
                    isLoading={updateUserMutation.isPending}
                    onClick={async () => {
                      await updateUserMutation.mutateAsync({ userId: selectedUser.userId, payload: editUserDraft });
                      setStatusMessage('Tenant user updated.');
                    }}
                  >
                    Save user
                  </Button>
                </div>

                <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4">
                  <p className="text-sm font-semibold text-slate-900">Reset Password</p>
                  <div className="mt-3 grid gap-4 md:grid-cols-[1fr_auto]">
                    <FormInput
                      label="Temporary Password"
                      value={resetPasswordDraft}
                      onChange={(event) => setResetPasswordDraft(event.target.value)}
                      placeholder="Leave blank to auto-generate"
                    />
                    <div className="flex items-end">
                      <Button
                        className="bg-amber-600 hover:bg-amber-500"
                        isLoading={resetPasswordMutation.isPending}
                        onClick={async () => {
                          const result = await resetPasswordMutation.mutateAsync({
                            userId: selectedUser.userId,
                            payload: { temporaryPassword: resetPasswordDraft || undefined }
                          });
                          setRevealedPassword({ userId: result.userId, password: result.temporaryPassword });
                          setResetPasswordDraft('');
                          setStatusMessage('Temporary password reset.');
                        }}
                      >
                        Reset password
                      </Button>
                    </div>
                  </div>
                </div>
              </>
            ) : null}
          </Card>
        </div>
      </div>

      <Card title="Advanced Config Entry">
        <div className="grid gap-4 md:grid-cols-3">
          <FormInput label="Config Domain" value={advancedConfigDraft.configDomain} onChange={(event) => setAdvancedConfigDraft((current) => ({ ...current, configDomain: event.target.value }))} />
          <FormInput label="Config Key" value={advancedConfigDraft.configKey} onChange={(event) => setAdvancedConfigDraft((current) => ({ ...current, configKey: event.target.value }))} />
          <label className="flex items-end gap-2 text-sm font-medium text-slate-700">
            <input
              type="checkbox"
              checked={advancedConfigDraft.isEncrypted}
              onChange={(event) => setAdvancedConfigDraft((current) => ({ ...current, isEncrypted: event.target.checked }))}
            />
            Encrypted
          </label>
          <div className="md:col-span-3">
            <TextAreaField
              label="JSON Value"
              rows={6}
              value={advancedConfigDraft.jsonValue}
              onChange={(event) => setAdvancedConfigDraft((current) => ({ ...current, jsonValue: event.target.value }))}
            />
          </div>
        </div>
        <div className="mt-4">
          <Button
            isLoading={configMutation.isPending}
            onClick={async () => {
              await configMutation.mutateAsync(advancedConfigDraft);
              setStatusMessage(`Saved config ${advancedConfigDraft.configDomain}/${advancedConfigDraft.configKey}.`);
            }}
          >
            Save config entry
          </Button>
        </div>
      </Card>
    </div>
  );
};

const Info = ({ label, children }: { label: string; children: string }) => (
  <div className="rounded-xl border border-slate-200 bg-slate-50 p-3">
    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">{label}</p>
    <p className="mt-2 text-sm text-slate-900">{children}</p>
  </div>
);

const RoleBadge = ({ role }: { role: TenantUserRole }) => (
  <Badge tone={role === 'TenantAdmin' ? 'warning' : 'info'}>{role === 'TenantAdmin' ? 'Tenant Admin' : 'Operator'}</Badge>
);

const StatusBadge = ({ status }: { status: string }) => {
  const tone = status === 'Active' ? 'success' : status === 'Pending' || status === 'Invited' ? 'warning' : status === 'Disconnected' ? 'neutral' : 'danger';
  return <Badge tone={tone}>{status}</Badge>;
};

const readStringConfig = (
  entries: Array<{ configDomain: string; configKey: string; jsonValue: string }>,
  configDomain: string,
  configKey: string
) => {
  const entry = entries.find((item) => item.configDomain === configDomain && item.configKey === configKey);
  if (!entry?.jsonValue) {
    return '';
  }

  try {
    const parsed = JSON.parse(entry.jsonValue);
    if (typeof parsed === 'string' || typeof parsed === 'number' || typeof parsed === 'boolean') {
      return String(parsed);
    }

    return JSON.stringify(parsed);
  } catch {
    return entry.jsonValue;
  }
};
