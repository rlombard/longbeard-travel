export type BookingStatus = 'Draft' | 'Confirmed' | 'Cancelled';
export type BookingItemStatus = 'Pending' | 'Requested' | 'Confirmed' | 'Cancelled';

export interface BookingListItem {
  id: string;
  quoteId: string;
  leadCustomerId?: string | null;
  leadCustomerName?: string | null;
  status: BookingStatus;
  createdAt: string;
  itemCount: number;
}

export interface BookingItem {
  id: string;
  bookingId: string;
  productId: string;
  productName: string;
  supplierId: string;
  supplierName: string;
  status: BookingItemStatus;
  notes?: string | null;
  createdAt: string;
}

export interface Booking {
  id: string;
  quoteId: string;
  leadCustomerId?: string | null;
  leadCustomerName?: string | null;
  status: BookingStatus;
  createdAt: string;
  items: BookingItem[];
  travellers: BookingTraveller[];
}

export interface BookingTraveller {
  customerId: string;
  customerName: string;
  relationshipToLeadCustomer?: string | null;
  notes?: string | null;
  createdAt: string;
}

export interface CreateBookingRequest {
  quoteId: string;
}
