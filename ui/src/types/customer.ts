export type PreferredContactMethod = 'Email' | 'Phone' | 'WhatsApp' | 'Any';
export type CustomerVerificationStatus = 'NotStarted' | 'Pending' | 'Verified' | 'Rejected' | 'Expired';
export type CustomerBudgetBand = 'Unknown' | 'Economy' | 'Standard' | 'Premium' | 'Luxury';
export type TravelPace = 'Unknown' | 'Relaxed' | 'Balanced' | 'Fast';
export type TravelValueLeaning = 'Unknown' | 'Value' | 'Balanced' | 'Luxury';

export interface CustomerRequest {
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  nationality?: string | null;
  countryOfResidence?: string | null;
  dateOfBirth?: string | null;
  preferredContactMethod: PreferredContactMethod;
  notes?: string | null;
}

export interface CustomerKycRequest {
  passportNumber?: string | null;
  documentReference?: string | null;
  passportExpiry?: string | null;
  issuingCountry?: string | null;
  visaNotes?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  emergencyContactRelationship?: string | null;
  verificationStatus: CustomerVerificationStatus;
  verificationNotes?: string | null;
  profileDataConsentGranted: boolean;
  kycDataConsentGranted: boolean;
}

export interface CustomerPreferenceRequest {
  budgetBand: CustomerBudgetBand;
  accommodationPreference?: string | null;
  roomPreference?: string | null;
  dietaryRequirements: string[];
  activityPreferences: string[];
  accessibilityRequirements: string[];
  paceOfTravel: TravelPace;
  valueLeaning: TravelValueLeaning;
  transportPreferences: string[];
  specialOccasions: string[];
  dislikedExperiences: string[];
  preferredDestinations: string[];
  avoidedDestinations: string[];
  operatorNotes?: string | null;
}

export interface BookingTravellerRequest {
  relationshipToLeadCustomer?: string | null;
  notes?: string | null;
}

export interface CustomerListItem {
  id: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  nationality?: string | null;
  countryOfResidence?: string | null;
  preferredContactMethod: PreferredContactMethod;
  updatedAt: string;
}

export interface CustomerKyc {
  passportNumber?: string | null;
  documentReference?: string | null;
  passportExpiry?: string | null;
  issuingCountry?: string | null;
  visaNotes?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  emergencyContactRelationship?: string | null;
  verificationStatus: CustomerVerificationStatus;
  verificationNotes?: string | null;
  profileDataConsentGranted: boolean;
  kycDataConsentGranted: boolean;
  updatedAt?: string | null;
}

export interface CustomerPreferences {
  budgetBand: CustomerBudgetBand;
  accommodationPreference?: string | null;
  roomPreference?: string | null;
  dietaryRequirements: string[];
  activityPreferences: string[];
  accessibilityRequirements: string[];
  paceOfTravel: TravelPace;
  valueLeaning: TravelValueLeaning;
  transportPreferences: string[];
  specialOccasions: string[];
  dislikedExperiences: string[];
  preferredDestinations: string[];
  avoidedDestinations: string[];
  operatorNotes?: string | null;
  updatedAt?: string | null;
}

export interface BookingTravellerLink {
  bookingId: string;
  relationshipToLeadCustomer?: string | null;
  notes?: string | null;
  createdAt: string;
}

export interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  nationality?: string | null;
  countryOfResidence?: string | null;
  dateOfBirth?: string | null;
  preferredContactMethod: PreferredContactMethod;
  notes?: string | null;
  createdAt: string;
  updatedAt: string;
  kyc: CustomerKyc;
  preferences: CustomerPreferences;
  leadQuoteIds: string[];
  leadItineraryIds: string[];
  leadBookingIds: string[];
  travellerBookings: BookingTravellerLink[];
}

export interface CustomerSearchQuery {
  searchTerm?: string;
  countryOfResidence?: string;
  nationality?: string;
}

export interface CustomerLinkResponse {
  customerId: string;
  targetId: string;
  targetType: string;
}
