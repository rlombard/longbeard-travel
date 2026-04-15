const signupSessionIdKey = 'tourops.signup.sessionId';
const signupSessionTokenKey = 'tourops.signup.accessToken';

export const getStoredSignupSession = () => {
  const sessionId = window.localStorage.getItem(signupSessionIdKey);
  const accessToken = window.localStorage.getItem(signupSessionTokenKey);
  return sessionId && accessToken ? { sessionId, accessToken } : null;
};

export const storeSignupSession = (sessionId: string, accessToken: string) => {
  window.localStorage.setItem(signupSessionIdKey, sessionId);
  window.localStorage.setItem(signupSessionTokenKey, accessToken);
};

export const clearSignupSession = () => {
  window.localStorage.removeItem(signupSessionIdKey);
  window.localStorage.removeItem(signupSessionTokenKey);
};
