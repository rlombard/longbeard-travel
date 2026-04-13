import { FormEvent, useState } from 'react';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { CreateProductRequest, ProductType } from '../../types/product';
import { useCreateProduct, useProducts } from './hooks';

const typeOptions = [
  { label: 'Tour', value: 'Tour' },
  { label: 'Hotel', value: 'Hotel' },
  { label: 'Transport', value: 'Transport' }
];

export const ProductsPage = () => {
  const { data: products = [], isLoading, isError, error } = useProducts();
  const createMutation = useCreateProduct();
  const [form, setForm] = useState<CreateProductRequest>({
    name: '',
    supplierId: '',
    type: 'Tour',
    metadata: '{}'
  });

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    await createMutation.mutateAsync(form);
    setForm((current) => ({ ...current, name: '' }));
  };

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <Card title="Create Product">
        <form className="space-y-3" onSubmit={onSubmit}>
          <FormInput
            label="Name"
            value={form.name}
            required
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
          <SelectDropdown
            label="Type"
            value={form.type}
            options={typeOptions}
            onChange={(e) => setForm({ ...form, type: e.target.value as ProductType })}
          />
          <FormInput
            label="Supplier Id"
            value={form.supplierId}
            required
            onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
          />
          <Button isLoading={createMutation.isPending}>Create Product</Button>
          {createMutation.isError && (
            <p className="text-sm text-red-600">{(createMutation.error as Error).message}</p>
          )}
        </form>
      </Card>

      <Card title="Products">
        {isLoading ? <p>Loading products...</p> : null}
        {isError ? <p className="text-red-600">{(error as Error).message}</p> : null}
        {!isLoading && !isError ? (
          <Table headers={['Name', 'Type', 'SupplierId']}>
            {products.map((product) => (
              <tr key={product.id} className="border-t border-slate-200">
                <td className="px-3 py-2">{product.name}</td>
                <td className="px-3 py-2">{product.type}</td>
                <td className="px-3 py-2 font-mono text-xs">{product.supplierId}</td>
              </tr>
            ))}
          </Table>
        ) : null}
      </Card>
    </div>
  );
};
