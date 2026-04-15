import { AdminGuard } from '../auth/AdminGuard';
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { AuthGuard } from '../auth/AuthGuard';
import { PageLayout } from '../components/PageLayout';
import { AdminUsersPage } from '../features/adminUsers/AdminUsersPage';
import { BookingDetailPage } from '../features/bookings/BookingDetailPage';
import { CustomerDetailPage } from '../features/customers/CustomerDetailPage';
import { CustomersPage } from '../features/customers/CustomersPage';
import { EmailsPage } from '../features/emails/EmailsPage';
import { BookingsPage } from '../features/bookings/BookingsPage';
import { InvoiceDetailPage } from '../features/invoices/InvoiceDetailPage';
import { InvoicesPage } from '../features/invoices/InvoicesPage';
import { AiItineraryAssistPage } from '../features/itineraries/AiItineraryAssistPage';
import { ItineraryBuilderPage } from '../features/itineraries/ItineraryBuilderPage';
import { ProductsPage } from '../features/products/ProductsPage';
import { OperationsPage } from '../features/tasks/OperationsPage';
import { QuoteGeneratorPage } from '../features/quotes/QuoteGeneratorPage';
import { SuppliersPage } from '../features/suppliers/SuppliersPage';

const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <AuthGuard>
        <PageLayout />
      </AuthGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/suppliers" replace /> },
      { path: '/suppliers', element: <SuppliersPage /> },
      { path: '/products', element: <ProductsPage /> },
      { path: '/itineraries', element: <ItineraryBuilderPage /> },
      { path: '/itineraries/ai', element: <AiItineraryAssistPage /> },
      { path: '/quotes', element: <QuoteGeneratorPage /> },
      { path: '/bookings', element: <BookingsPage /> },
      { path: '/bookings/:bookingId', element: <BookingDetailPage /> },
      { path: '/invoices', element: <InvoicesPage /> },
      { path: '/invoices/:invoiceId', element: <InvoiceDetailPage /> },
      { path: '/customers', element: <CustomersPage /> },
      { path: '/customers/:customerId', element: <CustomerDetailPage /> },
      {
        path: '/admin/users',
        element: (
          <AdminGuard>
            <AdminUsersPage />
          </AdminGuard>
        )
      },
      { path: '/emails', element: <EmailsPage /> },
      { path: '/operations', element: <OperationsPage /> }
    ]
  }
]);

export const AppRouter = () => <RouterProvider router={router} />;
