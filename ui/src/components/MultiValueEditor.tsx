import { KeyboardEvent, useState } from 'react';
import { Badge } from './Badge';

interface Props {
  label: string;
  values: string[];
  placeholder?: string;
  onChange: (values: string[]) => void;
}

const normalize = (value: string) => value.trim();

export const MultiValueEditor = ({ label, values, placeholder, onChange }: Props) => {
  const [draft, setDraft] = useState('');

  const addValue = (value: string) => {
    const next = normalize(value);
    if (!next || values.includes(next)) {
      return;
    }

    onChange([...values, next]);
    setDraft('');
  };

  const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key !== 'Enter' && event.key !== ',') {
      return;
    }

    event.preventDefault();
    addValue(draft);
  };

  return (
    <label className="block text-sm">
      <span className="mb-1 block font-medium text-slate-700">{label}</span>
      <div className="rounded border border-slate-300 bg-white px-3 py-2">
        <div className="mb-2 flex flex-wrap gap-2">
          {values.map((value) => (
            <button
              key={value}
              type="button"
              className="rounded-full"
              onClick={() => onChange(values.filter((item) => item !== value))}
            >
              <Badge tone="info" className="cursor-pointer hover:bg-sky-200">
                {value} x
              </Badge>
            </button>
          ))}
          {values.length === 0 ? <span className="text-xs text-slate-400">No values yet</span> : null}
        </div>
        <input
          className="w-full border-0 px-0 py-1 text-sm focus:outline-none focus:ring-0"
          placeholder={placeholder}
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          onKeyDown={onKeyDown}
          onBlur={() => addValue(draft)}
        />
      </div>
    </label>
  );
};
