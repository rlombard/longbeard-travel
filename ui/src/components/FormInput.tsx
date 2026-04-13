import { forwardRef, InputHTMLAttributes } from 'react';

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
}

export const FormInput = forwardRef<HTMLInputElement, Props>(({ label, className = '', ...props }, ref) => (
  <label className="block text-sm">
    <span className="mb-1 block font-medium text-slate-700">{label}</span>
    <input
      ref={ref}
      className={`w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none focus:ring-2 focus:ring-amber-200 ${className}`}
      {...props}
    />
  </label>
));

FormInput.displayName = 'FormInput';
