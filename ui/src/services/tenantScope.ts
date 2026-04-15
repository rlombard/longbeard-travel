const activeTenantStorageKey = 'tourops.activeTenantId';

export const getActiveTenantId = () => window.localStorage.getItem(activeTenantStorageKey);

export const setActiveTenantId = (tenantId?: string | null) => {
  if (!tenantId) {
    window.localStorage.removeItem(activeTenantStorageKey);
    return;
  }

  window.localStorage.setItem(activeTenantStorageKey, tenantId);
};
