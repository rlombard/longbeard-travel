export type PricingModel = 'PerPerson' | 'PerGroup' | 'PerUnit';

export interface Rate {
  id: string;
  productId: string;
  productRoomId?: string | null;
  productRoomName?: string | null;
  isActive: boolean;
  previousRateId?: string | null;
  supersededAt?: string | null;
  seasonStart: string;
  seasonEnd: string;
  pricingModel: PricingModel;
  baseCost: number;
  currency: string;
  minPax?: number | null;
  maxPax?: number | null;
  childDiscount?: number | null;
  singleSupplement?: number | null;
  capacity?: number | null;
  validityPeriod?: string | null;
  validityPeriodDescription?: string | null;
  rateVariation?: string | null;
  rateTypeName?: string | null;
  rateBasis?: string | null;
  occupancyType?: string | null;
  mealBasis?: string | null;
  minimumStay?: string | null;
  createdAt: string;
}

export interface RateRequest {
  productId: string;
  productRoomId?: string | null;
  seasonStart: string;
  seasonEnd: string;
  pricingModel: PricingModel;
  baseCost: number;
  currency: string;
  minPax?: number | null;
  maxPax?: number | null;
  childDiscount?: number | null;
  singleSupplement?: number | null;
  capacity?: number | null;
  validityPeriod?: string | null;
  validityPeriodDescription?: string | null;
  rateVariation?: string | null;
  rateTypeName?: string | null;
  rateBasis?: string | null;
  occupancyType?: string | null;
  mealBasis?: string | null;
  minimumStay?: string | null;
}
