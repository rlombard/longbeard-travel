import { FormEvent, useEffect, useMemo, useRef, useState } from 'react';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { useCreateSupplier, useSupplier, useSuppliers, useUpdateSupplier } from './hooks';
import { SupplierRequest } from '../../types/supplier';

const emptySupplier = (): SupplierRequest => ({
  name: '',
  email: '',
  phone: ''
});

export const SuppliersPage = () => {
  const searchRef = useRef<HTMLInputElement>(null);
  const [search, setSearch] = useState('');
  const [selectedSupplierId, setSelectedSupplierId] = useState<string>();
  const [draft, setDraft] = useState<SupplierRequest>(emptySupplier);

  const { data: suppliers = [], isLoading } = useSuppliers();
  const { data: supplier } = useSupplier(selectedSupplierId);
  const createMutation = useCreateSupplier();
  const updateMutation = useUpdateSupplier();

  const filteredSuppliers = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return suppliers;
    }

    return suppliers.filter((item) => [item.name, item.email, item.phone].some((value) => value?.toLowerCase().includes(query)));
  }, [search, suppliers]);

  useEffect(() => {
    if (!supplier) {
      return;
    }

    setDraft({
      name: supplier.name,
      email: supplier.email ?? '',
      phone: supplier.phone ?? ''
    });
  }, [supplier]);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === '/' && !event.metaKey && !event.ctrlKey && !event.altKey) {
        event.preventDefault();
        searchRef.current?.focus();
      }

      if (event.altKey && event.key.toLowerCase() === 'n') {
        event.preventDefault();
        setSelectedSupplierId(undefined);
        setDraft(emptySupplier());
      }

      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 's') {
        event.preventDefault();
        void submitDraft();
      }

      if (event.altKey && (event.key === 'ArrowDown' || event.key === 'ArrowUp')) {
        event.preventDefault();
        if (filteredSuppliers.length === 0) {
          return;
        }

        const currentIndex = filteredSuppliers.findIndex((item) => item.id === selectedSupplierId);
        const nextIndex = event.key === 'ArrowDown'
          ? Math.min(filteredSuppliers.length - 1, currentIndex + 1)
          : Math.max(0, currentIndex <= 0 ? 0 : currentIndex - 1);

        setSelectedSupplierId(filteredSuppliers[nextIndex].id);
      }
    };

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [filteredSuppliers, selectedSupplierId, draft]);

  const submitDraft = async () => {
    if (selectedSupplierId) {
      await updateMutation.mutateAsync({ id: selectedSupplierId, payload: draft });
      return;
    }

    const created = await createMutation.mutateAsync(draft);
    setSelectedSupplierId(created.id);
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    await submitDraft();
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
      <Card title="Suppliers">
        <div className="space-y-3">
          <FormInput
            ref={searchRef}
            label="Search / Focus with /"
            placeholder="Name, email or phone"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <div className="flex gap-2 text-xs text-slate-500">
            <span>`Alt+N` new</span>
            <span>`Alt+↑/↓` move</span>
            <span>`Ctrl/Cmd+S` save</span>
          </div>
          <div className="max-h-[65vh] space-y-2 overflow-auto pr-1">
            {isLoading ? <p className="text-sm text-slate-500">Loading suppliers...</p> : null}
            {filteredSuppliers.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setSelectedSupplierId(item.id)}
                className={`w-full rounded-lg border px-3 py-3 text-left transition ${selectedSupplierId === item.id ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 bg-slate-50 hover:border-slate-400 hover:bg-white'}`}
              >
                <div className="font-medium">{item.name}</div>
                <div className={`text-xs ${selectedSupplierId === item.id ? 'text-slate-200' : 'text-slate-500'}`}>{item.email || 'No email'} • {item.phone || 'No phone'}</div>
              </button>
            ))}
          </div>
        </div>
      </Card>

      <Card title={selectedSupplierId ? 'Edit Supplier' : 'New Supplier'}>
        <form className="space-y-4" onSubmit={onSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <FormInput label="Supplier Name" value={draft.name} required onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))} />
            <FormInput label="Email" value={draft.email ?? ''} onChange={(event) => setDraft((current) => ({ ...current, email: event.target.value }))} />
            <FormInput label="Phone" value={draft.phone ?? ''} onChange={(event) => setDraft((current) => ({ ...current, phone: event.target.value }))} />
          </div>

          <div className="flex flex-wrap gap-3">
            <Button type="submit" isLoading={createMutation.isPending || updateMutation.isPending}>{selectedSupplierId ? 'Save Supplier' : 'Create Supplier'}</Button>
            <Button
              type="button"
              className="bg-slate-200 text-slate-800 hover:bg-slate-300"
              onClick={() => {
                setSelectedSupplierId(undefined);
                setDraft(emptySupplier());
              }}
            >
              New Draft
            </Button>
          </div>

          {createMutation.isError ? <p className="text-sm text-red-600">{(createMutation.error as Error).message}</p> : null}
          {updateMutation.isError ? <p className="text-sm text-red-600">{(updateMutation.error as Error).message}</p> : null}
        </form>
      </Card>
    </div>
  );
};
