import { useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { Badge } from '../../components/Badge';
import { BootSplash } from '../../components/BootSplash';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { Table } from '../../components/Table';
import {
  useAcceptSignupTerms,
  useConfirmSignupTestPayment,
  useCreateSignupBillingIntent,
  useProvisionSignupSession,
  useResendSignupVerification,
  useSaveSignupAdmin,
  useSaveSignupEmail,
  useSaveSignupOrganization,
  useSelectSignupPlan,
  useSignupBootstrap,
  useSignupPlans,
  useSignupSession,
  useStartSignupSession,
  useVerifySignupEmail
} from './hooks';
import { clearSignupSession, getStoredSignupSession, storeSignupSession } from '../../services/signupSessionStore';
import { SignupAdminSetupRequest, SignupOrganizationRequest, SignupPlan, SignupSessionEnvelope } from '../../types/signup';
import { formatDateTime, humanize } from '../../utils/formatters';
import { useTenantDiscovery } from '../session/hooks';
import { setActiveTenantId } from '../../services/tenantScope';

const planStepOrder = ['plan', 'verification', 'organization', 'terms', 'billing', 'admin', 'provision', 'completed'];

export const SignupWizardPage = () => {
  const auth = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const bootstrapQuery = useSignupBootstrap();
  const plansQuery = useSignupPlans();
  const tenantDiscovery = useTenantDiscovery();
  const startSessionMutation = useStartSignupSession();
  const verifyEmailMutation = useVerifySignupEmail();
  const saveEmailMutation = useSaveSignupEmail();
  const resendVerificationMutation = useResendSignupVerification();
  const saveOrganizationMutation = useSaveSignupOrganization();
  const selectPlanMutation = useSelectSignupPlan();
  const acceptTermsMutation = useAcceptSignupTerms();
  const createBillingIntentMutation = useCreateSignupBillingIntent();
  const confirmTestPaymentMutation = useConfirmSignupTestPayment();
  const saveAdminMutation = useSaveSignupAdmin();
  const provisionMutation = useProvisionSignupSession();

  const [signupAccess, setSignupAccess] = useState(getStoredSignupSession());
  const sessionQuery = useSignupSession(signupAccess?.sessionId, signupAccess?.accessToken);

  const [emailDraft, setEmailDraft] = useState('');
  const [organizationDraft, setOrganizationDraft] = useState<SignupOrganizationRequest>({
    organizationName: '',
    organizationLegalName: '',
    tenantSlug: '',
    billingEmail: '',
    defaultCurrency: 'USD',
    timeZone: 'Africa/Johannesburg'
  });
  const [adminDraft, setAdminDraft] = useState<SignupAdminSetupRequest>({
    email: '',
    firstName: '',
    lastName: '',
    username: ''
  });
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [debugVerificationToken, setDebugVerificationToken] = useState<string | null>(null);
  const [handledVerificationKey, setHandledVerificationKey] = useState('');

  const session = sessionQuery.data?.session;
  const selectedPlan = useMemo(
    () => plansQuery.data?.find((plan) => plan.code === session?.selectedPlanCode),
    [plansQuery.data, session?.selectedPlanCode]
  );

  useEffect(() => {
    if (!session) {
      return;
    }

    setEmailDraft(session.email ?? '');
    setOrganizationDraft({
      organizationName: session.organizationName ?? '',
      organizationLegalName: session.organizationLegalName ?? '',
      tenantSlug: session.tenantSlug ?? '',
      billingEmail: session.billingEmail ?? session.email ?? '',
      defaultCurrency: session.defaultCurrency ?? 'USD',
      timeZone: session.timeZone ?? 'Africa/Johannesburg'
    });
    setAdminDraft({
      email: session.adminEmail ?? session.billingEmail ?? session.email ?? '',
      firstName: session.adminFirstName ?? '',
      lastName: session.adminLastName ?? '',
      username: session.adminUsername ?? ''
    });
  }, [session]);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const sessionId = params.get('session');
    const verifyToken = params.get('verify');
    const verificationKey = `${sessionId ?? ''}:${verifyToken ?? ''}`;

    if (!sessionId || !verifyToken || handledVerificationKey === verificationKey) {
      return;
    }

    setHandledVerificationKey(verificationKey);
    verifyEmailMutation.mutate(
      { sessionId, payload: { token: verifyToken } },
      {
        onSuccess: (result) => {
          rememberSignupSession(result);
          setStatusMessage('Email verified. Continue setup.');
          navigate('/signup', { replace: true });
        }
      }
    );
  }, [handledVerificationKey, location.search, navigate, verifyEmailMutation]);

  const rememberSignupSession = (result: SignupSessionEnvelope) => {
    const nextToken = result.accessToken ?? signupAccess?.accessToken;
    if (nextToken) {
      storeSignupSession(result.session.id, nextToken);
      setSignupAccess({ sessionId: result.session.id, accessToken: nextToken });
    }

    if (result.debugVerificationToken) {
      setDebugVerificationToken(result.debugVerificationToken);
    }
  };

  const ensureSession = async () => {
    if (signupAccess?.sessionId && signupAccess.accessToken) {
      return signupAccess;
    }

    const started = await startSessionMutation.mutateAsync();
    if (!started.accessToken) {
      throw new Error('Signup session token was not returned.');
    }

    rememberSignupSession(started);
    return { sessionId: started.session.id, accessToken: started.accessToken };
  };

  const startOrSelectPlan = async (plan: SignupPlan) => {
    setStatusMessage(null);
    const current = await ensureSession();
    const result = await selectPlanMutation.mutateAsync({
      sessionId: current.sessionId,
      accessToken: current.accessToken,
      payload: { planCode: plan.code }
    });
    rememberSignupSession(result);
  };

  const launchWorkspace = async () => {
    if (!session?.tenantSlug) {
      return;
    }

    const discovery = await tenantDiscovery.mutateAsync({ tenantSlug: session.tenantSlug });
    if (!discovery.auth || !discovery.tenantId) {
      setStatusMessage('Tenant login route was not found.');
      return;
    }

    setActiveTenantId(discovery.tenantId);
    await auth.beginLogin({
      ...discovery.auth,
      kind: 'tenant',
      tenantId: discovery.tenantId,
      tenantSlug: discovery.tenantSlug
    });
  };

  const activeError = [
    bootstrapQuery.error,
    plansQuery.error,
    sessionQuery.error,
    startSessionMutation.error,
    verifyEmailMutation.error,
    saveEmailMutation.error,
    resendVerificationMutation.error,
    saveOrganizationMutation.error,
    selectPlanMutation.error,
    acceptTermsMutation.error,
    createBillingIntentMutation.error,
    confirmTestPaymentMutation.error,
    saveAdminMutation.error,
    provisionMutation.error,
    tenantDiscovery.error
  ].find(Boolean) as Error | undefined;

  if (bootstrapQuery.isLoading || plansQuery.isLoading || verifyEmailMutation.isPending) {
    return <BootSplash eyebrow="Opening Signup" title="Preparing SaaS onboarding" detail="Loading plans, session state, and verification flow." />;
  }

  if (!bootstrapQuery.data?.enabled) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-10">
        <Card title="Signup Unavailable">
          <p className="text-sm text-slate-600">{bootstrapQuery.data?.disabledReason || 'Signup is currently disabled.'}</p>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.18),_transparent_32%),linear-gradient(180deg,_#fffaf2_0%,_#f8fafc_45%,_#eef2ff_100%)] px-4 py-10">
      <div className="mx-auto max-w-6xl space-y-6">
        <Card title="SaaS Signup Wizard">
          <div className="flex flex-wrap items-center gap-2">
            <Badge tone="info">Multi-tenant SaaS</Badge>
            <Badge tone="warning">Self onboarding</Badge>
            {session ? <Badge tone="success">{session.status}</Badge> : null}
          </div>
          <p className="mt-3 text-sm text-slate-600">
            Start signup, verify email, choose a plan, create tenant admin, and provision your tenant realm with minimal manual ops work.
          </p>
        </Card>

        {statusMessage ? (
          <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 shadow-sm">
            <p className="text-sm font-medium text-emerald-700">{statusMessage}</p>
          </div>
        ) : null}

        {debugVerificationToken ? (
          <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 shadow-sm">
            <p className="text-sm font-medium text-amber-700">Debug verification token: {debugVerificationToken}</p>
          </div>
        ) : null}

        {activeError ? (
          <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 shadow-sm">
            <p className="text-sm font-medium text-rose-700">{activeError.message}</p>
          </div>
        ) : null}

        <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
          <div className="space-y-6">
            <Card title="Choose Your Plan">
              <div className="grid gap-4 lg:grid-cols-3">
                {plansQuery.data?.map((plan) => (
                  <article key={plan.code} className={`rounded-2xl border p-4 ${session?.selectedPlanCode === plan.code ? 'border-amber-300 bg-amber-50' : 'border-slate-200 bg-white'}`}>
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge tone={plan.signupKind === 'Paid' ? 'warning' : plan.signupKind === 'Trial' ? 'info' : 'success'}>{plan.signupKind}</Badge>
                      {session?.selectedPlanCode === plan.code ? <Badge tone="success">Selected</Badge> : null}
                    </div>
                    <h3 className="mt-3 text-lg font-semibold text-slate-900">{plan.name}</h3>
                    <p className="mt-2 text-sm text-slate-600">{plan.description}</p>
                    <p className="mt-3 text-sm font-medium text-slate-900">
                      {plan.monthlyPrice > 0 ? `${plan.monthlyPrice} ${plan.currency}/mo` : plan.signupKind === 'Trial' ? `${plan.trialDays} day trial` : 'No monthly charge'}
                    </p>
                    <div className="mt-4">
                      <Button isLoading={selectPlanMutation.isPending && session?.selectedPlanCode !== plan.code} onClick={() => startOrSelectPlan(plan)}>
                        {session?.selectedPlanCode === plan.code ? 'Selected' : 'Choose plan'}
                      </Button>
                    </div>
                  </article>
                ))}
              </div>
            </Card>

            <Card title="Email Verification">
              <div className="grid gap-4 md:grid-cols-[1fr_auto]">
                <FormInput label="Work Email" value={emailDraft} onChange={(event) => setEmailDraft(event.target.value)} placeholder="you@agency.com" />
                <div className="flex items-end gap-2">
                  <Button
                    isLoading={saveEmailMutation.isPending}
                    onClick={async () => {
                      const current = await ensureSession();
                      const result = await saveEmailMutation.mutateAsync({
                        sessionId: current.sessionId,
                        accessToken: current.accessToken,
                        payload: { email: emailDraft }
                      });
                      rememberSignupSession(result);
                      setStatusMessage('Verification email sent.');
                    }}
                  >
                    Send verify email
                  </Button>
                </div>
              </div>
              {session?.verificationSentAt ? (
                <div className="mt-4 flex flex-wrap items-center gap-2">
                  <Badge tone={session.emailVerified ? 'success' : 'warning'}>{session.emailVerified ? 'Verified' : 'Pending verification'}</Badge>
                  <span className="text-sm text-slate-600">Sent {formatDateTime(session.verificationSentAt)}</span>
                </div>
              ) : null}
              {!session?.emailVerified && signupAccess ? (
                <div className="mt-4">
                  <Button
                    className="bg-slate-600 hover:bg-slate-500"
                    isLoading={resendVerificationMutation.isPending}
                    onClick={async () => {
                      const result = await resendVerificationMutation.mutateAsync({
                        sessionId: signupAccess.sessionId,
                        accessToken: signupAccess.accessToken
                      });
                      rememberSignupSession(result);
                      setStatusMessage('Verification email resent.');
                    }}
                  >
                    Resend verification
                  </Button>
                </div>
              ) : null}
            </Card>

            <Card title="Organization Details">
              <div className="grid gap-4 md:grid-cols-2">
                <FormInput label="Organization Name" value={organizationDraft.organizationName} onChange={(event) => setOrganizationDraft((current) => ({ ...current, organizationName: event.target.value }))} />
                <FormInput label="Legal Name" value={organizationDraft.organizationLegalName ?? ''} onChange={(event) => setOrganizationDraft((current) => ({ ...current, organizationLegalName: event.target.value }))} />
                <FormInput label="Tenant Code" value={organizationDraft.tenantSlug} onChange={(event) => setOrganizationDraft((current) => ({ ...current, tenantSlug: event.target.value.toLowerCase() }))} />
                <FormInput label="Billing Email" value={organizationDraft.billingEmail} onChange={(event) => setOrganizationDraft((current) => ({ ...current, billingEmail: event.target.value }))} />
                <FormInput label="Currency" value={organizationDraft.defaultCurrency} onChange={(event) => setOrganizationDraft((current) => ({ ...current, defaultCurrency: event.target.value.toUpperCase() }))} />
                <FormInput label="Time Zone" value={organizationDraft.timeZone} onChange={(event) => setOrganizationDraft((current) => ({ ...current, timeZone: event.target.value }))} />
              </div>
              <div className="mt-4">
                <Button
                  isLoading={saveOrganizationMutation.isPending}
                  onClick={async () => {
                    const current = await ensureSession();
                    const result = await saveOrganizationMutation.mutateAsync({
                      sessionId: current.sessionId,
                      accessToken: current.accessToken,
                      payload: organizationDraft
                    });
                    rememberSignupSession(result);
                    setStatusMessage('Organization details saved.');
                  }}
                >
                  Save organization
                </Button>
              </div>
            </Card>

            <Card title="Terms And Billing">
              <div className="space-y-4">
                <label className="flex items-center gap-2 text-sm text-slate-700">
                  <input
                    type="checkbox"
                    checked={session?.termsAccepted ?? false}
                    onChange={async (event) => {
                      const current = await ensureSession();
                      const result = await acceptTermsMutation.mutateAsync({
                        sessionId: current.sessionId,
                        accessToken: current.accessToken,
                        payload: { accepted: event.target.checked }
                      });
                      rememberSignupSession(result);
                    }}
                  />
                  I accept required terms and onboarding confirmations
                </label>

                {selectedPlan?.signupKind === 'Paid' ? (
                  <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                    <p className="text-sm font-medium text-slate-900">Paid plan billing</p>
                    <p className="mt-2 text-sm text-slate-600">
                      Billing status: <span className="font-medium">{session?.billingStatus ?? 'Pending'}</span>
                    </p>
                    <div className="mt-4 flex flex-wrap gap-2">
                      <Button
                        isLoading={createBillingIntentMutation.isPending}
                        onClick={async () => {
                          const current = await ensureSession();
                          const result = await createBillingIntentMutation.mutateAsync({
                            sessionId: current.sessionId,
                            accessToken: current.accessToken
                          });
                          rememberSignupSession(result);
                          setStatusMessage('Billing intent created.');
                        }}
                      >
                        Create billing intent
                      </Button>
                      {bootstrapQuery.data?.allowTestPaymentConfirmation ? (
                        <Button
                          className="bg-amber-600 hover:bg-amber-500"
                          isLoading={confirmTestPaymentMutation.isPending}
                          onClick={async () => {
                            const current = await ensureSession();
                            const result = await confirmTestPaymentMutation.mutateAsync({
                              sessionId: current.sessionId,
                              accessToken: current.accessToken
                            });
                            rememberSignupSession(result);
                            setStatusMessage('Test payment confirmed.');
                          }}
                        >
                          Confirm test payment
                        </Button>
                      ) : null}
                    </div>
                  </div>
                ) : (
                  <p className="text-sm text-slate-600">Selected plan does not require paid billing during signup.</p>
                )}
              </div>
            </Card>

            <Card title="Initial Tenant Admin">
              <div className="grid gap-4 md:grid-cols-2">
                <FormInput label="Admin Email" value={adminDraft.email} onChange={(event) => setAdminDraft((current) => ({ ...current, email: event.target.value }))} />
                <FormInput label="Username" value={adminDraft.username} onChange={(event) => setAdminDraft((current) => ({ ...current, username: event.target.value }))} />
                <FormInput label="First Name" value={adminDraft.firstName} onChange={(event) => setAdminDraft((current) => ({ ...current, firstName: event.target.value }))} />
                <FormInput label="Last Name" value={adminDraft.lastName} onChange={(event) => setAdminDraft((current) => ({ ...current, lastName: event.target.value }))} />
              </div>
              <div className="mt-4">
                <Button
                  isLoading={saveAdminMutation.isPending}
                  onClick={async () => {
                    const current = await ensureSession();
                    const result = await saveAdminMutation.mutateAsync({
                      sessionId: current.sessionId,
                      accessToken: current.accessToken,
                      payload: adminDraft
                    });
                    rememberSignupSession(result);
                    setStatusMessage('Initial admin prepared.');
                  }}
                >
                  Save admin setup
                </Button>
              </div>
            </Card>

            <Card title="Provision Tenant">
              <div className="flex flex-wrap items-center gap-2">
                <Badge tone={session?.status === 'Active' ? 'success' : session?.status === 'Failed' ? 'danger' : 'info'}>{session?.status ?? 'Draft'}</Badge>
                {session?.lastError ? <Badge tone="danger">Needs retry</Badge> : null}
              </div>
              <p className="mt-3 text-sm text-slate-600">
                This stage creates the tenant, provisions Keycloak realm setup, seeds config, creates the first tenant admin, and activates the tenant.
              </p>
              <div className="mt-4 flex flex-wrap gap-2">
                <Button
                  isLoading={provisionMutation.isPending}
                  onClick={async () => {
                    const current = await ensureSession();
                    const result = await provisionMutation.mutateAsync({
                      sessionId: current.sessionId,
                      accessToken: current.accessToken
                    });
                    rememberSignupSession(result);
                    setStatusMessage('Tenant provisioning completed.');
                  }}
                >
                  Provision tenant
                </Button>
                {session?.status === 'Active' ? (
                  <Button className="bg-emerald-700 hover:bg-emerald-600" onClick={() => launchWorkspace()}>
                    Launch workspace
                  </Button>
                ) : null}
                {session ? (
                  <Button
                    className="bg-slate-600 hover:bg-slate-500"
                    onClick={() => {
                      clearSignupSession();
                      setSignupAccess(null);
                      navigate('/signup', { replace: true });
                    }}
                  >
                    Clear local signup
                  </Button>
                ) : null}
              </div>
              {session?.temporaryPassword ? (
                <div className="mt-4 rounded-2xl border border-amber-200 bg-amber-50 p-4">
                  <p className="text-sm font-semibold text-amber-800">Temporary admin password</p>
                  <p className="mt-2 text-sm text-amber-700">{session.temporaryPassword}</p>
                </div>
              ) : null}
            </Card>
          </div>

          <div className="space-y-6">
            <Card title="Progress">
              <div className="space-y-3">
                {planStepOrder.map((step, index) => {
                  const isActive = session?.currentStep === step || (!session && step === 'plan');
                  const isDone = isStepComplete(step, session, selectedPlan);
                  return (
                    <div key={step} className={`rounded-xl border px-3 py-3 ${isActive ? 'border-amber-300 bg-amber-50' : 'border-slate-200 bg-white'}`}>
                      <div className="flex items-center justify-between gap-3">
                        <p className="text-sm font-medium text-slate-900">
                          {index + 1}. {humanize(step)}
                        </p>
                        <Badge tone={isDone ? 'success' : isActive ? 'warning' : 'neutral'}>{isDone ? 'Done' : isActive ? 'Current' : 'Pending'}</Badge>
                      </div>
                    </div>
                  );
                })}
              </div>
            </Card>

            <Card title="Session Snapshot">
              {!session ? <p className="text-sm text-slate-500">No signup session started yet.</p> : null}
              {session ? (
                <Table headers={['Field', 'Value']}>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Status</td>
                    <td className="px-3 py-2 text-slate-900">{session.status}</td>
                  </tr>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Tenant</td>
                    <td className="px-3 py-2 text-slate-900">{session.organizationName ?? '-'}</td>
                  </tr>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Tenant Code</td>
                    <td className="px-3 py-2 text-slate-900">{session.tenantSlug ?? '-'}</td>
                  </tr>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Plan</td>
                    <td className="px-3 py-2 text-slate-900">{session.selectedPlanCode ?? '-'}</td>
                  </tr>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Billing</td>
                    <td className="px-3 py-2 text-slate-900">{session.billingStatus}</td>
                  </tr>
                  <tr className="border-t border-slate-200">
                    <td className="px-3 py-2 text-slate-600">Expires</td>
                    <td className="px-3 py-2 text-slate-900">{formatDateTime(session.expiresAt)}</td>
                  </tr>
                </Table>
              ) : null}
            </Card>

            {selectedPlan ? (
              <Card title="Selected Plan Limits">
                <div className="space-y-3">
                  {Object.entries(selectedPlan.limits).map(([key, value]) => (
                    <div key={key} className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-3">
                      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">{humanize(key)}</p>
                      <p className="mt-2 text-sm text-slate-900">{value}</p>
                    </div>
                  ))}
                </div>
              </Card>
            ) : null}
          </div>
        </div>
      </div>
    </div>
  );
};

const isStepComplete = (step: string, session?: SignupSessionEnvelope['session'], selectedPlan?: SignupPlan) => {
  if (!session) {
    return false;
  }

  switch (step) {
    case 'plan':
      return Boolean(session.selectedPlanCode);
    case 'verification':
      return session.emailVerified;
    case 'organization':
      return Boolean(session.organizationName && session.tenantSlug && session.billingEmail);
    case 'terms':
      return !selectedPlan?.requiresTermsAcceptance || session.termsAccepted;
    case 'billing':
      return selectedPlan?.signupKind !== 'Paid' || session.billingStatus === 'Confirmed';
    case 'admin':
      return Boolean(session.adminEmail && session.adminFirstName && session.adminLastName && session.adminUsername);
    case 'provision':
      return session.status === 'Active';
    case 'completed':
      return session.status === 'Active';
    default:
      return false;
  }
};
