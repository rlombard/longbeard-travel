import { Badge } from '../../components/Badge';
import {
  CustomerBudgetBand,
  CustomerVerificationStatus,
  PreferredContactMethod,
  TravelPace,
  TravelValueLeaning
} from '../../types/customer';
import { humanize } from '../../utils/formatters';

const verificationTone: Record<CustomerVerificationStatus, 'neutral' | 'info' | 'success' | 'warning' | 'danger'> = {
  NotStarted: 'neutral',
  Pending: 'warning',
  Verified: 'success',
  Rejected: 'danger',
  Expired: 'warning'
};

export const CustomerVerificationBadge = ({ status }: { status: CustomerVerificationStatus }) => (
  <Badge tone={verificationTone[status]}>{humanize(status)}</Badge>
);

export const customerVerificationOptions: { label: string; value: CustomerVerificationStatus | '' }[] = [
  { label: 'All', value: '' },
  { label: 'Not Started', value: 'NotStarted' },
  { label: 'Pending', value: 'Pending' },
  { label: 'Verified', value: 'Verified' },
  { label: 'Rejected', value: 'Rejected' },
  { label: 'Expired', value: 'Expired' }
];

export const preferredContactOptions: { label: string; value: PreferredContactMethod }[] = [
  { label: 'Email', value: 'Email' },
  { label: 'Phone', value: 'Phone' },
  { label: 'WhatsApp', value: 'WhatsApp' },
  { label: 'Any', value: 'Any' }
];

export const budgetBandOptions: { label: string; value: CustomerBudgetBand }[] = [
  { label: 'Unknown', value: 'Unknown' },
  { label: 'Economy', value: 'Economy' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
  { label: 'Luxury', value: 'Luxury' }
];

export const travelPaceOptions: { label: string; value: TravelPace }[] = [
  { label: 'Unknown', value: 'Unknown' },
  { label: 'Relaxed', value: 'Relaxed' },
  { label: 'Balanced', value: 'Balanced' },
  { label: 'Fast', value: 'Fast' }
];

export const valueLeaningOptions: { label: string; value: TravelValueLeaning }[] = [
  { label: 'Unknown', value: 'Unknown' },
  { label: 'Value', value: 'Value' },
  { label: 'Balanced', value: 'Balanced' },
  { label: 'Luxury', value: 'Luxury' }
];
