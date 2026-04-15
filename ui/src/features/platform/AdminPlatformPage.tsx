import { ReactNode, useEffect, useState } from 'react';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { getActiveTenantId, setActiveTenantId } from '../../services/tenantScope';
import { AssignTenantUserRequest, CreateTenantRequest, UpdateTenantOnboardingRequest, UpsertTenantConfigRequest } from '../../types/platform';
import { useConfirmPlatformSignupBilling, usePlatformSignupSessions, useRetryPlatformSignupSession } from '../signup/hooks';
import { formatDateTime, humanize } from '../../utils/formatters';
import { useAssignTenantUser, useCreateTenant, useTenant, useTenants, useUpdateTenantOnboarding, useUpsertTenantConfig } from './hooks';

const createDraftDefaults: CreateTenantRequest = {
  slug: '',
  name: '',
  legalName: '',
  billingEmail: '',
  defaultCurrency: 'USD',
  timeZone: 'Africa/Johannesburg',
  licensePlanCode: 'saas-trial',
  isStandaloneTenant: false,
  bootstrapAdmin: {
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    temporaryPassword: ''
  }
};

const onboardingDraftDefaults: UpdateTenantOnboardingRequest = {
  step: 'organization',
  markCompleted: true,
  completeOnboarding: false,
  payloadJson: '',
  error: ''
};

const configDraftDefaults: UpsertTenantConfigRequest = {
  configDomain: 'email',
  configKey: 'default',
  jsonValue: '{\n  "provider": "microsoft365"\n}',
  isEncrypted: false
};

const userDraftDefaults: AssignTenantUserRequest = {
  userId: '',
  email: '',
  displayName: '',
  role: 'Operator'
};

