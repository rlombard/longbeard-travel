export const formatDateOnly = (value?: string | null) => {
  if (!value) {
    return '-';
  }

  const parsed = new Date(`${value}T00:00:00`);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString();
};

export const formatDateTime = (value?: string | null) => {
  if (!value) {
    return '-';
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleString();
};

export const formatCurrency = (amount: number, currency: string) => {
  try {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency,
      maximumFractionDigits: 2
    }).format(amount);
  } catch {
    return `${amount.toFixed(2)} ${currency}`;
  }
};

export const formatPercent = (value: number) => `${Math.round(value * 100)}%`;

export const humanize = (value?: string | null) => {
  if (!value) {
    return '-';
  }

  return value.replace(/([a-z])([A-Z])/g, '$1 $2');
};

export const maskValue = (value?: string | null, visible = 4) => {
  if (!value) {
    return '-';
  }

  if (value.length <= visible) {
    return '*'.repeat(value.length);
  }

  return `${'*'.repeat(Math.max(0, value.length - visible))}${value.slice(-visible)}`;
};
