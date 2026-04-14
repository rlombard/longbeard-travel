import { Link, Outlet, useLocation } from 'react-router-dom';

const nav = [
  { to: '/suppliers', label: 'Suppliers' },
  { to: '/products', label: 'Products' },
  { to: '/itineraries', label: 'Itineraries' },
  { to: '/itineraries/ai', label: 'AI Itinerary Assist' },
  { to: '/quotes', label: 'Quote Generator' },
  { to: '/bookings', label: 'Bookings' },
  { to: '/invoices', label: 'Invoices' },
  { to: '/customers', label: 'Customers' },
  { to: '/emails', label: 'Emails' },
  { to: '/operations', label: 'Operations' }
];

export const PageLayout = () => {
  const location = useLocation();

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.18),_transparent_32%),linear-gradient(180deg,_#fffaf2_0%,_#f8fafc_45%,_#eef2ff_100%)]">
      <div className="mx-auto max-w-7xl px-4 py-6">
      <header className="mb-6 flex flex-col gap-4 rounded-2xl border border-white/70 bg-white/90 px-5 py-4 shadow-[0_20px_50px_rgba(15,23,42,0.08)] backdrop-blur md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.3em] text-amber-700">Operator Console</p>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">AI Forged Tour Ops</h1>
        </div>
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
      </header>
      <Outlet />
      </div>
    </div>
  );
};

const isActive = (pathname: string, target: string) => {
  if (target === '/itineraries') {
    return pathname === '/itineraries';
  }

  return pathname === target || pathname.startsWith(`${target}/`);
};
