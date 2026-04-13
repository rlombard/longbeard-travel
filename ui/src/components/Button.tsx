import { ButtonHTMLAttributes } from 'react';

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  isLoading?: boolean;
};

export const Button = ({ children, className = '', isLoading, disabled, ...props }: Props) => (
  <button
    className={`rounded bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50 ${className}`}
    disabled={disabled || isLoading}
    {...props}
  >
    {isLoading ? 'Working...' : children}
  </button>
);
