import { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  tone?: 'neutral' | 'info' | 'success' | 'warning' | 'danger' | 'ai';
  className?: string;
}

const toneClasses: Record<NonNullable<Props['tone']>, string> = {
  neutral: 'bg-slate-100 text-slate-700',
  info: 'bg-sky-100 text-sky-700',
  success: 'bg-emerald-100 text-emerald-700',
  warning: 'bg-amber-100 text-amber-800',
  danger: 'bg-rose-100 text-rose-700',
  ai: 'bg-indigo-100 text-indigo-700'
};

export const Badge = ({ children, tone = 'neutral', className = '' }: Props) => (
  <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${toneClasses[tone]} ${className}`}>{children}</span>
);
