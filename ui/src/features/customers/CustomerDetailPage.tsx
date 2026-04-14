import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { MultiValueEditor } from '../../components/MultiValueEditor';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { Badge } from '../../components/Badge';
import { useBooking, useBookings } from '../bookings/hooks';
import { getBooking } from '../../services/bookingsApi';
import { getQuote } from '../../services/quotesApi';
import { getItinerary } from '../../services/itinerariesApi';
import { useCustomer, useAttachCustomerToBooking, useAttachCustomerToItinerary, useAttachCustomerToQuote, useRemoveBookingTraveller, useUpdateCustomer, useUpdateCustomerKyc, useUpdateCustomerPreferences, useUpsertBookingTraveller } from './hooks';
import { CustomerKycRequest, CustomerPreferenceRequest, CustomerRequest, PreferredContactMethod } from '../../types/customer';
import {
  budgetBandOptions,
  CustomerVerificationBadge,
  preferredContactOptions,
  travelPaceOptions,
  valueLeaningOptions
} from './statusPresentation';
import { formatCurrency, formatDateOnly, formatDateTime, humanize, maskValue } from '../../utils/formatters';

type TabKey = 'profile' | 'preferences' | 'kyc' | 'relationships';

const tabs: { key: TabKey; label: string }[] = [
  { key: 'profile', label: 'Profile' },
  { key: 'preferences', label: 'Preferences' },
  { key: 'kyc', label: 'KYC' },
  { key: 'relationships', label: 'Links' }
];

