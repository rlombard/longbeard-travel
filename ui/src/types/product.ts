export type ProductType = 'Tour' | 'Hotel' | 'Transport';

export interface Address {
  streetAddress?: string | null;
  suburb?: string | null;
  townOrCity?: string | null;
  stateOrProvince?: string | null;
  country?: string | null;
  postCode?: string | null;
}

export interface TourismLevy {
  amount?: string | null;
  currency?: string | null;
  unit?: string | null;
  ageApplicability?: string | null;
  effectiveDates?: string | null;
  conditions?: string | null;
  rawText?: string | null;
  included: boolean;
}

export interface ProductContact {
  id?: string;
  contactType: string;
  contactName: string;
  contactEmail: string;
  contactPhoneNumber: string;
}

export interface ProductExtra {
  id?: string;
  description: string;
  chargeUnit: string;
  charge: string;
}

export interface ProductRoom {
  id?: string;
  name: string;
  minimumOccupancy?: string | null;
  maximumOccupancy?: string | null;
  additionalNotes?: string | null;
  rateConditions?: string | null;
}

export interface ProductLookupValue {
  id?: string;
  value: string;
}

export interface ProductListItem {
  id: string;
  supplierId: string;
  name: string;
  type: ProductType;
  roomCount: number;
  contractValidityPeriod?: string | null;
  checkInTime?: string | null;
  checkOutTime?: string | null;
}

export interface Product {
  id: string;
  supplierId: string;
  name: string;
  type: ProductType;
  contractValidityPeriod?: string | null;
  commission?: string | null;
  physicalAddress: Address;
  mailingAddress: Address;
  checkInTime?: string | null;
  checkOutTime?: string | null;
  blockOutDates?: string | null;
  tourismLevy: TourismLevy;
  roomPolicies?: string | null;
  ratePolicies?: string | null;
  childPolicies?: string | null;
  cancellationPolicies?: string | null;
  inclusions?: string | null;
  exclusions?: string | null;
  specials?: string | null;
  contacts: ProductContact[];
  extras: ProductExtra[];
  rooms: ProductRoom[];
  rateTypes: ProductLookupValue[];
  rateBases: ProductLookupValue[];
  mealBases: ProductLookupValue[];
  validityPeriods: ProductLookupValue[];
  createdAt: string;
}

export interface ProductRequest {
  supplierId: string;
  name: string;
  type: ProductType;
  contractValidityPeriod?: string | null;
  commission?: string | null;
  physicalAddress: Address;
  mailingAddress: Address;
  checkInTime?: string | null;
  checkOutTime?: string | null;
  blockOutDates?: string | null;
  tourismLevy: TourismLevy;
  roomPolicies?: string | null;
  ratePolicies?: string | null;
  childPolicies?: string | null;
  cancellationPolicies?: string | null;
  inclusions?: string | null;
  exclusions?: string | null;
  specials?: string | null;
  contacts: ProductContact[];
  extras: ProductExtra[];
  rooms: ProductRoom[];
  rateTypes: ProductLookupValue[];
  rateBases: ProductLookupValue[];
  mealBases: ProductLookupValue[];
  validityPeriods: ProductLookupValue[];
}
