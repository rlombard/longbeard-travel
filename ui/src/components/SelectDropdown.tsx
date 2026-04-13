import { SelectHTMLAttributes } from 'react';

interface Option {
  label: string;
  value: string;
}

interface Props extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  options: Option[];
}

export const SelectDropdown = ({ label, options, className = '', ...props }: Props) => (
  <label className="block text-sm">
    <span className="mb-1 block font-medium text-slate-700">{label}</span>
    <select
      className={`w-full rounded border border-slate-300 px-3 py-2 text-sm focus:border-slate-500 focus:outline-none ${className}`}
      {...props}
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  </label>
);
