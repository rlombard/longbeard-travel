import { lazy, ReactElement, Suspense } from 'react';
import { AdminGuard } from '../auth/AdminGuard';
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { AuthGuard } from '../auth/AuthGuard';
import { TenantAdminGuard } from '../auth/TenantAdminGuard';
import { BootSplash } from '../components/BootSplash';
import { PageLayout } from '../components/PageLayout';
import { PlatformLayout } from '../components/PlatformLayout';

const LandingPage = lazy(async () => ({ default: (await import('../features/public/LandingPage')).LandingPage }));
const AuthCallbackPage = lazy(async () => ({ default: (await import('../features/public/AuthCallbackPage')).AuthCallbackPage }));
const SignupWizardPage = lazy(async () => ({ default: (await import('../features/signup/SignupWizardPage')).SignupWizardPage }));
const SuppliersPage = lazy(async () => ({ default: (await import('../features/suppliers/SuppliersPage')).SuppliersPage }));
const ProductsPage = lazy(async () => ({ default: (await import('../features/products/ProductsPage')).ProductsPage }));
const ItineraryBuilderPage = lazy(async () => ({ default: (await import('../features/itineraries/ItineraryBuilderPage')).ItineraryBuilderPage }));
const AiItineraryAssistPage = lazy(async () => ({ default: (await import('../features/itineraries/AiItineraryAssistPage')).AiItineraryAssistPage }));
const QuoteGeneratorPage = lazy(async () => ({ default: (await import('../features/quotes/QuoteGeneratorPage')).QuoteGeneratorPage }));
const BookingsPage = lazy(async () => ({ default: (await import('../features/bookings/BookingsPage')).BookingsPage }));
const BookingDetailPage = lazy(async () => ({ default: (await import('../features/bookings/BookingDetailPage')).BookingDetailPage }));
const InvoicesPage = lazy(async () => ({ default: (await import('../features/invoices/InvoicesPage')).InvoicesPage }));
const InvoiceDetailPage = lazy(async () => ({ default: (await import('../features/invoices/InvoiceDetailPage')).InvoiceDetailPage }));
const CustomersPage = lazy(async () => ({ default: (await import('../features/customers/CustomersPage')).CustomersPage }));
const CustomerDetailPage = lazy(async () => ({ default: (await import('../features/customers/CustomerDetailPage')).CustomerDetailPage }));
const EmailsPage = lazy(async () => ({ default: (await import('../features/emails/EmailsPage')).EmailsPage }));
const OperationsPage = lazy(async () => ({ default: (await import('../features/tasks/OperationsPage')).OperationsPage }));
const TenantSettingsPage = lazy(async () => ({ default: (await import('../features/tenantAdmin/TenantSettingsPage')).TenantSettingsPage }));
const AdminPlatformPage = lazy(async () => ({ default: (await import('../features/platform/AdminPlatformPage')).AdminPlatformPage }));
const AdminUsersPage = lazy(async () => ({ default: (await import('../features/adminUsers/AdminUsersPage')).AdminUsersPage }));

