import { apiClient } from './apiClient';
import {
  BookingTravellerRequest,
  Customer,
  CustomerLinkResponse,
  CustomerKycRequest,
  CustomerListItem,
  CustomerPreferenceRequest,
  CustomerRequest,
  CustomerSearchQuery
} from '../types/customer';

const buildParams = (query: CustomerSearchQuery) => {
  const params = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (!value) {
      return;
    }

    params.set(key, value);
  });

  return params;
};

export const createCustomer = async (payload: CustomerRequest): Promise<Customer> => {
  const { data } = await apiClient.post<Customer>('/customers', payload);
  return data;
};

export const getCustomer = async (customerId: string): Promise<Customer> => {
  const { data } = await apiClient.get<Customer>(`/customers/${customerId}`);
  return data;
};

export const searchCustomers = async (query: CustomerSearchQuery = {}): Promise<CustomerListItem[]> => {
  const { data } = await apiClient.get<CustomerListItem[]>('/customers', { params: buildParams(query) });
  return Array.isArray(data) ? data : [];
};

export const updateCustomer = async (customerId: string, payload: CustomerRequest): Promise<Customer> => {
  const { data } = await apiClient.put<Customer>(`/customers/${customerId}`, payload);
  return data;
};

export const updateCustomerKyc = async (customerId: string, payload: CustomerKycRequest): Promise<Customer> => {
  const { data } = await apiClient.put<Customer>(`/customers/${customerId}/kyc`, payload);
  return data;
};

export const updateCustomerPreferences = async (customerId: string, payload: CustomerPreferenceRequest): Promise<Customer> => {
  const { data } = await apiClient.put<Customer>(`/customers/${customerId}/preferences`, payload);
  return data;
};

export const attachCustomerToQuote = async (customerId: string, quoteId: string): Promise<CustomerLinkResponse> => {
  const { data } = await apiClient.post<CustomerLinkResponse>(`/customers/${customerId}/quotes/${quoteId}`);
  return data;
};

export const attachCustomerToItinerary = async (customerId: string, itineraryId: string): Promise<CustomerLinkResponse> => {
  const { data } = await apiClient.post<CustomerLinkResponse>(`/customers/${customerId}/itineraries/${itineraryId}`);
  return data;
};

export const attachCustomerToBooking = async (customerId: string, bookingId: string): Promise<CustomerLinkResponse> => {
  const { data } = await apiClient.post<CustomerLinkResponse>(`/customers/${customerId}/bookings/${bookingId}`);
  return data;
};

export const upsertBookingTraveller = async (customerId: string, bookingId: string, payload: BookingTravellerRequest): Promise<CustomerLinkResponse> => {
  const { data } = await apiClient.put<CustomerLinkResponse>(`/customers/${customerId}/bookings/${bookingId}/traveller`, payload);
  return data;
};

export const removeBookingTraveller = async (customerId: string, bookingId: string): Promise<void> => {
  await apiClient.delete(`/customers/${customerId}/bookings/${bookingId}/traveller`);
};
