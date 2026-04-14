import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { useCreateBooking } from '../bookings/hooks';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { useGenerateQuote } from './hooks';

export const QuoteGeneratorPage = () => {
  const navigate = useNavigate();
  const [itineraryId, setItineraryId] = useState('');
  const [adults, setAdults] = useState(1);
  const [children, setChildren] = useState(0);
  const [currency, setCurrency] = useState('USD');
  const [markup, setMarkup] = useState(0.2);
  const generateMutation = useGenerateQuote();
  const createBookingMutation = useCreateBooking();

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();

    await generateMutation.mutateAsync({
      itineraryId,
      pax: adults + children,
      currency,
      markup
    });
  };

  const quote = generateMutation.data;

  return (
    <div className="space-y-6">
      <Card title="Generate Quote">
        <form className="grid gap-4 md:grid-cols-2" onSubmit={onSubmit}>
          <FormInput label="Itinerary ID" required value={itineraryId} onChange={(e) => setItineraryId(e.target.value)} />
          <SelectDropdown
            label="Currency"
            value={currency}
            options={[
              { label: 'USD', value: 'USD' },
              { label: 'EUR', value: 'EUR' },
              { label: 'GBP', value: 'GBP' }
            ]}
            onChange={(e) => setCurrency(e.target.value)}
          />
          <FormInput label="Adults" type="number" min={1} value={adults} onChange={(e) => setAdults(Number(e.target.value) || 1)} />
          <FormInput label="Children" type="number" min={0} value={children} onChange={(e) => setChildren(Number(e.target.value) || 0)} />
          <FormInput
            label="Markup % (0.2 = 20%)"
            type="number"
            min={0}
            step={0.01}
            value={markup}
            onChange={(e) => setMarkup(Number(e.target.value) || 0)}
          />
          <div className="flex items-end">
            <Button isLoading={generateMutation.isPending}>Generate Quote</Button>
          </div>
        </form>
        {generateMutation.isError ? (
          <p className="mt-3 text-sm text-red-600">{(generateMutation.error as Error).message}</p>
        ) : null}
      </Card>

      {quote ? (
        <Card title="Quote Result">
          <div className="mb-4 flex flex-col gap-3 rounded-lg border border-amber-200 bg-amber-50/70 px-4 py-3 text-sm text-slate-700 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.25em] text-amber-700">Quote Ready</p>
              <p className="font-mono text-xs text-slate-600">{quote.id}</p>
            </div>
            <Button
              type="button"
              isLoading={createBookingMutation.isPending}
              onClick={async () => {
                const booking = await createBookingMutation.mutateAsync({ quoteId: quote.id });
                navigate(`/bookings/${booking.id}`);
              }}
            >
              Convert To Booking
            </Button>
          </div>
          <div className="mb-4 grid gap-3 md:grid-cols-3">
            <p><strong>Total Cost:</strong> {quote.totalCost} {quote.currency}</p>
            <p><strong>Total Price:</strong> {quote.totalPrice} {quote.currency}</p>
            <p><strong>Margin:</strong> {quote.margin} {quote.currency}</p>
          </div>
          <Table headers={['Product', 'Base Cost', 'Adjusted Cost', 'Final Price']}>
            {quote.lineItems.map((line, index) => (
              <tr key={`${line.productId}-${index}`} className="border-t border-slate-200">
                <td className="px-3 py-2 font-mono text-xs">{line.productId}</td>
                <td className="px-3 py-2">{line.baseCost}</td>
                <td className="px-3 py-2">{line.adjustedCost}</td>
                <td className="px-3 py-2">{line.finalPrice}</td>
              </tr>
            ))}
          </Table>
          {createBookingMutation.isError ? (
            <p className="mt-3 text-sm text-red-600">{(createBookingMutation.error as Error).message}</p>
          ) : null}
        </Card>
      ) : null}
    </div>
  );
};
