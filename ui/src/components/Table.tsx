import { ReactNode } from 'react';

interface Props {
  headers: string[];
  children: ReactNode;
}

export const Table = ({ headers, children }: Props) => (
  <div className="overflow-auto rounded border border-slate-200">
    <table className="min-w-full bg-white text-sm">
      <thead className="bg-slate-100 text-left">
        <tr>
          {headers.map((header) => (
            <th key={header} className="px-3 py-2 font-medium text-slate-700">
              {header}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>{children}</tbody>
    </table>
  </div>
);
