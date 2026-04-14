import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  attachCustomerToBooking,
  attachCustomerToItinerary,
  attachCustomerToQuote,
  createCustomer,
  getCustomer,
  removeBookingTraveller,
  searchCustomers,
  updateCustomer,
  updateCustomerKyc,
  updateCustomerPreferences,
  upsertBookingTraveller
} from '../../services/customersApi';
import {
  BookingTravellerRequest,
  CustomerKycRequest,
  CustomerPreferenceRequest,
  CustomerRequest,
  CustomerSearchQuery
} from '../../types/customer';

export const useCustomers = (query: CustomerSearchQuery) =>
  useQuery({
    queryKey: ['customers', query],
    queryFn: () => searchCustomers(query)
  });

export const useCustomer = (customerId?: string) =>
  useQuery({
    queryKey: ['customer', customerId],
    queryFn: () => getCustomer(customerId!),
    enabled: Boolean(customerId)
  });

export const useCreateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CustomerRequest) => createCustomer(payload),
    onSuccess: (customer) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.setQueryData(['customer', customer.id], customer);
    }
  });
};

export const useUpdateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, payload }: { customerId: string; payload: CustomerRequest }) => updateCustomer(customerId, payload),
    onSuccess: (customer) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.setQueryData(['customer', customer.id], customer);
    }
  });
};

export const useUpdateCustomerKyc = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, payload }: { customerId: string; payload: CustomerKycRequest }) => updateCustomerKyc(customerId, payload),
    onSuccess: (customer) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.setQueryData(['customer', customer.id], customer);
    }
  });
};

export const useUpdateCustomerPreferences = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, payload }: { customerId: string; payload: CustomerPreferenceRequest }) => updateCustomerPreferences(customerId, payload),
    onSuccess: (customer) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.setQueryData(['customer', customer.id], customer);
    }
  });
};

export const useAttachCustomerToQuote = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, quoteId }: { customerId: string; quoteId: string }) => attachCustomerToQuote(customerId, quoteId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] });
    }
  });
};

export const useAttachCustomerToItinerary = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, itineraryId }: { customerId: string; itineraryId: string }) => attachCustomerToItinerary(customerId, itineraryId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] });
    }
  });
};

export const useAttachCustomerToBooking = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, bookingId }: { customerId: string; bookingId: string }) => attachCustomerToBooking(customerId, bookingId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['bookings'] });
      queryClient.invalidateQueries({ queryKey: ['booking', variables.bookingId] });
    }
  });
};

export const useUpsertBookingTraveller = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, bookingId, payload }: { customerId: string; bookingId: string; payload: BookingTravellerRequest }) =>
      upsertBookingTraveller(customerId, bookingId, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['booking', variables.bookingId] });
    }
  });
};

export const useRemoveBookingTraveller = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ customerId, bookingId }: { customerId: string; bookingId: string }) => removeBookingTraveller(customerId, bookingId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['booking', variables.bookingId] });
    }
  });
};
