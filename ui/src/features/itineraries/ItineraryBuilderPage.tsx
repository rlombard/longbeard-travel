import { FormEvent, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { useProducts } from '../products/hooks';
import { useCreateItinerary, useItinerary } from './hooks';
import { formatDateOnly, formatDateTime } from '../../utils/formatters';
import { Badge } from '../../components/Badge';

interface DayItem {
  dayNumber: number;
  productId: string;
  quantity: number;
  notes?: string;
}

export const ItineraryBuilderPage = () => {
  const [startDate, setStartDate] = useState('');
  const [duration, setDuration] = useState(3);
  const [itemsByDay, setItemsByDay] = useState<Record<number, DayItem>>({});
  const [lookupId, setLookupId] = useState('');
  const [activeLookupId, setActiveLookupId] = useState<string>();

  const { data: products = [] } = useProducts();
  const createMutation = useCreateItinerary();
  const lookupQuery = useItinerary(activeLookupId);

  const days = useMemo(() => Array.from({ length: duration }, (_, idx) => idx + 1), [duration]);
  const productOptions = [
    { label: 'Select product', value: '' },
    ...products.map((product) => ({ label: product.name, value: product.id }))
  ];

  const updateDay = (day: number, value: Partial<DayItem>) => {
    setItemsByDay((current) => ({
      ...current,
      [day]: {
        dayNumber: day,
        productId: current[day]?.productId ?? '',
        quantity: current[day]?.quantity ?? 1,
        notes: current[day]?.notes,
        ...value
      }
    }));
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();

    const items = days
      .map((day) => itemsByDay[day])
      .filter((item): item is DayItem => Boolean(item?.productId));

    await createMutation.mutateAsync({
      startDate,
      duration,
      items
    });
  };

  return (
    <div className="space-y-6">
      <Card title="Itineraries">
        <div className="mb-5 flex flex-col gap-3 rounded-2xl border border-indigo-200 bg-indigo-50/80 px-4 py-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.25em] text-indigo-700">AI Assist Ready</p>
            <p className="text-sm text-slate-700">Use AI product assist and draft generation, then save only after operator review.</p>
          </div>
          <Link className="inline-flex rounded-full bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-500" to="/app/itineraries/ai">
            Open AI Itinerary Assist
          </Link>
        </div>

        <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
          <form className="space-y-4" onSubmit={onSubmit}>
            <div className="grid gap-4 md:grid-cols-2">
              <FormInput label="Start Date" type="date" value={startDate} required onChange={(event) => setStartDate(event.target.value)} />
              <FormInput
                label="Duration"
                type="number"
                min={1}
                value={duration}
                onChange={(event) => setDuration(Math.max(1, Number(event.target.value) || 1))}
              />
            </div>

            <div className="space-y-4">
              {days.map((day) => (
                <div key={day} className="rounded-xl border border-slate-200 bg-slate-50 p-4">
                  <div className="mb-3 flex items-center justify-between">
                    <h3 className="text-sm font-semibold text-slate-900">Day {day}</h3>
                    <Badge tone="neutral">Manual draft</Badge>
                  </div>
                  <div className="grid gap-3 md:grid-cols-3">
                    <SelectDropdown
                      label="Product"
                      value={itemsByDay[day]?.productId ?? ''}
                      options={productOptions}
                      onChange={(event) => updateDay(day, { productId: event.target.value })}
                    />
                    <FormInput
                      label="Quantity"
                      type="number"
                      min={1}
                      value={itemsByDay[day]?.quantity ?? 1}
                      onChange={(event) => updateDay(day, { quantity: Math.max(1, Number(event.target.value) || 1) })}
                    />
                    <FormInput
                      label="Notes"
                      value={itemsByDay[day]?.notes ?? ''}
                      onChange={(event) => updateDay(day, { notes: event.target.value })}
                    />
                  </div>
                </div>
              ))}
            </div>

            <div className="flex items-center gap-3">
              <Button isLoading={createMutation.isPending}>Save Itinerary</Button>
              <p className="text-xs text-slate-500">This creates a normal persisted itinerary. No AI step here.</p>
            </div>

            {createMutation.isSuccess ? (
              <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                Saved itinerary <span className="font-mono">{createMutation.data.id}</span>
              </div>
            ) : null}
            {createMutation.isError ? <p className="text-sm text-rose-600">{(createMutation.error as Error).message}</p> : null}
          </form>

          <div className="space-y-4">
            <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Open Persisted Itinerary</h3>
              <div className="flex gap-3">
                <FormInput label="Itinerary ID" value={lookupId} onChange={(event) => setLookupId(event.target.value)} />
                <div className="flex items-end">
                  <Button type="button" onClick={() => setActiveLookupId(lookupId || undefined)}>
                    Open
                  </Button>
                </div>
              </div>
            </div>

            {lookupQuery.isLoading ? <p className="text-sm text-slate-500">Loading itinerary...</p> : null}
            {lookupQuery.isError ? <p className="text-sm text-rose-600">{(lookupQuery.error as Error).message}</p> : null}
            {lookupQuery.data ? (
              <div className="space-y-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge tone="success">Saved</Badge>
                  {lookupQuery.data.leadCustomerName ? <Badge tone="info">{lookupQuery.data.leadCustomerName}</Badge> : null}
                </div>
                <div className="grid gap-3 md:grid-cols-2">
                  <p className="text-sm text-slate-700"><strong>ID:</strong> <span className="font-mono text-xs">{lookupQuery.data.id}</span></p>
                  <p className="text-sm text-slate-700"><strong>Created:</strong> {formatDateTime(lookupQuery.data.createdAt)}</p>
                  <p className="text-sm text-slate-700"><strong>Start:</strong> {formatDateOnly(lookupQuery.data.startDate)}</p>
                  <p className="text-sm text-slate-700"><strong>Duration:</strong> {lookupQuery.data.duration} days</p>
                </div>
                <Table headers={['Day', 'Product', 'Quantity', 'Notes']}>
                  {lookupQuery.data.items.map((item) => (
                    <tr key={item.id} className="border-t border-slate-200">
                      <td className="px-3 py-2 text-slate-700">{item.dayNumber}</td>
                      <td className="px-3 py-2 font-mono text-xs text-slate-700">{item.productId}</td>
                      <td className="px-3 py-2 text-slate-700">{item.quantity}</td>
                      <td className="px-3 py-2 text-slate-700">{item.notes ?? '-'}</td>
                    </tr>
                  ))}
                </Table>
              </div>
            ) : null}
          </div>
        </div>
      </Card>
    </div>
  );
};
