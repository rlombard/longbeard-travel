import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { getCustomer } from '../../services/customersApi';
import { CustomerRequest, CustomerVerificationStatus, PreferredContactMethod } from '../../types/customer';
import { useCreateCustomer, useCustomers } from './hooks';
import { customerVerificationOptions, CustomerVerificationBadge, preferredContactOptions } from './statusPresentation';
import { formatDateTime } from '../../utils/formatters';
import { Badge } from '../../components/Badge';

const emptyCustomer: CustomerRequest = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  nationality: '',
  countryOfResidence: '',
  dateOfBirth: '',
  preferredContactMethod: 'Email',
  notes: ''
};

type RecentActivityFilter = '' | 'Active' | 'LeadBooking' | 'Traveller';

export const CustomersPage = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const [nationality, setNationality] = useState('');
  const [countryOfResidence, setCountryOfResidence] = useState('');
  const [verificationStatus, setVerificationStatus] = useState<CustomerVerificationStatus | ''>('');
  const [recentActivity, setRecentActivity] = useState<RecentActivityFilter>('');
  const [draft, setDraft] = useState<CustomerRequest>(emptyCustomer);

  const customersQuery = useCustomers({
    searchTerm: searchTerm || undefined,
    nationality: nationality || undefined,
    countryOfResidence: countryOfResidence || undefined
  });
  const createMutation = useCreateCustomer();

  const customerIds = useMemo(() => (customersQuery.data ?? []).map((customer) => customer.id), [customersQuery.data]);
  const detailQuery = useQuery({
    queryKey: ['customers', 'detail-list', customerIds],
    queryFn: async () => Promise.all(customerIds.map((customerId) => getCustomer(customerId))),
    enabled: customerIds.length > 0
  });

  const rows = useMemo(() => {
    const detailsById = new Map((detailQuery.data ?? []).map((customer) => [customer.id, customer]));

    return (customersQuery.data ?? [])
      .map((customer) => ({
        summary: customer,
        detail: detailsById.get(customer.id)
      }))
      .filter((row) => {
        if (verificationStatus && row.detail?.kyc.verificationStatus !== verificationStatus) {
          return false;
        }

        if (recentActivity === 'LeadBooking' && !row.detail?.leadBookingIds.length) {
          return false;
        }

        if (recentActivity === 'Traveller' && !row.detail?.travellerBookings.length) {
          return false;
        }

        if (recentActivity === 'Active') {
          const active = Boolean(row.detail?.leadBookingIds.length || row.detail?.leadItineraryIds.length || row.detail?.leadQuoteIds.length || row.detail?.travellerBookings.length);
          if (!active) {
            return false;
          }
        }

        return true;
      });
  }, [customersQuery.data, detailQuery.data, recentActivity, verificationStatus]);

  return (
    <div className="space-y-6">
      <Card title="Customers">
        <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
          <div className="space-y-4">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <div className="mb-3 flex flex-wrap items-center gap-2">
                <Badge tone="info">{rows.length} shown</Badge>
                <Badge tone="warning">KYC hidden in list</Badge>
              </div>
              <div className="grid gap-3 md:grid-cols-5">
                <FormInput label="Search" value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} />
                <FormInput label="Nationality" value={nationality} onChange={(event) => setNationality(event.target.value)} />
                <FormInput label="Residence" value={countryOfResidence} onChange={(event) => setCountryOfResidence(event.target.value)} />
                <SelectDropdown label="Verification" value={verificationStatus} options={customerVerificationOptions} onChange={(event) => setVerificationStatus(event.target.value as CustomerVerificationStatus | '')} />
                <SelectDropdown
                  label="Recent Activity"
                  value={recentActivity}
                  options={[
                    { label: 'All', value: '' },
                    { label: 'Any active link', value: 'Active' },
                    { label: 'Lead booking', value: 'LeadBooking' },
                    { label: 'Traveller only', value: 'Traveller' }
                  ]}
                  onChange={(event) => setRecentActivity(event.target.value as RecentActivityFilter)}
                />
              </div>
            </div>

            {customersQuery.isLoading ? <p className="text-sm text-slate-500">Loading customers...</p> : null}
            {customersQuery.isError ? <p className="text-sm text-rose-600">{(customersQuery.error as Error).message}</p> : null}
            {rows.length === 0 && !customersQuery.isLoading ? <p className="text-sm text-slate-500">No customers match the current filters.</p> : null}
            {rows.length > 0 ? (
              <Table headers={['Customer', 'Contact', 'Verification', 'Activity', 'Updated', 'Action']}>
                {rows.map((row) => (
                  <tr key={row.summary.id} className="border-t border-slate-200">
                    <td className="px-3 py-3">
                      <p className="font-medium text-slate-900">{row.summary.fullName}</p>
                      <p className="text-xs text-slate-500">{row.summary.nationality ?? row.summary.countryOfResidence ?? 'No nationality/residence yet'}</p>
                    </td>
                    <td className="px-3 py-3">
                      <p className="text-slate-700">{row.summary.email ?? '-'}</p>
                      <p className="text-xs text-slate-500">{row.summary.phone ?? '-'}</p>
                    </td>
                    <td className="px-3 py-3">
                      {row.detail ? <CustomerVerificationBadge status={row.detail.kyc.verificationStatus} /> : <Badge tone="neutral">Loading</Badge>}
                    </td>
                    <td className="px-3 py-3">
                      {row.detail ? (
                        <div className="flex flex-wrap gap-1">
                          <Badge tone="info">Quotes {row.detail.leadQuoteIds.length}</Badge>
                          <Badge tone="info">Trips {row.detail.leadItineraryIds.length}</Badge>
                          <Badge tone="info">Bookings {row.detail.leadBookingIds.length}</Badge>
                        </div>
                      ) : null}
                    </td>
                    <td className="px-3 py-3 text-slate-700">{formatDateTime(row.summary.updatedAt)}</td>
                    <td className="px-3 py-3">
                      <Link className="text-sm font-medium text-slate-900 underline decoration-amber-300 underline-offset-4" to={`/customers/${row.summary.id}`}>
                        Open customer
                      </Link>
                    </td>
                  </tr>
                ))}
              </Table>
            ) : null}
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-slate-900">Create customer</h3>
              <Badge tone="info">Lead traveller</Badge>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <FormInput label="First Name" required value={draft.firstName} onChange={(event) => setDraft((current) => ({ ...current, firstName: event.target.value }))} />
              <FormInput label="Last Name" required value={draft.lastName} onChange={(event) => setDraft((current) => ({ ...current, lastName: event.target.value }))} />
              <FormInput label="Email" value={draft.email ?? ''} onChange={(event) => setDraft((current) => ({ ...current, email: event.target.value }))} />
              <FormInput label="Phone" value={draft.phone ?? ''} onChange={(event) => setDraft((current) => ({ ...current, phone: event.target.value }))} />
              <FormInput label="Nationality" value={draft.nationality ?? ''} onChange={(event) => setDraft((current) => ({ ...current, nationality: event.target.value }))} />
              <FormInput label="Country Of Residence" value={draft.countryOfResidence ?? ''} onChange={(event) => setDraft((current) => ({ ...current, countryOfResidence: event.target.value }))} />
              <FormInput label="Date Of Birth" type="date" value={draft.dateOfBirth ?? ''} onChange={(event) => setDraft((current) => ({ ...current, dateOfBirth: event.target.value }))} />
              <SelectDropdown label="Preferred Contact" value={draft.preferredContactMethod} options={preferredContactOptions} onChange={(event) => setDraft((current) => ({ ...current, preferredContactMethod: event.target.value as PreferredContactMethod }))} />
            </div>
            <div className="mt-3">
              <TextAreaField label="Operator Notes" rows={3} value={draft.notes ?? ''} onChange={(event) => setDraft((current) => ({ ...current, notes: event.target.value }))} />
            </div>
            <div className="mt-4 flex items-center gap-3">
              <Button
                isLoading={createMutation.isPending}
                onClick={async () => {
                  const customer = await createMutation.mutateAsync(draft);
                  setDraft(emptyCustomer);
                  navigate(`/customers/${customer.id}`);
                }}
              >
                Create customer
              </Button>
            </div>
            {createMutation.isError ? <p className="mt-3 text-sm text-rose-600">{(createMutation.error as Error).message}</p> : null}
          </div>
        </div>
      </Card>
    </div>
  );
};
