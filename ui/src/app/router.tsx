import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { AuthGuard } from '../auth/AuthGuard';
import { PageLayout } from '../components/PageLayout';
import { ItineraryBuilderPage } from '../features/itineraries/ItineraryBuilderPage';
import { ProductsPage } from '../features/products/ProductsPage';
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
      { path: '/quotes', element: <QuoteGeneratorPage /> }
    ]
  }
]);

export const AppRouter = () => <RouterProvider router={router} />;
