import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { BootSplash } from '../../components/BootSplash';
import { useSessionBootstrap } from '../session/hooks';

export const AuthCallbackPage = () => {
  const auth = useAuth();
  const navigate = useNavigate();
  const bootstrapQuery = useSessionBootstrap();

  useEffect(() => {
    if (auth.isReady && !auth.isAuthenticated) {
      navigate('/', { replace: true });
    }
  }, [auth.isAuthenticated, auth.isReady, navigate]);

  useEffect(() => {
    if (auth.isAuthenticated && bootstrapQuery.data?.session?.homeArea) {
      navigate(bootstrapQuery.data.session.homeArea, { replace: true });
    }
  }, [auth.isAuthenticated, bootstrapQuery.data?.session?.homeArea, navigate]);

  return <BootSplash eyebrow="Signing In" title="Completing login" detail="Hang tight while we finish authentication and open the right workspace." />;
};
