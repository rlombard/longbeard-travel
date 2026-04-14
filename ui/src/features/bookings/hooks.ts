import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createBookingFromQuote,
  getBooking,
  getBookings,
  updateBookingItemNote,
  updateBookingItemStatus,
  updateBookingStatus
} from '../../services/bookingsApi';
import { BookingItemStatus, BookingStatus, CreateBookingRequest } from '../../types/booking';

export const useBookings = () =>
  useQuery({
    queryKey: ['bookings'],
    queryFn: getBookings
  });

export const useBooking = (bookingId?: string) =>
  useQuery({
    queryKey: ['booking', bookingId],
    queryFn: () => getBooking(bookingId!),
    enabled: Boolean(bookingId)
  });

export const useCreateBooking = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateBookingRequest) => createBookingFromQuote(payload),
    onSuccess: (booking) => {
      queryClient.invalidateQueries({ queryKey: ['bookings'] });
      queryClient.setQueryData(['booking', booking.id], booking);
    }
  });
};

export const useUpdateBookingStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: BookingStatus }) => updateBookingStatus(id, status),
    onSuccess: (booking) => {
      queryClient.invalidateQueries({ queryKey: ['bookings'] });
      queryClient.setQueryData(['booking', booking.id], booking);
    }
  });
};

export const useUpdateBookingItemStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: BookingItemStatus }) => updateBookingItemStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bookings'] });
      queryClient.invalidateQueries({ queryKey: ['booking'] });
    }
  });
};

export const useUpdateBookingItemNote = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, note }: { id: string; note: string }) => updateBookingItemNote(id, note),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['booking'] });
    }
  });
};
