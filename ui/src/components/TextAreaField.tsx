import { TextareaHTMLAttributes } from 'react';

interface Props extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
}

export const TextAreaField = ({ label, className = '', rows = 3, ...props }: Props) => (
  <label className="block text-sm">
    <span className="mb-1 block font-medium text-slate-700">{label}</span>
    <textarea
      rows={rows}
      className={`w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none focus:ring-2 focus:ring-amber-200 ${className}`}
      {...props}
    />
  </label>
);
