export interface ItineraryItem {
  id?: string;
  dayNumber: number;
  productId: string;
  quantity: number;
  notes?: string;
}

export interface Itinerary {
  id: string;
  leadCustomerId?: string | null;
  leadCustomerName?: string | null;
  startDate: string;
  duration: number;
  createdAt: string;
  items: ItineraryItem[];
}

export interface CreateItineraryRequest {
  startDate: string;
  duration: number;
  items: ItineraryItem[];
}