export const AdminPlatformPage = () => {
  const [selectedTenantId, setSelectedTenantId] = useState<string | undefined>(getActiveTenantId() ?? undefined);
  const [createDraft, setCreateDraft] = useState<CreateTenantRequest>(createDraftDefaults);
  const [onboardingDraft, setOnboardingDraft] = useState<UpdateTenantOnboardingRequest>(onboardingDraftDefaults);
  const [configDraft, setConfigDraft] = useState<UpsertTenantConfigRequest>(configDraftDefaults);
  const [userDraft, setUserDraft] = useState<AssignTenantUserRequest>(userDraftDefaults);

  const tenantsQuery = useTenants();
  const tenantQuery = useTenant(selectedTenantId);
  const createMutation = useCreateTenant();
  const onboardingMutation = useUpdateTenantOnboarding();
  const configMutation = useUpsertTenantConfig();
  const assignUserMutation = useAssignTenantUser();
  const signupSessionsQuery = usePlatformSignupSessions();
  const retrySignupMutation = useRetryPlatformSignupSession();
  const confirmSignupBillingMutation = useConfirmPlatformSignupBilling();

  useEffect(() => {
    if (selectedTenantId) {
      setActiveTenantId(selectedTenantId);
      return;
    }

    if ((tenantsQuery.data?.length ?? 0) > 0) {
      const firstTenantId = tenantsQuery.data![0].id;
      setSelectedTenantId(firstTenantId);
      setActiveTenantId(firstTenantId);
    }
  }, [selectedTenantId, tenantsQuery.data]);

  const activeError = [tenantsQuery.error, tenantQuery.error, createMutation.error, onboardingMutation.error, configMutation.error, assignUserMutation.error, signupSessionsQuery.error, retrySignupMutation.error, confirmSignupBillingMutation.error]
    .find(Boolean) as Error | undefined;
  const detail = tenantQuery.data;
  const license = detail?.license ?? null;

  return (
    <div className="space-y-6">
      <Card title="Platform Tenants">
        <div className="flex flex-wrap items-center gap-2">
          <Badge tone="warning">Platform admin</Badge>
          <Badge tone="info">{tenantsQuery.data?.length ?? 0} tenants</Badge>
          {selectedTenantId ? <Badge tone="success">Active tenant scope set</Badge> : null}
        </div>
        <p className="mt-3 text-sm text-slate-600">
          Manage SaaS tenants, license state, onboarding, config, and operator access. Active tenant selection also scopes the regular operator screens through the BFF.
        </p>
      </Card>

      <div className="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
        <Card title="Tenant List">
          {tenantsQuery.isLoading ? <p className="text-sm text-slate-500">Loading tenants...</p> : null}
          {tenantsQuery.data?.length ? (
            <Table headers={['Tenant', 'License', 'Onboarding', 'Scope']}>
              {tenantsQuery.data.map((tenant) => (
                <tr key={tenant.id} className={`border-t border-slate-200 ${selectedTenantId === tenant.id ? 'bg-amber-50/50' : ''}`}>
                  <td className="px-3 py-3">
                    <p className="font-medium text-slate-900">{tenant.name}</p>
                    <p className="text-xs text-slate-500">{tenant.slug}</p>
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex flex-wrap gap-1">
                      <Badge tone={tenant.licenseStatus === 'Active' ? 'success' : tenant.licenseStatus === 'Trial' ? 'warning' : 'danger'}>
                        {tenant.licensePlanCode ?? 'No plan'}
                      </Badge>
                      <Badge tone="info">{tenant.licenseStatus ?? 'Unknown'}</Badge>
                    </div>
                  </td>
                  <td className="px-3 py-3">
                    <div className="flex flex-wrap gap-1">
                      <Badge tone={tenant.onboardingStatus === 'Completed' ? 'success' : tenant.onboardingStatus === 'Blocked' ? 'danger' : 'warning'}>
                        {tenant.onboardingStatus}
                      </Badge>
                      <Badge tone="neutral">{tenant.currentOnboardingStep}</Badge>
                    </div>
                  </td>
                  <td className="px-3 py-3">
                    <Button
                      className={selectedTenantId === tenant.id ? 'bg-amber-600 hover:bg-amber-500' : 'bg-slate-900 hover:bg-slate-700'}
                      onClick={() => {
                        setSelectedTenantId(tenant.id);
                        setActiveTenantId(tenant.id);
                      }}
                    >
                      Scope
                    </Button>
                  </td>
                </tr>
              ))}
            </Table>
          ) : null}
        </Card>

        <Card title="Create Tenant">
          <div className="grid gap-4 md:grid-cols-2">
            <FormInput label="Slug" value={createDraft.slug} onChange={(event) => setCreateDraft((current) => ({ ...current, slug: event.target.value }))} />
            <FormInput label="Display Name" value={createDraft.name} onChange={(event) => setCreateDraft((current) => ({ ...current, name: event.target.value }))} />
            <FormInput label="Legal Name" value={createDraft.legalName ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, legalName: event.target.value }))} />
            <FormInput label="Billing Email" value={createDraft.billingEmail ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, billingEmail: event.target.value }))} />
            <FormInput label="Currency" value={createDraft.defaultCurrency} onChange={(event) => setCreateDraft((current) => ({ ...current, defaultCurrency: event.target.value }))} />
            <FormInput label="Time Zone" value={createDraft.timeZone} onChange={(event) => setCreateDraft((current) => ({ ...current, timeZone: event.target.value }))} />
            <FormInput label="Plan Code" value={createDraft.licensePlanCode} onChange={(event) => setCreateDraft((current) => ({ ...current, licensePlanCode: event.target.value }))} />
            <SelectDropdown
              label="Mode"
              value={createDraft.isStandaloneTenant ? 'standalone' : 'saas'}
              options={[
                { label: 'SaaS', value: 'saas' },
                { label: 'Standalone', value: 'standalone' }
              ]}
              onChange={(event) => setCreateDraft((current) => ({ ...current, isStandaloneTenant: event.target.value === 'standalone' }))}
            />
            <FormInput label="Admin Username" value={createDraft.bootstrapAdmin?.username ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, bootstrapAdmin: { ...current.bootstrapAdmin!, username: event.target.value } }))} />
            <FormInput label="Admin Email" value={createDraft.bootstrapAdmin?.email ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, bootstrapAdmin: { ...current.bootstrapAdmin!, email: event.target.value } }))} />
            <FormInput label="Admin First Name" value={createDraft.bootstrapAdmin?.firstName ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, bootstrapAdmin: { ...current.bootstrapAdmin!, firstName: event.target.value } }))} />
            <FormInput label="Admin Last Name" value={createDraft.bootstrapAdmin?.lastName ?? ''} onChange={(event) => setCreateDraft((current) => ({ ...current, bootstrapAdmin: { ...current.bootstrapAdmin!, lastName: event.target.value } }))} />
          </div>
          <div className="mt-4">
            <Button
              isLoading={createMutation.isPending}
              onClick={async () => {
                const created = await createMutation.mutateAsync(createDraft);
                setSelectedTenantId(created.tenant.id);
                setActiveTenantId(created.tenant.id);
                setCreateDraft(createDraftDefaults);
              }}
            >
              Create tenant
            </Button>
          </div>
        </Card>
      </div>

      {detail ? (
        <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
          <Card title="Tenant Detail">
            <div className="grid gap-4 md:grid-cols-2">
              <Info label="Tenant">{detail.tenant.name}</Info>
              <Info label="Slug">{detail.tenant.slug}</Info>
              <Info label="Status">{detail.tenant.status}</Info>
              <Info label="Users">{String(detail.tenant.activeUsers)}</Info>
              <Info label="Email Accounts">{String(detail.tenant.connectedEmailAccounts)}</Info>
              <Info label="Updated">{formatDateTime(detail.tenant.updatedAt)}</Info>
            </div>

            {license ? (
              <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge tone={license.status === 'Active' ? 'success' : license.status === 'Trial' ? 'warning' : 'danger'}>
                    {license.planCode}
                  </Badge>
                  <Badge tone="info">{license.billingMode}</Badge>
                </div>
                <div className="mt-3 grid gap-4 md:grid-cols-2">
                  {Object.entries(license.limits).map(([key, value]) => (
                    <Info key={key} label={humanize(key)}>
                      {license.currentUsage[key] ?? 0} / {value}
                    </Info>
                  ))}
                </div>
              </div>
            ) : null}

            <div className="mt-6">
              <h3 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-500">Usage</h3>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                {detail.usage.map((metric) => (
                  <div key={metric.metricKey} className="rounded-xl border border-slate-200 bg-white p-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">{metric.category}</p>
                    <p className="mt-1 font-medium text-slate-900">{metric.metricKey}</p>
                    <p className="mt-2 text-sm text-slate-600">
                      {metric.quantity} {metric.unit}
                    </p>
                  </div>
                ))}
              </div>
            </div>

            <div className="mt-6">
              <h3 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-500">Identity</h3>
              <div className="mt-3 space-y-3">
                {detail.identityMappings.map((mapping) => (
                  <div key={mapping.id} className="rounded-xl border border-slate-200 bg-white p-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge tone="info">{mapping.isolationMode}</Badge>
                      <Badge tone={mapping.provisioningStatus === 'Ready' ? 'success' : mapping.provisioningStatus === 'Failed' ? 'danger' : 'warning'}>
                        {mapping.provisioningStatus}
                      </Badge>
                    </div>
                    <p className="mt-2 text-sm text-slate-700">Realm: {mapping.realmName}</p>
                    <p className="text-sm text-slate-600">Issuer: {mapping.issuerUrl ?? '-'}</p>
                  </div>
                ))}
              </div>
            </div>
          </Card>

          <div className="space-y-6">
            <Card title="Onboarding">
              <div className="flex flex-wrap items-center gap-2">
                <Badge tone={detail.onboarding?.status === 'Completed' ? 'success' : detail.onboarding?.status === 'Blocked' ? 'danger' : 'warning'}>
                  {detail.onboarding?.status ?? 'NotStarted'}
                </Badge>
                <Badge tone="neutral">{detail.onboarding?.currentStep ?? 'organization'}</Badge>
              </div>
              <div className="mt-4 space-y-4">
                <FormInput label="Step" value={onboardingDraft.step} onChange={(event) => setOnboardingDraft((current) => ({ ...current, step: event.target.value }))} />
                <TextAreaField label="Payload JSON" rows={4} value={onboardingDraft.payloadJson ?? ''} onChange={(event) => setOnboardingDraft((current) => ({ ...current, payloadJson: event.target.value }))} />
                <div className="flex gap-2">
                  <Button
                    isLoading={onboardingMutation.isPending}
                    onClick={async () => {
                      await onboardingMutation.mutateAsync({ tenantId: detail.tenant.id, payload: { ...onboardingDraft, markCompleted: true, completeOnboarding: false } });
                    }}
                  >
                    Mark step done
                  </Button>
                  <Button
                    className="bg-emerald-700 hover:bg-emerald-600"
                    isLoading={onboardingMutation.isPending}
                    onClick={async () => {
                      await onboardingMutation.mutateAsync({ tenantId: detail.tenant.id, payload: { ...onboardingDraft, markCompleted: true, completeOnboarding: true } });
                    }}
                  >
                    Finish onboarding
                  </Button>
                </div>
              </div>
            </Card>

            <Card title="Tenant Config">
              <div className="space-y-4">
                <FormInput label="Config Domain" value={configDraft.configDomain} onChange={(event) => setConfigDraft((current) => ({ ...current, configDomain: event.target.value }))} />
                <FormInput label="Config Key" value={configDraft.configKey} onChange={(event) => setConfigDraft((current) => ({ ...current, configKey: event.target.value }))} />
                <TextAreaField label="JSON Value" rows={6} value={configDraft.jsonValue} onChange={(event) => setConfigDraft((current) => ({ ...current, jsonValue: event.target.value }))} />
                <Button isLoading={configMutation.isPending} onClick={() => configMutation.mutateAsync({ tenantId: detail.tenant.id, payload: configDraft })}>
                  Save config
                </Button>
              </div>
            </Card>

            <Card title="Assign User">
              <div className="space-y-4">
                <FormInput label="User Id" value={userDraft.userId} onChange={(event) => setUserDraft((current) => ({ ...current, userId: event.target.value }))} />
                <FormInput label="Email" value={userDraft.email} onChange={(event) => setUserDraft((current) => ({ ...current, email: event.target.value }))} />
                <FormInput label="Display Name" value={userDraft.displayName} onChange={(event) => setUserDraft((current) => ({ ...current, displayName: event.target.value }))} />
                <SelectDropdown
                  label="Role"
                  value={userDraft.role}
                  options={[
                    { label: 'Operator', value: 'Operator' },
                    { label: 'Tenant Admin', value: 'TenantAdmin' },
                    { label: 'Platform Admin', value: 'PlatformAdmin' }
                  ]}
                  onChange={(event) => setUserDraft((current) => ({ ...current, role: event.target.value as AssignTenantUserRequest['role'] }))}
                />
                <Button isLoading={assignUserMutation.isPending} onClick={() => assignUserMutation.mutateAsync({ tenantId: detail.tenant.id, payload: userDraft })}>
                  Assign user
                </Button>
              </div>
            </Card>
          </div>
        </div>
      ) : null}

      {detail ? (
        <div className="grid gap-6 xl:grid-cols-2">
          <Card title="Users">
            <Table headers={['User', 'Role', 'Status', 'Last Seen']}>
              {detail.users.map((user) => (
                <tr key={user.id} className="border-t border-slate-200">
                  <td className="px-3 py-3">
                    <p className="font-medium text-slate-900">{user.displayName}</p>
                    <p className="text-xs text-slate-500">{user.email}</p>
                  </td>
                  <td className="px-3 py-3 text-slate-700">{user.role}</td>
                  <td className="px-3 py-3 text-slate-700">{user.status}</td>
                  <td className="px-3 py-3 text-slate-700">{formatDateTime(user.lastSeenAt ?? undefined)}</td>
                </tr>
              ))}
            </Table>
          </Card>

          <Card title="Audit Trail">
            <Table headers={['Action', 'Result', 'Actor', 'At']}>
              {detail.auditEvents.map((event) => (
                <tr key={event.id} className="border-t border-slate-200">
                  <td className="px-3 py-3">
                    <p className="font-medium text-slate-900">{event.action}</p>
                    <p className="text-xs text-slate-500">{event.scopeType}</p>
                  </td>
                  <td className="px-3 py-3 text-slate-700">{event.result}</td>
                  <td className="px-3 py-3 text-slate-700">{event.actorDisplayName ?? event.actorUserId ?? '-'}</td>
                  <td className="px-3 py-3 text-slate-700">{formatDateTime(event.createdAt)}</td>
                </tr>
              ))}
            </Table>
          </Card>
        </div>
      ) : null}

      <Card title="Signup Onboarding Queue">
        {signupSessionsQuery.isLoading ? <p className="text-sm text-slate-500">Loading signup sessions...</p> : null}
        {signupSessionsQuery.data?.length === 0 ? <p className="text-sm text-slate-500">No recent public signup sessions.</p> : null}
        {signupSessionsQuery.data?.length ? (
          <Table headers={['Org', 'Plan', 'Status', 'Billing', 'Actions']}>
            {signupSessionsQuery.data.map((signup) => (
              <tr key={signup.id} className="border-t border-slate-200">
                <td className="px-3 py-3">
                  <p className="font-medium text-slate-900">{signup.organizationName ?? '-'}</p>
                  <p className="text-xs text-slate-500">{signup.email ?? '-'}</p>
                </td>
                <td className="px-3 py-3 text-slate-700">{signup.selectedPlanCode ?? '-'}</td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-1">
                    <Badge tone={signup.status === 'Active' ? 'success' : signup.status === 'Failed' ? 'danger' : 'warning'}>{signup.status}</Badge>
                    <Badge tone="neutral">{signup.currentStep}</Badge>
                  </div>
                  {signup.lastError ? <p className="mt-1 text-xs text-rose-600">{signup.lastError}</p> : null}
                </td>
                <td className="px-3 py-3">
                  <Badge tone={signup.billingStatus === 'Confirmed' || signup.billingStatus === 'NotRequired' ? 'success' : 'warning'}>{signup.billingStatus}</Badge>
                </td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-2">
                    {(signup.billingStatus === 'Pending' || signup.billingStatus === 'RequiresManualReview') ? (
                      <Button className="bg-amber-600 hover:bg-amber-500" onClick={() => confirmSignupBillingMutation.mutate(signup.id)}>
                        Confirm Billing
                      </Button>
                    ) : null}
                    {(signup.status === 'Failed' || signup.status === 'PaymentConfirmed') ? (
                      <Button className="bg-sky-700 hover:bg-sky-600" onClick={() => retrySignupMutation.mutate(signup.id)}>
                        Retry
                      </Button>
                    ) : null}
                  </div>
                </td>
              </tr>
            ))}
          </Table>
        ) : null}
      </Card>

      {activeError ? <p className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">{activeError.message}</p> : null}
    </div>
  );
};

const Info = ({ label, children }: { label: string; children: ReactNode }) => (
  <div className="rounded-xl border border-slate-200 bg-white p-3">
    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">{label}</p>
    <p className="mt-2 text-sm text-slate-800">{children}</p>
  </div>
);
