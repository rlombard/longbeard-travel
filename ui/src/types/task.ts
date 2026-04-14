export type OperationalTaskStatus = 'ToDo' | 'Waiting' | 'FollowUp' | 'Blocked' | 'Done';

export interface OperationalTask {
  id: string;
  title: string;
  description?: string | null;
  status: OperationalTaskStatus;
  assignedToUserId: string;
  createdByUserId: string;
  dueDate?: string | null;
  bookingId?: string | null;
  bookingItemId?: string | null;
  relatedBookingId?: string | null;
  productName?: string | null;
  supplierName?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskRequest {
  bookingId?: string;
  bookingItemId?: string;
  title: string;
  description?: string;
  dueDate?: string;
  assignedToUserId: string;
}

export interface UpdateTaskDetailsRequest {
  title: string;
  description?: string;
  dueDate?: string;
}