export const CustomerDetailPage = () => {
  const { customerId } = useParams();
  const [activeTab, setActiveTab] = useState<TabKey>('profile');
  const [quoteLinkId, setQuoteLinkId] = useState('');
  const [itineraryLinkId, setItineraryLinkId] = useState('');
  const [bookingLinkId, setBookingLinkId] = useState('');
  const [travellerBookingId, setTravellerBookingId] = useState('');
  const [travellerRelationship, setTravellerRelationship] = useState('');
  const [travellerNotes, setTravellerNotes] = useState('');

  const customerQuery = useCustomer(customerId);
  const bookingsQuery = useBookings();
  const selectedTravellerBooking = useBooking(travellerBookingId || undefined);
  const updateCustomerMutation = useUpdateCustomer();
  const updateKycMutation = useUpdateCustomerKyc();
  const updatePreferencesMutation = useUpdateCustomerPreferences();
  const attachQuoteMutation = useAttachCustomerToQuote();
  const attachItineraryMutation = useAttachCustomerToItinerary();
  const attachBookingMutation = useAttachCustomerToBooking();
  const upsertTravellerMutation = useUpsertBookingTraveller();
  const removeTravellerMutation = useRemoveBookingTraveller();

  const [profileDraft, setProfileDraft] = useState<CustomerRequest | null>(null);
  const [kycDraft, setKycDraft] = useState<CustomerKycRequest | null>(null);
  const [preferenceDraft, setPreferenceDraft] = useState<CustomerPreferenceRequest | null>(null);

  useEffect(() => {
    if (!customerQuery.data) {
      return;
    }

    setProfileDraft({
      firstName: customerQuery.data.firstName,
      lastName: customerQuery.data.lastName,
      email: customerQuery.data.email ?? '',
      phone: customerQuery.data.phone ?? '',
      nationality: customerQuery.data.nationality ?? '',
      countryOfResidence: customerQuery.data.countryOfResidence ?? '',
      dateOfBirth: customerQuery.data.dateOfBirth ?? '',
      preferredContactMethod: customerQuery.data.preferredContactMethod,
      notes: customerQuery.data.notes ?? ''
    });
    setKycDraft({
      passportNumber: customerQuery.data.kyc.passportNumber ?? '',
      documentReference: customerQuery.data.kyc.documentReference ?? '',
      passportExpiry: customerQuery.data.kyc.passportExpiry ?? '',
      issuingCountry: customerQuery.data.kyc.issuingCountry ?? '',
      visaNotes: customerQuery.data.kyc.visaNotes ?? '',
      emergencyContactName: customerQuery.data.kyc.emergencyContactName ?? '',
      emergencyContactPhone: customerQuery.data.kyc.emergencyContactPhone ?? '',
      emergencyContactRelationship: customerQuery.data.kyc.emergencyContactRelationship ?? '',
      verificationStatus: customerQuery.data.kyc.verificationStatus,
      verificationNotes: customerQuery.data.kyc.verificationNotes ?? '',
      profileDataConsentGranted: customerQuery.data.kyc.profileDataConsentGranted,
      kycDataConsentGranted: customerQuery.data.kyc.kycDataConsentGranted
    });
    setPreferenceDraft({
      budgetBand: customerQuery.data.preferences.budgetBand,
      accommodationPreference: customerQuery.data.preferences.accommodationPreference ?? '',
      roomPreference: customerQuery.data.preferences.roomPreference ?? '',
      dietaryRequirements: customerQuery.data.preferences.dietaryRequirements,
      activityPreferences: customerQuery.data.preferences.activityPreferences,
      accessibilityRequirements: customerQuery.data.preferences.accessibilityRequirements,
      paceOfTravel: customerQuery.data.preferences.paceOfTravel,
      valueLeaning: customerQuery.data.preferences.valueLeaning,
      transportPreferences: customerQuery.data.preferences.transportPreferences,
      specialOccasions: customerQuery.data.preferences.specialOccasions,
      dislikedExperiences: customerQuery.data.preferences.dislikedExperiences,
      preferredDestinations: customerQuery.data.preferences.preferredDestinations,
      avoidedDestinations: customerQuery.data.preferences.avoidedDestinations,
      operatorNotes: customerQuery.data.preferences.operatorNotes ?? ''
    });
  }, [customerQuery.data]);

  const relatedDataQuery = useQuery({
    queryKey: ['customer', customerId, 'relationships', customerQuery.data?.leadQuoteIds, customerQuery.data?.leadItineraryIds, customerQuery.data?.leadBookingIds],
    enabled: Boolean(customerQuery.data),
    queryFn: async () => {
      const customer = customerQuery.data!;
      const [quotes, itineraries, bookings] = await Promise.all([
        Promise.all(customer.leadQuoteIds.map((id) => getQuote(id))),
        Promise.all(customer.leadItineraryIds.map((id) => getItinerary(id))),
        Promise.all(customer.leadBookingIds.map((id) => getBooking(id)))
      ]);

      return { quotes, itineraries, bookings };
    }
  });

  const bookingOptions = useMemo(
    () => [{ label: 'Select booking', value: '' }, ...(bookingsQuery.data ?? []).map((booking) => ({ label: `${booking.id.slice(0, 8)} ${booking.leadCustomerName ?? ''}`.trim(), value: booking.id }))],
    [bookingsQuery.data]
  );

  if (customerQuery.isLoading || !profileDraft || !kycDraft || !preferenceDraft) {
    return <Card title="Customer Detail"><p className="text-sm text-slate-500">Loading customer...</p></Card>;
  }

  if (customerQuery.isError || !customerQuery.data) {
    return <Card title="Customer Detail"><p className="text-sm text-rose-600">{customerQuery.isError ? (customerQuery.error as Error).message : 'Customer not found.'}</p></Card>;
  }

  const customer = customerQuery.data;

  return (
    <div className="space-y-6">
      <Card title="Customer Detail">
        <div className="mb-5 grid gap-4 xl:grid-cols-[1.15fr_0.85fr]">
          <div>
            <div className="mb-3 flex flex-wrap items-center gap-2">
              <h2 className="text-xl font-semibold text-slate-900">{customer.fullName}</h2>
              <CustomerVerificationBadge status={customer.kyc.verificationStatus} />
              <Badge tone="info">{humanize(customer.preferredContactMethod)}</Badge>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <p className="text-sm text-slate-700"><strong>Email:</strong> {customer.email ?? '-'}</p>
              <p className="text-sm text-slate-700"><strong>Phone:</strong> {customer.phone ?? '-'}</p>
              <p className="text-sm text-slate-700"><strong>Nationality:</strong> {customer.nationality ?? '-'}</p>
              <p className="text-sm text-slate-700"><strong>Residence:</strong> {customer.countryOfResidence ?? '-'}</p>
              <p className="text-sm text-slate-700"><strong>Date Of Birth:</strong> {formatDateOnly(customer.dateOfBirth)}</p>
              <p className="text-sm text-slate-700"><strong>Updated:</strong> {formatDateTime(customer.updatedAt)}</p>
            </div>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
            <p className="mb-3 text-sm font-semibold text-slate-900">Sensitive data summary</p>
            <div className="space-y-2 text-sm text-slate-700">
              <p><strong>Passport:</strong> {maskValue(customer.kyc.passportNumber)}</p>
              <p><strong>Document Ref:</strong> {maskValue(customer.kyc.documentReference)}</p>
              <p><strong>Passport Expiry:</strong> {formatDateOnly(customer.kyc.passportExpiry)}</p>
              <p><strong>Profile Consent:</strong> {customer.kyc.profileDataConsentGranted ? 'Granted' : 'Not granted'}</p>
              <p><strong>KYC Consent:</strong> {customer.kyc.kycDataConsentGranted ? 'Granted' : 'Not granted'}</p>
            </div>
          </div>
        </div>

        <div className="mb-5 flex flex-wrap gap-2">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              type="button"
              className={`rounded-full px-4 py-2 text-sm font-medium ${activeTab === tab.key ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-700'}`}
              onClick={() => setActiveTab(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {activeTab === 'profile' ? (
          <div className="space-y-4">
            <div className="grid gap-3 md:grid-cols-2">
              <FormInput label="First Name" value={profileDraft.firstName} onChange={(event) => setProfileDraft((current) => current ? { ...current, firstName: event.target.value } : current)} />
              <FormInput label="Last Name" value={profileDraft.lastName} onChange={(event) => setProfileDraft((current) => current ? { ...current, lastName: event.target.value } : current)} />
              <FormInput label="Email" value={profileDraft.email ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, email: event.target.value } : current)} />
              <FormInput label="Phone" value={profileDraft.phone ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, phone: event.target.value } : current)} />
              <FormInput label="Nationality" value={profileDraft.nationality ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, nationality: event.target.value } : current)} />
              <FormInput label="Country Of Residence" value={profileDraft.countryOfResidence ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, countryOfResidence: event.target.value } : current)} />
              <FormInput label="Date Of Birth" type="date" value={profileDraft.dateOfBirth ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, dateOfBirth: event.target.value } : current)} />
              <SelectDropdown label="Preferred Contact" value={profileDraft.preferredContactMethod} options={preferredContactOptions} onChange={(event) => setProfileDraft((current) => current ? { ...current, preferredContactMethod: event.target.value as PreferredContactMethod } : current)} />
            </div>
            <TextAreaField label="Notes" rows={4} value={profileDraft.notes ?? ''} onChange={(event) => setProfileDraft((current) => current ? { ...current, notes: event.target.value } : current)} />
            <Button isLoading={updateCustomerMutation.isPending} onClick={() => void updateCustomerMutation.mutateAsync({ customerId: customer.id, payload: profileDraft })}>
              Save profile
            </Button>
          </div>
        ) : null}

        {activeTab === 'preferences' ? (
          <div className="space-y-4">
            <div className="grid gap-3 md:grid-cols-2">
              <SelectDropdown label="Budget Band" value={preferenceDraft.budgetBand} options={budgetBandOptions} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, budgetBand: event.target.value as CustomerPreferenceRequest['budgetBand'] } : current)} />
              <FormInput label="Accommodation" value={preferenceDraft.accommodationPreference ?? ''} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, accommodationPreference: event.target.value } : current)} />
              <FormInput label="Room Preference" value={preferenceDraft.roomPreference ?? ''} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, roomPreference: event.target.value } : current)} />
              <SelectDropdown label="Pace" value={preferenceDraft.paceOfTravel} options={travelPaceOptions} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, paceOfTravel: event.target.value as CustomerPreferenceRequest['paceOfTravel'] } : current)} />
              <SelectDropdown label="Value Leaning" value={preferenceDraft.valueLeaning} options={valueLeaningOptions} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, valueLeaning: event.target.value as CustomerPreferenceRequest['valueLeaning'] } : current)} />
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
              <MultiValueEditor label="Dietary Requirements" values={preferenceDraft.dietaryRequirements} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, dietaryRequirements: values } : current)} />
              <MultiValueEditor label="Activity Preferences" values={preferenceDraft.activityPreferences} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, activityPreferences: values } : current)} />
              <MultiValueEditor label="Accessibility Needs" values={preferenceDraft.accessibilityRequirements} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, accessibilityRequirements: values } : current)} />
              <MultiValueEditor label="Transport Preferences" values={preferenceDraft.transportPreferences} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, transportPreferences: values } : current)} />
              <MultiValueEditor label="Special Occasions" values={preferenceDraft.specialOccasions} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, specialOccasions: values } : current)} />
              <MultiValueEditor label="Disliked Experiences" values={preferenceDraft.dislikedExperiences} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, dislikedExperiences: values } : current)} />
              <MultiValueEditor label="Preferred Destinations" values={preferenceDraft.preferredDestinations} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, preferredDestinations: values } : current)} />
              <MultiValueEditor label="Avoided Destinations" values={preferenceDraft.avoidedDestinations} onChange={(values) => setPreferenceDraft((current) => current ? { ...current, avoidedDestinations: values } : current)} />
            </div>

            <TextAreaField label="Operator Notes" rows={4} value={preferenceDraft.operatorNotes ?? ''} onChange={(event) => setPreferenceDraft((current) => current ? { ...current, operatorNotes: event.target.value } : current)} />
            <Button isLoading={updatePreferencesMutation.isPending} onClick={() => void updatePreferencesMutation.mutateAsync({ customerId: customer.id, payload: preferenceDraft })}>
              Save preferences
            </Button>
          </div>
        ) : null}

        {activeTab === 'kyc' ? (
          <div className="space-y-4">
            <div className="grid gap-3 md:grid-cols-2">
              <FormInput label="Passport Number" value={kycDraft.passportNumber ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, passportNumber: event.target.value } : current)} />
              <FormInput label="Document Reference" value={kycDraft.documentReference ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, documentReference: event.target.value } : current)} />
              <FormInput label="Passport Expiry" type="date" value={kycDraft.passportExpiry ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, passportExpiry: event.target.value } : current)} />
              <FormInput label="Issuing Country" value={kycDraft.issuingCountry ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, issuingCountry: event.target.value } : current)} />
              <FormInput label="Emergency Contact Name" value={kycDraft.emergencyContactName ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, emergencyContactName: event.target.value } : current)} />
              <FormInput label="Emergency Contact Phone" value={kycDraft.emergencyContactPhone ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, emergencyContactPhone: event.target.value } : current)} />
              <FormInput label="Emergency Contact Relationship" value={kycDraft.emergencyContactRelationship ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, emergencyContactRelationship: event.target.value } : current)} />
              <SelectDropdown
                label="Verification Status"
                value={kycDraft.verificationStatus}
                options={[
                  { label: 'Not Started', value: 'NotStarted' },
                  { label: 'Pending', value: 'Pending' },
                  { label: 'Verified', value: 'Verified' },
                  { label: 'Rejected', value: 'Rejected' },
                  { label: 'Expired', value: 'Expired' }
                ]}
                onChange={(event) => setKycDraft((current) => current ? { ...current, verificationStatus: event.target.value as CustomerKycRequest['verificationStatus'] } : current)}
              />
            </div>

            <TextAreaField label="Visa Notes" rows={3} value={kycDraft.visaNotes ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, visaNotes: event.target.value } : current)} />
            <TextAreaField label="Verification Notes" rows={3} value={kycDraft.verificationNotes ?? ''} onChange={(event) => setKycDraft((current) => current ? { ...current, verificationNotes: event.target.value } : current)} />

            <div className="flex flex-wrap gap-4 text-sm text-slate-700">
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={kycDraft.profileDataConsentGranted} onChange={(event) => setKycDraft((current) => current ? { ...current, profileDataConsentGranted: event.target.checked } : current)} />
                Profile data consent granted
              </label>
              <label className="flex items-center gap-2">
                <input type="checkbox" checked={kycDraft.kycDataConsentGranted} onChange={(event) => setKycDraft((current) => current ? { ...current, kycDataConsentGranted: event.target.checked } : current)} />
                KYC data consent granted
              </label>
            </div>

            <Button isLoading={updateKycMutation.isPending} onClick={() => void updateKycMutation.mutateAsync({ customerId: customer.id, payload: kycDraft })}>
              Save KYC
            </Button>
          </div>
        ) : null}

        {activeTab === 'relationships' ? (
          <div className="space-y-6">
            <div className="grid gap-4 xl:grid-cols-[1fr_1fr]">
              <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                <h3 className="mb-3 text-sm font-semibold text-slate-900">Attach as lead customer</h3>
                <div className="space-y-3">
                  <div className="flex gap-3">
                    <FormInput label="Quote ID" value={quoteLinkId} onChange={(event) => setQuoteLinkId(event.target.value)} />
                    <div className="flex items-end">
                      <Button type="button" isLoading={attachQuoteMutation.isPending} onClick={() => void attachQuoteMutation.mutateAsync({ customerId: customer.id, quoteId: quoteLinkId })}>
                        Attach
                      </Button>
                    </div>
                  </div>
                  <div className="flex gap-3">
                    <FormInput label="Itinerary ID" value={itineraryLinkId} onChange={(event) => setItineraryLinkId(event.target.value)} />
                    <div className="flex items-end">
                      <Button type="button" isLoading={attachItineraryMutation.isPending} onClick={() => void attachItineraryMutation.mutateAsync({ customerId: customer.id, itineraryId: itineraryLinkId })}>
                        Attach
                      </Button>
                    </div>
                  </div>
                  <div className="flex gap-3">
                    <FormInput label="Booking ID" value={bookingLinkId} onChange={(event) => setBookingLinkId(event.target.value)} />
                    <div className="flex items-end">
                      <Button type="button" isLoading={attachBookingMutation.isPending} onClick={() => void attachBookingMutation.mutateAsync({ customerId: customer.id, bookingId: bookingLinkId })}>
                        Attach
                      </Button>
                    </div>
                  </div>
                </div>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <h3 className="mb-3 text-sm font-semibold text-slate-900">Manage party-member link</h3>
                <div className="grid gap-3 md:grid-cols-2">
                  <SelectDropdown label="Booking" value={travellerBookingId} options={bookingOptions} onChange={(event) => setTravellerBookingId(event.target.value)} />
                  <FormInput label="Relationship" value={travellerRelationship} onChange={(event) => setTravellerRelationship(event.target.value)} />
                </div>
                <TextAreaField label="Notes" rows={3} value={travellerNotes} onChange={(event) => setTravellerNotes(event.target.value)} />
                <div className="mt-3 flex gap-3">
                  <Button
                    type="button"
                    isLoading={upsertTravellerMutation.isPending}
                    disabled={!travellerBookingId}
                    onClick={() =>
                      void upsertTravellerMutation.mutateAsync({
                        customerId: customer.id,
                        bookingId: travellerBookingId,
                        payload: {
                          relationshipToLeadCustomer: travellerRelationship || undefined,
                          notes: travellerNotes || undefined
                        }
                      })
                    }
                  >
                    Save traveller link
                  </Button>
                  <Button
                    type="button"
                    className="bg-rose-600 hover:bg-rose-500"
                    disabled={!travellerBookingId}
                    isLoading={removeTravellerMutation.isPending}
                    onClick={() => void removeTravellerMutation.mutateAsync({ customerId: customer.id, bookingId: travellerBookingId })}
                  >
                    Remove traveller link
                  </Button>
                </div>
                {selectedTravellerBooking.data ? <p className="mt-3 text-xs text-slate-500">Selected booking has {selectedTravellerBooking.data.travellers.length} traveller links.</p> : null}
              </div>
            </div>

            <div className="space-y-4">
              <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <h3 className="mb-3 text-sm font-semibold text-slate-900">Lead quotes</h3>
                {relatedDataQuery.data?.quotes.length ? (
                  <Table headers={['Quote', 'Currency', 'Total Price', 'Status']}>
                    {relatedDataQuery.data.quotes.map((quote) => (
                      <tr key={quote.id} className="border-t border-slate-200">
                        <td className="px-3 py-2 font-mono text-xs text-slate-700">{quote.id}</td>
                        <td className="px-3 py-2 text-slate-700">{quote.currency}</td>
                        <td className="px-3 py-2 text-slate-700">{formatCurrency(quote.totalPrice, quote.currency)}</td>
                        <td className="px-3 py-2"><Badge tone="info">{quote.status}</Badge></td>
                      </tr>
                    ))}
                  </Table>
                ) : <p className="text-sm text-slate-500">No lead quotes linked.</p>}
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <h3 className="mb-3 text-sm font-semibold text-slate-900">Lead itineraries</h3>
                {relatedDataQuery.data?.itineraries.length ? (
                  <Table headers={['Itinerary', 'Start', 'Duration', 'Items']}>
                    {relatedDataQuery.data.itineraries.map((itinerary) => (
                      <tr key={itinerary.id} className="border-t border-slate-200">
                        <td className="px-3 py-2 font-mono text-xs text-slate-700">{itinerary.id}</td>
                        <td className="px-3 py-2 text-slate-700">{formatDateOnly(itinerary.startDate)}</td>
                        <td className="px-3 py-2 text-slate-700">{itinerary.duration}</td>
                        <td className="px-3 py-2 text-slate-700">{itinerary.items.length}</td>
                      </tr>
                    ))}
                  </Table>
                ) : <p className="text-sm text-slate-500">No lead itineraries linked.</p>}
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <h3 className="mb-3 text-sm font-semibold text-slate-900">Lead bookings and traveller history</h3>
                {relatedDataQuery.data?.bookings.length ? (
                  <Table headers={['Booking', 'Status', 'Lead', 'Items', 'Created']}>
                    {relatedDataQuery.data.bookings.map((booking) => (
                      <tr key={booking.id} className="border-t border-slate-200">
                        <td className="px-3 py-2 font-mono text-xs text-slate-700">{booking.id}</td>
                        <td className="px-3 py-2"><Badge tone="info">{booking.status}</Badge></td>
                        <td className="px-3 py-2 text-slate-700">{booking.leadCustomerName ?? '-'}</td>
                        <td className="px-3 py-2 text-slate-700">{booking.items.length}</td>
                        <td className="px-3 py-2 text-slate-700">{formatDateTime(booking.createdAt)}</td>
                      </tr>
                    ))}
                  </Table>
                ) : <p className="text-sm text-slate-500">No lead bookings linked.</p>}

                <div className="mt-4">
                  <p className="mb-2 text-sm font-semibold text-slate-900">Traveller-only bookings</p>
                  {customer.travellerBookings.length === 0 ? <p className="text-sm text-slate-500">No traveller-only links.</p> : null}
                  {customer.travellerBookings.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {customer.travellerBookings.map((booking) => (
                        <Badge key={`${booking.bookingId}-${booking.createdAt}`} tone="warning">
                          {booking.bookingId.slice(0, 8)} {booking.relationshipToLeadCustomer ?? ''}
                        </Badge>
                      ))}
                    </div>
                  ) : null}
                </div>
              </div>
            </div>
          </div>
        ) : null}
      </Card>
    </div>
  );
};
