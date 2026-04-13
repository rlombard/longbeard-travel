import { FormEvent, useMemo, useState } from 'react';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { useProducts } from '../products/hooks';
import { useCreateItinerary } from './hooks';

interface DayItem {
  dayNumber: number;
  productId: string;
  quantity: number;
  notes?: string;
}

export const ItineraryBuilderPage = () => {
  const [startDate, setStartDate] = useState('');
  const [duration, setDuration] = useState(1);
  const [itemsByDay, setItemsByDay] = useState<Record<number, DayItem>>({});

  const { data: products = [] } = useProducts();
  const createMutation = useCreateItinerary();

  const days = useMemo(() => Array.from({ length: duration }, (_, idx) => idx + 1), [duration]);
  const productOptions = [
    { label: 'Select product', value: '' },
    ...products.map((p) => ({ label: p.name, value: p.id }))
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
    <Card title="Itinerary Builder">
      <form className="space-y-4" onSubmit={onSubmit}>
        <div className="grid gap-4 md:grid-cols-2">
          <FormInput label="Start Date" type="date" value={startDate} required onChange={(e) => setStartDate(e.target.value)} />
          <FormInput
            label="Duration"
            type="number"
            min={1}
            value={duration}
            onChange={(e) => setDuration(Number(e.target.value) || 1)}
          />
        </div>

        <div className="space-y-4">
          {days.map((day) => (
            <div key={day} className="rounded border border-slate-200 p-3">
              <h3 className="mb-2 text-sm font-semibold">Day {day}</h3>
              <div className="grid gap-3 md:grid-cols-3">
                <SelectDropdown
                  label="Product"
                  value={itemsByDay[day]?.productId ?? ''}
                  options={productOptions}
                  onChange={(e) => updateDay(day, { productId: e.target.value })}
                />
                <FormInput
                  label="Quantity"
                  type="number"
                  min={1}
                  value={itemsByDay[day]?.quantity ?? 1}
                  onChange={(e) => updateDay(day, { quantity: Number(e.target.value) || 1 })}
                />
                <FormInput
                  label="Notes"
                  value={itemsByDay[day]?.notes ?? ''}
                  onChange={(e) => updateDay(day, { notes: e.target.value })}
                />
              </div>
            </div>
          ))}
        </div>

        <Button isLoading={createMutation.isPending}>Create Itinerary</Button>
        {createMutation.isSuccess ? (
          <p className="text-sm text-emerald-700">Created itinerary: {createMutation.data.id}</p>
        ) : null}
        {createMutation.isError ? <p className="text-sm text-red-600">{(createMutation.error as Error).message}</p> : null}
      </form>
    </Card>
  );
};
