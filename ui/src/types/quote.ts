export interface QuoteLineItem {
  productId: string;
  baseCost: number;
  adjustedCost: number;
  finalPrice: number;
  markupPercentage: number;
  currency: string;
}

export interface Quote {
  id: string;
  itineraryId: string;
  totalCost: number;
  totalPrice: number;
  margin: number;
  currency: string;
  status: 'Draft' | 'Generated';
  createdAt: string;
  lineItems: QuoteLineItem[];
}

export interface GenerateQuoteRequest {
  itineraryId: string;
  pax: number;
  currency: string;
  markup: number;
}
