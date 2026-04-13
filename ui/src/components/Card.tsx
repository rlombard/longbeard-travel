import { ReactNode } from 'react';

interface Props {
  title: string;
  children: ReactNode;
}

export const Card = ({ title, children }: Props) => (
  <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
    <h2 className="mb-4 text-lg font-semibold">{title}</h2>
    {children}
  </section>
);
