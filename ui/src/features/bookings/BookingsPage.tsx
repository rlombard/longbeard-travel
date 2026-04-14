import { Link } from 'react-router-dom';
import { Card } from '../../components/Card';
import { Table } from '../../components/Table';
import { useBookings } from './hooks';
import { StatusBadge } from './statusPresentation';

const formatDate = (value: string) => new Date(value).toLocaleString();

export const BookingsPage = () => {
  const { data: bookings = [], isLoading, isError, error } = useBookings();

  return (
    <div className="space-y-6">
      <Card title="Bookings">
        {isLoading ? <p className="text-sm text-slate-500">Loading bookings...</p> : null}
        {isError ? <p className="text-sm text-red-600">{(error as Error).message}</p> : null}
        {!isLoading && bookings.length === 0 ? <p className="text-sm text-slate-500">No bookings yet. Convert a quote to create the first booking.</p> : null}
        {bookings.length > 0 ? (
          <Table headers={['Booking ID', 'Status', 'Created', 'Items', 'Action']}>
            {bookings.map((booking) => (
              <tr key={booking.id} className="border-t border-slate-200">
                <td className="px-3 py-3 font-mono text-xs text-slate-700">{booking.id}</td>
                <td className="px-3 py-3"><StatusBadge status={booking.status} /></td>
                <td className="px-3 py-3 text-slate-700">{formatDate(booking.createdAt)}</td>
                <td className="px-3 py-3 text-slate-700">{booking.itemCount}</td>
                <td className="px-3 py-3">
                  <Link className="text-sm font-medium text-slate-900 underline decoration-amber-300 underline-offset-4" to={`/bookings/${booking.id}`}>
                    Open booking
                  </Link>
                </td>
              </tr>
            ))}
          </Table>
        ) : null}
      </Card>
    </div>
  );
};
