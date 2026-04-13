import { FormEvent, useEffect, useMemo, useRef, useState } from 'react';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { SelectDropdown } from '../../components/SelectDropdown';
import { TextAreaField } from '../../components/TextAreaField';
import { useSuppliers } from '../suppliers/hooks';
import { useCreateProduct, useCreateRate, useProduct, useProductRates, useProducts, useUpdateProduct, useUpdateRate } from './hooks';
import { Address, Product, ProductContact, ProductExtra, ProductLookupValue, ProductRequest, ProductRoom, ProductType, TourismLevy } from '../../types/product';
import { PricingModel, Rate, RateRequest } from '../../types/rate';

const typeOptions = [
  { label: 'Tour', value: 'Tour' },
  { label: 'Hotel', value: 'Hotel' },
  { label: 'Transport', value: 'Transport' }
];

const pricingOptions = [
  { label: 'Per Person', value: 'PerPerson' },
  { label: 'Per Group', value: 'PerGroup' },
  { label: 'Per Unit', value: 'PerUnit' }
];

const emptyAddress = (): Address => ({
  streetAddress: '',
  suburb: '',
  townOrCity: '',
  stateOrProvince: '',
  country: '',
  postCode: ''
});

const emptyLevy = (): TourismLevy => ({
  amount: '',
  currency: '',
  unit: '',
  ageApplicability: '',
  effectiveDates: '',
  conditions: '',
  rawText: '',
  included: false
});

const emptyLookupValue = (): ProductLookupValue => ({
  value: ''
});

const emptyProduct = (): ProductRequest => ({
  supplierId: '',
  name: '',
  type: 'Hotel',
  contractValidityPeriod: '',
  commission: '',
  physicalAddress: emptyAddress(),
  mailingAddress: emptyAddress(),
  checkInTime: '',
  checkOutTime: '',
  blockOutDates: '',
  tourismLevy: emptyLevy(),
  roomPolicies: '',
  ratePolicies: '',
  childPolicies: '',
  cancellationPolicies: '',
  inclusions: '',
  exclusions: '',
  specials: '',
  contacts: [],
  extras: [],
  rooms: [],
  rateTypes: [],
  rateBases: [],
  mealBases: [],
  validityPeriods: []
});

const emptyContact = (): ProductContact => ({
  contactType: 'Reservation',
  contactName: '',
  contactEmail: '',
  contactPhoneNumber: ''
});

const emptyExtra = (): ProductExtra => ({
  description: '',
  chargeUnit: '',
  charge: ''
});

const emptyRoom = (): ProductRoom => ({
  name: '',
  minimumOccupancy: '',
  maximumOccupancy: '',
  additionalNotes: '',
  rateConditions: ''
});

const emptyRate = (productId = '', roomId = ''): RateRequest => ({
  productId,
  productRoomId: roomId || null,
  seasonStart: '',
  seasonEnd: '',
  pricingModel: 'PerPerson',
  baseCost: 0,
  currency: 'USD',
  minPax: null,
  maxPax: null,
  childDiscount: null,
  singleSupplement: null,
  capacity: null,
  validityPeriod: '',
  validityPeriodDescription: '',
  rateVariation: '',
  rateTypeName: '',
  rateBasis: '',
  occupancyType: '',
  mealBasis: '',
  minimumStay: ''
});

const toProductRequest = (product: Product): ProductRequest => ({
  supplierId: product.supplierId,
  name: product.name,
  type: product.type,
  contractValidityPeriod: product.contractValidityPeriod ?? '',
  commission: product.commission ?? '',
  physicalAddress: product.physicalAddress,
  mailingAddress: product.mailingAddress,
  checkInTime: product.checkInTime ?? '',
  checkOutTime: product.checkOutTime ?? '',
  blockOutDates: product.blockOutDates ?? '',
  tourismLevy: product.tourismLevy,
  roomPolicies: product.roomPolicies ?? '',
  ratePolicies: product.ratePolicies ?? '',
  childPolicies: product.childPolicies ?? '',
  cancellationPolicies: product.cancellationPolicies ?? '',
  inclusions: product.inclusions ?? '',
  exclusions: product.exclusions ?? '',
  specials: product.specials ?? '',
  contacts: product.contacts,
  extras: product.extras,
  rooms: product.rooms,
  rateTypes: product.rateTypes,
  rateBases: product.rateBases,
  mealBases: product.mealBases,
  validityPeriods: product.validityPeriods
});

