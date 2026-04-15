import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useSessionBootstrap } from '../features/session/hooks';
import { getActiveTenantId } from '../services/tenantScope';

const baseNav = [
  { to: '/app/suppliers', label: 'Suppliers' },
  { to: '/app/products', label: 'Products' },
  { to: '/app/itineraries', label: 'Itineraries' },
  { to: '/app/itineraries/ai', label: 'AI Itinerary Assist' },
  { to: '/app/quotes', label: 'Quote Generator' },
  { to: '/app/bookings', label: 'Bookings' },
  { to: '/app/invoices', label: 'Invoices' },
  { to: '/app/customers', label: 'Customers' },
  { to: '/app/emails', label: 'Emails' },
  { to: '/app/operations', label: 'Operations' }
];

export const PageLayout = () => {
  const location = useLocation();
  const activeTenantId = getActiveTenantId();
  const auth = useAuth();
  const bootstrapQuery = useSessionBootstrap();
  const session = bootstrapQuery.data?.session;
  const membership = session?.memberships.find((item) => item.tenantId === (activeTenantId ?? session?.currentTenantId ?? ''));
  const showSettings = session?.isPlatformAdmin || membership?.role === 'TenantAdmin';
  const navItems = showSettings
    ? [...baseNav, { to: '/app/settings', label: 'Settings' }]
    : baseNav;

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.18),_transparent_32%),linear-gradient(180deg,_#fffaf2_0%,_#f8fafc_45%,_#eef2ff_100%)]">
      <div className="mx-auto max-w-7xl px-4 py-6">
      <header className="mb-6 flex flex-col gap-4 rounded-2xl border border-white/70 bg-white/90 px-5 py-4 shadow-[0_20px_50px_rgba(15,23,42,0.08)] backdrop-blur md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.3em] text-amber-700">Tenant Workspace</p>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">AI Forged Tour Ops</h1>
          {activeTenantId ? <p className="mt-1 text-xs text-slate-500">Tenant scope: {activeTenantId}</p> : null}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <nav className="flex flex-wrap gap-2">
          {navItems.map((item) => (
            <Link
              key={item.to}
              to={item.to}
              className={`rounded-full px-4 py-2 text-sm font-medium transition ${isActive(location.pathname, item.to) ? 'bg-slate-900 text-white shadow-lg shadow-slate-900/20' : 'bg-slate-100 text-slate-700 hover:bg-white hover:text-slate-900'}`}
            >
              {item.label}
            </Link>
          ))}
          </nav>
          <button className="rounded-full bg-slate-100 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-200" onClick={() => auth.logout()}>
            Logout
          </button>
        </div>
      </header>
      <Outlet />
      </div>
    </div>
  );
};

const isActive = (pathname: string, target: string) => {
  if (target === '/app/itineraries') {
    return pathname === '/app/itineraries';
  }

  return pathname === target || pathname.startsWith(`${target}/`);
};
