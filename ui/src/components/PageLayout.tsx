import { Link, Outlet, useLocation } from 'react-router-dom';

const nav = [
  { to: '/products', label: 'Products' },
  { to: '/itineraries', label: 'Itinerary Builder' },
  { to: '/quotes', label: 'Quote Generator' }
];

export const PageLayout = () => {
  const location = useLocation();

  return (
    <div className="mx-auto min-h-screen max-w-6xl px-4 py-6">
      <header className="mb-6 flex items-center justify-between rounded-lg border border-slate-200 bg-white px-4 py-3 shadow-sm">
        <h1 className="text-xl font-bold">AI Forged Tour Ops – Operator Console</h1>
        <nav className="flex gap-2">
          {nav.map((item) => (
            <Link
              key={item.to}
              to={item.to}
              className={`rounded px-3 py-2 text-sm ${location.pathname === item.to ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-700'}`}
            >
              {item.label}
            </Link>
          ))}
        </nav>
      </header>
      <Outlet />
    </div>
  );
};
