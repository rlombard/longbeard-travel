import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const nav = [
  { to: '/platform/tenants', label: 'Tenants' },
  { to: '/platform/users', label: 'Users' }
];

export const PlatformLayout = () => {
  const location = useLocation();
  const auth = useAuth();

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,_#f7f4ea_0%,_#fff_30%,_#eef2f7_100%)]">
      <div className="mx-auto max-w-7xl px-4 py-6">
        <header className="mb-6 flex flex-col gap-4 rounded-2xl border border-white/70 bg-white/90 px-5 py-4 shadow-[0_20px_50px_rgba(15,23,42,0.08)] backdrop-blur md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-amber-700">Platform Management</p>
            <h1 className="text-2xl font-bold tracking-tight text-slate-900">AI Forged Tour Ops</h1>
            <p className="mt-1 text-xs text-slate-500">Deployment owner area. Tenant workflow stays separate.</p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <nav className="flex flex-wrap gap-2">
              {nav.map((item) => (
                <Link
                  key={item.to}
                  to={item.to}
                  className={`rounded-full px-4 py-2 text-sm font-medium transition ${isActive(location.pathname, item.to) ? 'bg-slate-900 text-white shadow-lg shadow-slate-900/20' : 'bg-slate-100 text-slate-700 hover:bg-white hover:text-slate-900'}`}
                >
                  {item.label}
                </Link>
              ))}
            </nav>
            <Link to="/app" className="rounded-full bg-amber-100 px-4 py-2 text-sm font-medium text-amber-900 hover:bg-amber-200">
              Operator App
            </Link>
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

const isActive = (pathname: string, target: string) => pathname === target || pathname.startsWith(`${target}/`);