const toRateRequest = (rate: Rate): RateRequest => ({
  productId: rate.productId,
  productRoomId: rate.productRoomId ?? null,
  seasonStart: rate.seasonStart,
  seasonEnd: rate.seasonEnd,
  pricingModel: rate.pricingModel,
  baseCost: rate.baseCost,
  currency: rate.currency,
  minPax: rate.minPax ?? null,
  maxPax: rate.maxPax ?? null,
  childDiscount: rate.childDiscount ?? null,
  singleSupplement: rate.singleSupplement ?? null,
  capacity: rate.capacity ?? null,
  validityPeriod: rate.validityPeriod ?? '',
  validityPeriodDescription: rate.validityPeriodDescription ?? '',
  rateVariation: rate.rateVariation ?? '',
  rateTypeName: rate.rateTypeName ?? '',
  rateBasis: rate.rateBasis ?? '',
  occupancyType: rate.occupancyType ?? '',
  mealBasis: rate.mealBasis ?? '',
  minimumStay: rate.minimumStay ?? ''
});

const SectionTitle = ({ title, hint }: { title: string; hint?: string }) => (
  <div className="mb-3 flex items-end justify-between gap-3 border-b border-slate-200 pb-2">
    <h3 className="text-sm font-semibold uppercase tracking-[0.16em] text-slate-800">{title}</h3>
    {hint ? <span className="text-xs text-slate-500">{hint}</span> : null}
  </div>
);

const LookupListEditor = ({
  title,
  hint,
  label,
  items,
  onChange,
  onAdd,
  onRemove
}: {
  title: string;
  hint?: string;
  label: string;
  items: ProductLookupValue[];
  onChange: (index: number, value: string) => void;
  onAdd: () => void;
  onRemove: (index: number) => void;
}) => (
  <div>
    <SectionTitle title={title} hint={hint} />
    <div className="space-y-3">
      {items.map((item, index) => (
        <div key={item.id ?? `${title}-${index}`} className="grid gap-3 rounded-lg border border-slate-200 bg-slate-50 p-3 md:grid-cols-[1fr_auto]">
          <FormInput label={label} value={item.value} onChange={(event) => onChange(index, event.target.value)} />
          <div className="flex items-end">
            <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => onRemove(index)}>Remove</Button>
          </div>
        </div>
      ))}
      <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={onAdd}>Add {title.slice(0, -1)}</Button>
    </div>
  </div>
);

