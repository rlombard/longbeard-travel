export const BootSplash = ({
  eyebrow = 'Loading',
  title = 'Starting TourOps',
  detail = 'Preparing your workspace.'
}: {
  eyebrow?: string;
  title?: string;
  detail?: string;
}) => (
  <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(251,191,36,0.18),_transparent_32%),linear-gradient(180deg,_#fffaf2_0%,_#f8fafc_45%,_#eef2ff_100%)] px-4 py-10">
    <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
      <section className="rounded-[2rem] border border-white/70 bg-white/90 p-8 shadow-[0_24px_80px_rgba(15,23,42,0.10)] backdrop-blur">
        <p className="text-xs font-semibold uppercase tracking-[0.35em] text-amber-700">{eyebrow}</p>
        <h1 className="mt-4 text-4xl font-bold tracking-tight text-slate-900">{title}</h1>
        <p className="mt-4 max-w-2xl text-sm leading-6 text-slate-600">{detail}</p>

        <div className="mt-8 space-y-3">
          <div className="h-3 w-48 animate-pulse rounded-full bg-amber-200" />
          <div className="h-3 w-full animate-pulse rounded-full bg-slate-200" />
          <div className="h-3 w-5/6 animate-pulse rounded-full bg-slate-200" />
        </div>

        <div className="mt-10 grid gap-4 md:grid-cols-3">
          <SplashTile />
          <SplashTile />
          <SplashTile />
        </div>
      </section>

      <section className="rounded-[2rem] border border-slate-200 bg-white p-8 shadow-[0_24px_80px_rgba(15,23,42,0.08)]">
        <div className="flex items-center gap-3">
          <div className="h-3 w-3 animate-pulse rounded-full bg-amber-500" />
          <p className="text-sm font-medium text-slate-700">Loading</p>
        </div>
        <div className="mt-6 space-y-4">
          <div className="h-4 w-40 animate-pulse rounded-full bg-slate-200" />
          <div className="h-11 w-full animate-pulse rounded-2xl bg-slate-100" />
          <div className="h-11 w-36 animate-pulse rounded-full bg-slate-900/15" />
        </div>
      </section>
    </div>
  </div>
);

const SplashTile = () => (
  <article className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
    <div className="h-3 w-24 animate-pulse rounded-full bg-slate-200" />
    <div className="mt-3 h-3 w-full animate-pulse rounded-full bg-slate-200" />
    <div className="mt-2 h-3 w-5/6 animate-pulse rounded-full bg-slate-200" />
  </article>
);
