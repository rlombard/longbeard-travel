import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { Button } from '../../components/Button';
import { BootSplash } from '../../components/BootSplash';
import { FormInput } from '../../components/FormInput';
import { useSessionBootstrap, useTenantDiscovery } from '../session/hooks';
import { setActiveTenantId } from '../../services/tenantScope';

export const LandingPage = () => {
  const auth = useAuth();
  const navigate = useNavigate();
  const bootstrapQuery = useSessionBootstrap();
  const discovery = useTenantDiscovery();
  const [identifier, setIdentifier] = useState('');
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (bootstrapQuery.data?.session?.currentTenantId) {
      setActiveTenantId(bootstrapQuery.data.session.currentTenantId);
    }
  }, [bootstrapQuery.data?.session?.currentTenantId]);

  useEffect(() => {
    if (auth.isAuthenticated && bootstrapQuery.data?.session?.homeArea) {
      navigate(bootstrapQuery.data.session.homeArea, { replace: true });
    }
  }, [auth.isAuthenticated, bootstrapQuery.data?.session?.homeArea, navigate]);

  const isStandalone = bootstrapQuery.data?.deploymentMode === 'Standalone';
  const operatorLabel = useMemo(
    () => isStandalone ? 'Login to your workspace' : 'Find your workspace',
    [isStandalone]
  );

  const beginStandaloneLogin = async () => {
    const target = bootstrapQuery.data?.standaloneTenantAuth;
    if (!target) {
      setMessage('Standalone tenant auth is not configured.');
      return;
    }

    await auth.beginLogin({
      ...target,
      kind: 'tenant'
    });
  };

  const beginManagementLogin = async () => {
    const target = bootstrapQuery.data?.managementAuth;
    if (!target) {
      setMessage('Management auth is not configured.');
      return;
    }

    setActiveTenantId(null);
    await auth.beginLogin({
      ...target,
      kind: 'management'
    });
  };

  const beginTenantLogin = async () => {
    setMessage(null);

    if (isStandalone) {
      await beginStandaloneLogin();
      return;
    }

    const value = identifier.trim();
    if (!value) {
      setMessage('Enter work email or tenant code.');
      return;
    }

    const result = await discovery.mutateAsync(value.includes('@')
      ? { email: value }
      : { tenantSlug: value });

    if (!result.found || !result.auth || !result.tenantId) {
      setMessage('Workspace not found. Use your tenant code or ask support.');
      return;
    }

    setActiveTenantId(result.tenantId);
    await auth.beginLogin({
      ...result.auth,
      kind: 'tenant',
      tenantId: result.tenantId,
      tenantSlug: result.tenantSlug
    });
  };

  if (!auth.isReady || bootstrapQuery.isLoading) {
    return <BootSplash eyebrow="Opening Entry" title="Preparing workspace access" detail="Checking deployment mode, tenant routing, and saved session state." />;
  }

  if (auth.error) {
    return <div className="p-8 text-red-600">Authentication error: {auth.error}</div>;
  }

  if (bootstrapQuery.isError) {
    return (
      <div className="p-8 text-red-600">
        Platform bootstrap failed: {(bootstrapQuery.error as Error).message}
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.18),_transparent_32%),linear-gradient(180deg,_#fffaf2_0%,_#f8fafc_45%,_#eef2ff_100%)] px-4 py-10">
      <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[1.2fr_0.8fr]">
        <section className="rounded-[2rem] border border-white/70 bg-white/90 p-8 shadow-[0_24px_80px_rgba(15,23,42,0.10)] backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.35em] text-amber-700">AI Forged Tour Ops</p>
          <h1 className="mt-4 text-4xl font-bold tracking-tight text-slate-900">TourOps for dedicated operators and clean platform oversight.</h1>
          <p className="mt-4 max-w-2xl text-sm leading-6 text-slate-600">
            Operators land in their own tenant workspace. Platform owners get a separate management view for tenants, billing, licensing, onboarding, and support.
          </p>
          <div className="mt-8 grid gap-4 md:grid-cols-3">
            <Tile title="Operator-first" copy="Tenant users see only their own workspace, data, email setup, and operations." />
            <Tile title="Platform control" copy="Deployment owners manage onboarding, license state, usage, billing, and audit." />
            <Tile title="Mode-aware" copy="Standalone stays simple. SaaS adds a separate management surface." />
          </div>
        </section>

        <section className="rounded-[2rem] border border-slate-200 bg-white p-8 shadow-[0_24px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-500">Entry</p>
          <h2 className="mt-3 text-2xl font-semibold text-slate-900">{operatorLabel}</h2>
          <p className="mt-2 text-sm text-slate-600">
            {isStandalone
              ? 'Standalone keeps one tenant experience. No SaaS management UI is shown.'
              : 'Use work email or tenant code. We route you into the correct tenant realm.'}
          </p>

          {!isStandalone ? (
            <div className="mt-6 space-y-3">
              <FormInput
                label="Work Email Or Tenant Code"
                value={identifier}
                onChange={(event) => setIdentifier(event.target.value)}
                placeholder="you@agency.com or safari-hq"
              />
            </div>
          ) : null}

          {message ? <p className="mt-4 text-sm text-rose-600">{message}</p> : null}

          <div className="mt-6 flex flex-wrap gap-3">
            <Button isLoading={discovery.isPending} onClick={() => beginTenantLogin()}>
              {isStandalone ? 'Login' : 'Continue'}
            </Button>
            {bootstrapQuery.data?.publicSignupEnabled ? (
              <Button className="bg-sky-700 hover:bg-sky-600" onClick={() => navigate('/signup')}>
                Start Signup
              </Button>
            ) : null}
            {bootstrapQuery.data?.platformManagementEnabled ? (
              <Button className="bg-amber-600 hover:bg-amber-500" onClick={() => beginManagementLogin()}>
                Management Login
              </Button>
            ) : null}
          </div>
        </section>
      </div>
    </div>
  );
};

const Tile = ({ title, copy }: { title: string; copy: string }) => (
  <article className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
    <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
    <p className="mt-2 text-sm leading-6 text-slate-600">{copy}</p>
  </article>
);