export const ProductsPage = () => {
  const searchRef = useRef<HTMLInputElement>(null);
  const [search, setSearch] = useState('');
  const [selectedProductId, setSelectedProductId] = useState<string>();
  const [selectedRateId, setSelectedRateId] = useState<string>();
  const [draft, setDraft] = useState<ProductRequest>(emptyProduct);
  const [rateDraft, setRateDraft] = useState<RateRequest>(emptyRate());

  const { data: products = [], isLoading: isProductsLoading } = useProducts();
  const { data: product } = useProduct(selectedProductId);
  const { data: rates = [] } = useProductRates(selectedProductId);
  const { data: suppliers = [] } = useSuppliers();
  const createProductMutation = useCreateProduct();
  const updateProductMutation = useUpdateProduct();
  const createRateMutation = useCreateRate();
  const updateRateMutation = useUpdateRate();

  const supplierMap = useMemo(() => Object.fromEntries(suppliers.map((supplier) => [supplier.id, supplier.name])), [suppliers]);
  const filteredProducts = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return products;
    }

    return products.filter((item) => [item.name, supplierMap[item.supplierId], item.contractValidityPeriod].some((value) => value?.toLowerCase().includes(query)));
  }, [products, search, supplierMap]);

  useEffect(() => {
    if (!product) {
      return;
    }

    setDraft(toProductRequest(product));
    setRateDraft(emptyRate(product.id, product.rooms[0]?.id ?? ''));
  }, [product]);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      const activeElement = document.activeElement as HTMLElement | null;
      const isTyping = activeElement ? ['INPUT', 'TEXTAREA', 'SELECT'].includes(activeElement.tagName) : false;

      if (event.key === '/' && !event.metaKey && !event.ctrlKey && !event.altKey) {
        event.preventDefault();
        searchRef.current?.focus();
      }

      if (event.altKey && event.key.toLowerCase() === 'n') {
        event.preventDefault();
        setSelectedProductId(undefined);
        setSelectedRateId(undefined);
        setDraft(emptyProduct());
        setRateDraft(emptyRate());
      }

      if (event.altKey && event.key.toLowerCase() === 'r') {
        event.preventDefault();
        setSelectedRateId(undefined);
        setRateDraft(emptyRate(selectedProductId ?? '', draft.rooms[0]?.id ?? ''));
      }

      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 's') {
        event.preventDefault();
        void submitProduct();
      }

      if (!isTyping && event.altKey && (event.key === 'ArrowDown' || event.key === 'ArrowUp')) {
        event.preventDefault();
        if (filteredProducts.length === 0) {
          return;
        }

        const currentIndex = filteredProducts.findIndex((item) => item.id === selectedProductId);
        const nextIndex = event.key === 'ArrowDown'
          ? Math.min(filteredProducts.length - 1, currentIndex + 1)
          : Math.max(0, currentIndex <= 0 ? 0 : currentIndex - 1);

        setSelectedProductId(filteredProducts[nextIndex].id);
      }
    };

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [draft.rooms, filteredProducts, selectedProductId]);

  const submitProduct = async () => {
    if (selectedProductId) {
      await updateProductMutation.mutateAsync({ id: selectedProductId, payload: draft });
      return;
    }

    const created = await createProductMutation.mutateAsync(draft);
    setSelectedProductId(created.id);
  };

  const submitRate = async (event?: FormEvent) => {
    event?.preventDefault();
    if (!selectedProductId) {
      return;
    }

    const payload = { ...rateDraft, productId: selectedProductId };

    if (selectedRateId) {
      const createdRevision = await updateRateMutation.mutateAsync({ id: selectedRateId, payload });
      setSelectedRateId(createdRevision.id);
      setRateDraft(toRateRequest(createdRevision));
      return;
    }

    const created = await createRateMutation.mutateAsync(payload);
    setSelectedRateId(created.id);
    setRateDraft(toRateRequest(created));
  };

  const updateAddress = (key: 'physicalAddress' | 'mailingAddress', field: keyof Address, value: string) => {
    setDraft((current) => ({
      ...current,
      [key]: {
        ...current[key],
        [field]: value
      }
    }));
  };

  const updateLevy = (field: keyof TourismLevy, value: string | boolean) => {
    setDraft((current) => ({
      ...current,
      tourismLevy: {
        ...current.tourismLevy,
        [field]: value
      }
    }));
  };

  const updateLookupList = (key: 'rateTypes' | 'rateBases' | 'mealBases' | 'validityPeriods', index: number, value: string) => {
    setDraft((current) => ({
      ...current,
      [key]: current[key].map((item, itemIndex) => itemIndex === index ? { ...item, value } : item)
    }));
  };

  const addLookupValue = (key: 'rateTypes' | 'rateBases' | 'mealBases' | 'validityPeriods') => {
    setDraft((current) => ({
      ...current,
      [key]: [...current[key], emptyLookupValue()]
    }));
  };

  const removeLookupValue = (key: 'rateTypes' | 'rateBases' | 'mealBases' | 'validityPeriods', index: number) => {
    setDraft((current) => ({
      ...current,
      [key]: current[key].filter((_, itemIndex) => itemIndex !== index)
    }));
  };

  const roomOptions = [{ label: 'No room link', value: '' }, ...draft.rooms.map((room) => ({ label: room.name || 'Unnamed room', value: room.id ?? '' }))];

  return (
    <div className="grid gap-6 xl:grid-cols-[340px_minmax(0,1fr)]">
      <Card title="Products">
        <div className="space-y-3">
          <FormInput
            ref={searchRef}
            label="Search / Focus with /"
            placeholder="Property, supplier or validity"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <div className="flex flex-wrap gap-2 text-xs text-slate-500">
            <span>`Alt+N` new product</span>
            <span>`Alt+R` new rate</span>
            <span>`Alt+↑/↓` move</span>
            <span>`Ctrl/Cmd+S` save product</span>
          </div>
          <div className="max-h-[72vh] space-y-2 overflow-auto pr-1">
            {isProductsLoading ? <p className="text-sm text-slate-500">Loading products...</p> : null}
            {filteredProducts.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => setSelectedProductId(item.id)}
                className={`w-full rounded-lg border px-3 py-3 text-left transition ${selectedProductId === item.id ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 bg-slate-50 hover:border-slate-400 hover:bg-white'}`}
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="font-medium">{item.name}</div>
                    <div className={`text-xs ${selectedProductId === item.id ? 'text-slate-200' : 'text-slate-500'}`}>{supplierMap[item.supplierId] || 'Unknown supplier'}</div>
                  </div>
                  <div className={`rounded-full px-2 py-1 text-[11px] font-medium ${selectedProductId === item.id ? 'bg-white/15 text-white' : 'bg-amber-100 text-amber-900'}`}>{item.roomCount} rooms</div>
                </div>
                <div className={`mt-2 text-xs ${selectedProductId === item.id ? 'text-slate-200' : 'text-slate-500'}`}>{item.contractValidityPeriod || 'No validity period'} • {item.checkInTime || '--'} / {item.checkOutTime || '--'}</div>
              </button>
            ))}
          </div>
        </div>
      </Card>

      <div className="space-y-6">
        <Card title={selectedProductId ? 'Edit Property Product' : 'New Property Product'}>
          <form
            className="space-y-6"
            onSubmit={(event) => {
              event.preventDefault();
              void submitProduct();
            }}
          >
            <div className="grid gap-6 lg:grid-cols-2">
              <div className="space-y-4">
                <SectionTitle title="Core" hint="Fast tab order for keyboard entry" />
                <FormInput label="Property Name" value={draft.name} required onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))} />
                <SelectDropdown
                  label="Supplier"
                  value={draft.supplierId}
                  options={[{ label: 'Select supplier', value: '' }, ...suppliers.map((supplier) => ({ label: supplier.name, value: supplier.id }))]}
                  onChange={(event) => setDraft((current) => ({ ...current, supplierId: event.target.value }))}
                />
                <SelectDropdown label="Type" value={draft.type} options={typeOptions} onChange={(event) => setDraft((current) => ({ ...current, type: event.target.value as ProductType }))} />
                <FormInput label="Contract Validity Period" value={draft.contractValidityPeriod ?? ''} onChange={(event) => setDraft((current) => ({ ...current, contractValidityPeriod: event.target.value }))} />
                <FormInput label="Commission" value={draft.commission ?? ''} onChange={(event) => setDraft((current) => ({ ...current, commission: event.target.value }))} />
                <div className="grid gap-4 md:grid-cols-2">
                  <FormInput label="Check-In" value={draft.checkInTime ?? ''} onChange={(event) => setDraft((current) => ({ ...current, checkInTime: event.target.value }))} />
                  <FormInput label="Check-Out" value={draft.checkOutTime ?? ''} onChange={(event) => setDraft((current) => ({ ...current, checkOutTime: event.target.value }))} />
                </div>
                <FormInput label="Block-Out Dates" value={draft.blockOutDates ?? ''} onChange={(event) => setDraft((current) => ({ ...current, blockOutDates: event.target.value }))} />
              </div>

              <div className="space-y-4">
                <SectionTitle title="Tourism Levy" hint="Keep normalized values here" />
                <div className="grid gap-4 md:grid-cols-2">
                  <FormInput label="Amount" value={draft.tourismLevy.amount ?? ''} onChange={(event) => updateLevy('amount', event.target.value)} />
                  <FormInput label="Currency" value={draft.tourismLevy.currency ?? ''} onChange={(event) => updateLevy('currency', event.target.value)} />
                  <FormInput label="Unit" value={draft.tourismLevy.unit ?? ''} onChange={(event) => updateLevy('unit', event.target.value)} />
                  <FormInput label="Age Applicability" value={draft.tourismLevy.ageApplicability ?? ''} onChange={(event) => updateLevy('ageApplicability', event.target.value)} />
                </div>
                <FormInput label="Effective Dates" value={draft.tourismLevy.effectiveDates ?? ''} onChange={(event) => updateLevy('effectiveDates', event.target.value)} />
                <TextAreaField label="Conditions" value={draft.tourismLevy.conditions ?? ''} onChange={(event) => updateLevy('conditions', event.target.value)} />
                <TextAreaField label="Raw Text" value={draft.tourismLevy.rawText ?? ''} onChange={(event) => updateLevy('rawText', event.target.value)} />
                <label className="flex items-center gap-2 text-sm font-medium text-slate-700">
                  <input type="checkbox" checked={draft.tourismLevy.included} onChange={(event) => updateLevy('included', event.target.checked)} />
                  Tourism levy included in rates
                </label>
              </div>
            </div>

            <div className="grid gap-6 lg:grid-cols-2">
              <div>
                <SectionTitle title="Physical Address" />
                <div className="grid gap-3 md:grid-cols-2">
                  <FormInput label="Street" value={draft.physicalAddress.streetAddress ?? ''} onChange={(event) => updateAddress('physicalAddress', 'streetAddress', event.target.value)} />
                  <FormInput label="Suburb" value={draft.physicalAddress.suburb ?? ''} onChange={(event) => updateAddress('physicalAddress', 'suburb', event.target.value)} />
                  <FormInput label="Town / City" value={draft.physicalAddress.townOrCity ?? ''} onChange={(event) => updateAddress('physicalAddress', 'townOrCity', event.target.value)} />
                  <FormInput label="State / Province" value={draft.physicalAddress.stateOrProvince ?? ''} onChange={(event) => updateAddress('physicalAddress', 'stateOrProvince', event.target.value)} />
                  <FormInput label="Country" value={draft.physicalAddress.country ?? ''} onChange={(event) => updateAddress('physicalAddress', 'country', event.target.value)} />
                  <FormInput label="Post Code" value={draft.physicalAddress.postCode ?? ''} onChange={(event) => updateAddress('physicalAddress', 'postCode', event.target.value)} />
                </div>
              </div>

              <div>
                <SectionTitle title="Mailing Address" />
                <div className="grid gap-3 md:grid-cols-2">
                  <FormInput label="Street" value={draft.mailingAddress.streetAddress ?? ''} onChange={(event) => updateAddress('mailingAddress', 'streetAddress', event.target.value)} />
                  <FormInput label="Suburb" value={draft.mailingAddress.suburb ?? ''} onChange={(event) => updateAddress('mailingAddress', 'suburb', event.target.value)} />
                  <FormInput label="Town / City" value={draft.mailingAddress.townOrCity ?? ''} onChange={(event) => updateAddress('mailingAddress', 'townOrCity', event.target.value)} />
                  <FormInput label="State / Province" value={draft.mailingAddress.stateOrProvince ?? ''} onChange={(event) => updateAddress('mailingAddress', 'stateOrProvince', event.target.value)} />
                  <FormInput label="Country" value={draft.mailingAddress.country ?? ''} onChange={(event) => updateAddress('mailingAddress', 'country', event.target.value)} />
                  <FormInput label="Post Code" value={draft.mailingAddress.postCode ?? ''} onChange={(event) => updateAddress('mailingAddress', 'postCode', event.target.value)} />
                </div>
              </div>
            </div>

            <div className="grid gap-6 lg:grid-cols-2">
              <div className="space-y-3">
                <SectionTitle title="Policies" />
                <TextAreaField label="Room Policies" value={draft.roomPolicies ?? ''} onChange={(event) => setDraft((current) => ({ ...current, roomPolicies: event.target.value }))} />
                <TextAreaField label="Rate Policies" value={draft.ratePolicies ?? ''} onChange={(event) => setDraft((current) => ({ ...current, ratePolicies: event.target.value }))} />
                <TextAreaField label="Child Policies" value={draft.childPolicies ?? ''} onChange={(event) => setDraft((current) => ({ ...current, childPolicies: event.target.value }))} />
              </div>
              <div className="space-y-3">
                <SectionTitle title="Commercial Terms" />
                <TextAreaField label="Cancellation Policies" value={draft.cancellationPolicies ?? ''} onChange={(event) => setDraft((current) => ({ ...current, cancellationPolicies: event.target.value }))} />
                <TextAreaField label="Inclusions" value={draft.inclusions ?? ''} onChange={(event) => setDraft((current) => ({ ...current, inclusions: event.target.value }))} />
                <TextAreaField label="Exclusions" value={draft.exclusions ?? ''} onChange={(event) => setDraft((current) => ({ ...current, exclusions: event.target.value }))} />
                <TextAreaField label="Specials" value={draft.specials ?? ''} onChange={(event) => setDraft((current) => ({ ...current, specials: event.target.value }))} />
              </div>
            </div>

            <div className="grid gap-6 xl:grid-cols-2">
              <LookupListEditor
                title="Rate Types"
                hint="Reusable labels from AI Forged or manual setup"
                label="Rate Type"
                items={draft.rateTypes}
                onChange={(index, value) => updateLookupList('rateTypes', index, value)}
                onAdd={() => addLookupValue('rateTypes')}
                onRemove={(index) => removeLookupValue('rateTypes', index)}
              />
              <LookupListEditor
                title="Rate Bases"
                label="Rate Basis"
                items={draft.rateBases}
                onChange={(index, value) => updateLookupList('rateBases', index, value)}
                onAdd={() => addLookupValue('rateBases')}
                onRemove={(index) => removeLookupValue('rateBases', index)}
              />
              <LookupListEditor
                title="Meal Bases"
                label="Meal Basis"
                items={draft.mealBases}
                onChange={(index, value) => updateLookupList('mealBases', index, value)}
                onAdd={() => addLookupValue('mealBases')}
                onRemove={(index) => removeLookupValue('mealBases', index)}
              />
              <LookupListEditor
                title="Validity Periods"
                hint="Product-level seasonal periods"
                label="Validity Period"
                items={draft.validityPeriods}
                onChange={(index, value) => updateLookupList('validityPeriods', index, value)}
                onAdd={() => addLookupValue('validityPeriods')}
                onRemove={(index) => removeLookupValue('validityPeriods', index)}
              />
            </div>

            <div className="space-y-6">
              <div>
                <SectionTitle title="Contacts" hint="Every child row is editable" />
                <div className="space-y-3">
                  {draft.contacts.map((contact, index) => (
                    <div key={contact.id ?? `contact-${index}`} className="grid gap-3 rounded-lg border border-slate-200 bg-slate-50 p-3 md:grid-cols-5">
                      <FormInput label="Type" value={contact.contactType} onChange={(event) => setDraft((current) => ({ ...current, contacts: current.contacts.map((item, itemIndex) => itemIndex === index ? { ...item, contactType: event.target.value } : item) }))} />
                      <FormInput label="Name" value={contact.contactName} onChange={(event) => setDraft((current) => ({ ...current, contacts: current.contacts.map((item, itemIndex) => itemIndex === index ? { ...item, contactName: event.target.value } : item) }))} />
                      <FormInput label="Email" value={contact.contactEmail} onChange={(event) => setDraft((current) => ({ ...current, contacts: current.contacts.map((item, itemIndex) => itemIndex === index ? { ...item, contactEmail: event.target.value } : item) }))} />
                      <FormInput label="Phone" value={contact.contactPhoneNumber} onChange={(event) => setDraft((current) => ({ ...current, contacts: current.contacts.map((item, itemIndex) => itemIndex === index ? { ...item, contactPhoneNumber: event.target.value } : item) }))} />
                      <div className="flex items-end">
                        <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, contacts: current.contacts.filter((_, itemIndex) => itemIndex !== index) }))}>Remove</Button>
                      </div>
                    </div>
                  ))}
                  <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, contacts: [...current.contacts, emptyContact()] }))}>Add Contact</Button>
                </div>
              </div>

              <div>
                <SectionTitle title="Extras" />
                <div className="space-y-3">
                  {draft.extras.map((extra, index) => (
                    <div key={extra.id ?? `extra-${index}`} className="grid gap-3 rounded-lg border border-slate-200 bg-slate-50 p-3 md:grid-cols-[2fr_1fr_1fr_auto]">
                      <FormInput label="Description" value={extra.description} onChange={(event) => setDraft((current) => ({ ...current, extras: current.extras.map((item, itemIndex) => itemIndex === index ? { ...item, description: event.target.value } : item) }))} />
                      <FormInput label="Charge Unit" value={extra.chargeUnit} onChange={(event) => setDraft((current) => ({ ...current, extras: current.extras.map((item, itemIndex) => itemIndex === index ? { ...item, chargeUnit: event.target.value } : item) }))} />
                      <FormInput label="Charge" value={extra.charge} onChange={(event) => setDraft((current) => ({ ...current, extras: current.extras.map((item, itemIndex) => itemIndex === index ? { ...item, charge: event.target.value } : item) }))} />
                      <div className="flex items-end">
                        <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, extras: current.extras.filter((_, itemIndex) => itemIndex !== index) }))}>Remove</Button>
                      </div>
                    </div>
                  ))}
                  <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, extras: [...current.extras, emptyExtra()] }))}>Add Extra</Button>
                </div>
              </div>

              <div>
                <SectionTitle title="Rooms" hint="Rates can link to saved room ids" />
                <div className="space-y-3">
                  {draft.rooms.map((room, index) => (
                    <div key={room.id ?? `room-${index}`} className="rounded-lg border border-slate-200 bg-slate-50 p-3">
                      <div className="grid gap-3 md:grid-cols-2">
                        <FormInput label="Room Name" value={room.name} onChange={(event) => setDraft((current) => ({ ...current, rooms: current.rooms.map((item, itemIndex) => itemIndex === index ? { ...item, name: event.target.value } : item) }))} />
                        <FormInput label="Minimum Occupancy" value={room.minimumOccupancy ?? ''} onChange={(event) => setDraft((current) => ({ ...current, rooms: current.rooms.map((item, itemIndex) => itemIndex === index ? { ...item, minimumOccupancy: event.target.value } : item) }))} />
                        <FormInput label="Maximum Occupancy" value={room.maximumOccupancy ?? ''} onChange={(event) => setDraft((current) => ({ ...current, rooms: current.rooms.map((item, itemIndex) => itemIndex === index ? { ...item, maximumOccupancy: event.target.value } : item) }))} />
                        <div className="flex items-end justify-end">
                          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, rooms: current.rooms.filter((_, itemIndex) => itemIndex !== index) }))}>Remove Room</Button>
                        </div>
                      </div>
                      <div className="mt-3 grid gap-3 md:grid-cols-2">
                        <TextAreaField label="Additional Notes" value={room.additionalNotes ?? ''} onChange={(event) => setDraft((current) => ({ ...current, rooms: current.rooms.map((item, itemIndex) => itemIndex === index ? { ...item, additionalNotes: event.target.value } : item) }))} />
                        <TextAreaField label="Rate Conditions" value={room.rateConditions ?? ''} onChange={(event) => setDraft((current) => ({ ...current, rooms: current.rooms.map((item, itemIndex) => itemIndex === index ? { ...item, rateConditions: event.target.value } : item) }))} />
                      </div>
                    </div>
                  ))}
                  <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => setDraft((current) => ({ ...current, rooms: [...current.rooms, emptyRoom()] }))}>Add Room</Button>
                </div>
              </div>
            </div>

            <div className="flex flex-wrap gap-3">
              <Button type="submit" isLoading={createProductMutation.isPending || updateProductMutation.isPending}>{selectedProductId ? 'Save Product' : 'Create Product'}</Button>
              <Button
                type="button"
                className="bg-slate-200 text-slate-800 hover:bg-slate-300"
                onClick={() => {
                  setSelectedProductId(undefined);
                  setSelectedRateId(undefined);
                  setDraft(emptyProduct());
                  setRateDraft(emptyRate());
                }}
              >
                New Draft
              </Button>
            </div>

            {createProductMutation.isError ? <p className="text-sm text-red-600">{(createProductMutation.error as Error).message}</p> : null}
            {updateProductMutation.isError ? <p className="text-sm text-red-600">{(updateProductMutation.error as Error).message}</p> : null}
          </form>
        </Card>

        <Card title="Rates">
          {!selectedProductId ? <p className="text-sm text-slate-500">Save or select a product first, then manage current and historical rates here.</p> : null}
          {selectedProductId ? (
            <div className="space-y-6">
              <div className="space-y-2">
                {rates.map((rate) => (
                  <button
                    key={rate.id}
                    type="button"
                    onClick={() => {
                      setSelectedRateId(rate.id);
                      setRateDraft(toRateRequest(rate));
                    }}
                    className={`flex w-full items-center justify-between rounded-lg border px-3 py-3 text-left ${selectedRateId === rate.id ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 bg-slate-50 hover:border-slate-400 hover:bg-white'}`}
                  >
                    <div>
                      <div className="font-medium">{rate.productRoomName || 'Product-wide rate'} • {rate.currency} {rate.baseCost.toFixed(2)}</div>
                      <div className={`text-xs ${selectedRateId === rate.id ? 'text-slate-200' : 'text-slate-500'}`}>{rate.seasonStart} to {rate.seasonEnd} • {rate.pricingModel} • {rate.rateBasis || 'No basis'}</div>
                    </div>
                    <div className={`rounded-full px-2 py-1 text-[11px] font-medium ${rate.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-200 text-slate-700'}`}>{rate.isActive ? 'Active' : 'Historical'}</div>
                  </button>
                ))}
              </div>

              <form className="space-y-4 border-t border-slate-200 pt-4" onSubmit={(event) => void submitRate(event)}>
                <SectionTitle title={selectedRateId ? 'Create Rate Revision' : 'New Rate'} hint="Editing a saved rate creates a new active revision" />
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  <SelectDropdown label="Room" value={rateDraft.productRoomId ?? ''} options={roomOptions} onChange={(event) => setRateDraft((current) => ({ ...current, productRoomId: event.target.value || null }))} />
                  <SelectDropdown label="Pricing Model" value={rateDraft.pricingModel} options={pricingOptions} onChange={(event) => setRateDraft((current) => ({ ...current, pricingModel: event.target.value as PricingModel }))} />
                  <FormInput label="Currency" value={rateDraft.currency} onChange={(event) => setRateDraft((current) => ({ ...current, currency: event.target.value }))} />
                  <FormInput label="Season Start" type="date" value={rateDraft.seasonStart} onChange={(event) => setRateDraft((current) => ({ ...current, seasonStart: event.target.value }))} />
                  <FormInput label="Season End" type="date" value={rateDraft.seasonEnd} onChange={(event) => setRateDraft((current) => ({ ...current, seasonEnd: event.target.value }))} />
                  <FormInput label="Base Cost" type="number" step="0.01" value={rateDraft.baseCost} onChange={(event) => setRateDraft((current) => ({ ...current, baseCost: Number(event.target.value) || 0 }))} />
                  <FormInput label="Min Pax" type="number" value={rateDraft.minPax ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, minPax: event.target.value === '' ? null : Number(event.target.value) }))} />
                  <FormInput label="Max Pax" type="number" value={rateDraft.maxPax ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, maxPax: event.target.value === '' ? null : Number(event.target.value) }))} />
                  <FormInput label="Capacity" type="number" value={rateDraft.capacity ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, capacity: event.target.value === '' ? null : Number(event.target.value) }))} />
                  <FormInput label="Child Discount" type="number" step="0.01" value={rateDraft.childDiscount ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, childDiscount: event.target.value === '' ? null : Number(event.target.value) }))} />
                  <FormInput label="Single Supplement" type="number" step="0.01" value={rateDraft.singleSupplement ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, singleSupplement: event.target.value === '' ? null : Number(event.target.value) }))} />
                  <FormInput label="Rate Type" value={rateDraft.rateTypeName ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, rateTypeName: event.target.value }))} />
                  <FormInput label="Rate Basis" value={rateDraft.rateBasis ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, rateBasis: event.target.value }))} />
                  <FormInput label="Occupancy Type" value={rateDraft.occupancyType ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, occupancyType: event.target.value }))} />
                  <FormInput label="Meal Basis" value={rateDraft.mealBasis ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, mealBasis: event.target.value }))} />
                  <FormInput label="Rate Variation" value={rateDraft.rateVariation ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, rateVariation: event.target.value }))} />
                  <FormInput label="Validity Period" value={rateDraft.validityPeriod ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, validityPeriod: event.target.value }))} />
                  <FormInput label="Validity Description" value={rateDraft.validityPeriodDescription ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, validityPeriodDescription: event.target.value }))} />
                </div>
                <TextAreaField label="Minimum Stay" value={rateDraft.minimumStay ?? ''} onChange={(event) => setRateDraft((current) => ({ ...current, minimumStay: event.target.value }))} />
                <div className="flex flex-wrap gap-3">
                  <Button type="submit" isLoading={createRateMutation.isPending || updateRateMutation.isPending}>{selectedRateId ? 'Create Revision' : 'Create Rate'}</Button>
                  <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" onClick={() => { setSelectedRateId(undefined); setRateDraft(emptyRate(selectedProductId, draft.rooms[0]?.id ?? '')); }}>New Rate Draft</Button>
                </div>
                {createRateMutation.isError ? <p className="text-sm text-red-600">{(createRateMutation.error as Error).message}</p> : null}
                {updateRateMutation.isError ? <p className="text-sm text-red-600">{(updateRateMutation.error as Error).message}</p> : null}
              </form>
            </div>
          ) : null}
        </Card>
      </div>
    </div>
  );
};
