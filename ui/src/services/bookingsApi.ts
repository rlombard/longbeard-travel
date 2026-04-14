import { apiClient } from './apiClient';
import { Booking, BookingItem, BookingItemStatus, BookingListItem, BookingStatus, CreateBookingRequest } from '../types/booking';

export const getBookings = async (): Promise<BookingListItem[]> => {
  const { data } = await apiClient.get<BookingListItem[]>('/bookings');
  return Array.isArray(data) ? data : [];
};

export const getBooking = async (id: string): Promise<Booking> => {
  const { data } = await apiClient.get<Booking>(`/bookings/${id}`);
  return data;
};

export const createBookingFromQuote = async (payload: CreateBookingRequest): Promise<Booking> => {
  const { data } = await apiClient.post<Booking>('/bookings/from-quote', payload);
  return data;
};

export const updateBookingStatus = async (id: string, status: BookingStatus): Promise<Booking> => {
  const { data } = await apiClient.patch<Booking>(`/bookings/${id}/status`, { status });
  return data;
};

export const updateBookingItemStatus = async (id: string, status: BookingItemStatus): Promise<BookingItem> => {
  const { data } = await apiClient.patch<BookingItem>(`/booking-items/${id}/status`, { status });
  return data;
};

export const updateBookingItemNote = async (id: string, note: string): Promise<BookingItem> => {
  const { data } = await apiClient.patch<BookingItem>(`/booking-items/${id}/note`, { note });
  return data;
};