const router = createBrowserRouter([
  {
    path: '/',
    element: withRouteLoader(<LandingPage />, 'Opening Entry', 'Loading public entry experience.')
  },
  {
    path: '/auth/callback',
    element: withRouteLoader(<AuthCallbackPage />, 'Signing In', 'Completing redirect and opening your workspace.')
  },
  {
    path: '/signup',
    element: withRouteLoader(<SignupWizardPage />, 'Opening Signup', 'Loading SaaS onboarding wizard and signup state.')
  },
  {
    path: '/app',
    element: (
      <AuthGuard>
        <PageLayout />
      </AuthGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/app/suppliers" replace /> },
      { path: 'suppliers', element: withRouteLoader(<SuppliersPage />, 'Opening Workspace', 'Loading suppliers workspace.') },
      { path: 'products', element: withRouteLoader(<ProductsPage />, 'Opening Workspace', 'Loading products workspace.') },
      { path: 'itineraries', element: withRouteLoader(<ItineraryBuilderPage />, 'Opening Workspace', 'Loading itinerary builder.') },
      { path: 'itineraries/ai', element: withRouteLoader(<AiItineraryAssistPage />, 'Opening Workspace', 'Loading AI itinerary assist.') },
      { path: 'quotes', element: withRouteLoader(<QuoteGeneratorPage />, 'Opening Workspace', 'Loading quote generator.') },
      { path: 'bookings', element: withRouteLoader(<BookingsPage />, 'Opening Workspace', 'Loading bookings workspace.') },
      { path: 'bookings/:bookingId', element: withRouteLoader(<BookingDetailPage />, 'Opening Workspace', 'Loading booking detail.') },
      { path: 'invoices', element: withRouteLoader(<InvoicesPage />, 'Opening Workspace', 'Loading invoices workspace.') },
      { path: 'invoices/:invoiceId', element: withRouteLoader(<InvoiceDetailPage />, 'Opening Workspace', 'Loading invoice detail.') },
      { path: 'customers', element: withRouteLoader(<CustomersPage />, 'Opening Workspace', 'Loading customers workspace.') },
      { path: 'customers/:customerId', element: withRouteLoader(<CustomerDetailPage />, 'Opening Workspace', 'Loading customer detail.') },
      { path: 'emails', element: withRouteLoader(<EmailsPage />, 'Opening Workspace', 'Loading email workspace.') },
      { path: 'operations', element: withRouteLoader(<OperationsPage />, 'Opening Workspace', 'Loading operations board.') },
      {
        path: 'settings',
        element: (
          <TenantAdminGuard>
            {withRouteLoader(<TenantSettingsPage />, 'Opening Settings', 'Loading tenant email system, users, and config.')}
          </TenantAdminGuard>
        )
      }
    ]
  },
  {
    path: '/platform',
    element: (
      <AdminGuard>
        <PlatformLayout />
      </AdminGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/platform/tenants" replace /> },
      { path: 'tenants', element: withRouteLoader(<AdminPlatformPage />, 'Opening Platform', 'Loading tenant portfolio overview.') },
      { path: 'users', element: withRouteLoader(<AdminUsersPage />, 'Opening Platform', 'Loading platform user administration.') }
    ]
  },
  {
    path: '/suppliers',
    element: <Navigate to="/app/suppliers" replace />
  },
  {
    path: '/products',
    element: <Navigate to="/app/products" replace />
  },
  {
    path: '/itineraries',
    element: <Navigate to="/app/itineraries" replace />
  },
  {
    path: '/itineraries/ai',
    element: <Navigate to="/app/itineraries/ai" replace />
  },
  {
    path: '/quotes',
    element: <Navigate to="/app/quotes" replace />
  },
  {
    path: '/bookings',
    element: <Navigate to="/app/bookings" replace />
  },
  {
    path: '/bookings/:bookingId',
    element: <Navigate to="/app/bookings" replace />
  },
  {
    path: '/invoices',
    element: <Navigate to="/app/invoices" replace />
  },
  {
    path: '/customers',
    element: <Navigate to="/app/customers" replace />
  },
  {
    path: '/emails',
    element: <Navigate to="/app/emails" replace />
  },
  {
    path: '/operations',
    element: <Navigate to="/app/operations" replace />
  },
  {
    path: '/settings',
    element: <Navigate to="/app/settings" replace />
  },
  {
    path: '/admin/platform',
    element: <Navigate to="/platform/tenants" replace />
  },
  {
    path: '/admin/users',
    element: <Navigate to="/platform/users" replace />
  }
]);

export const AppRouter = () => <RouterProvider router={router} />;

function withRouteLoader(element: ReactElement, title: string, detail: string) {
  return (
    <Suspense fallback={<BootSplash eyebrow={title} title={title} detail={detail} />}>
      {element}
    </Suspense>
  );
}
