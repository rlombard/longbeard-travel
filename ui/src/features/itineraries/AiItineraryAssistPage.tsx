import { FormEvent, useMemo, useState } from 'react';
import { Button } from '../../components/Button';
import { Card } from '../../components/Card';
import { FormInput } from '../../components/FormInput';
import { MultiValueEditor } from '../../components/MultiValueEditor';
import { SelectDropdown } from '../../components/SelectDropdown';
import { Table } from '../../components/Table';
import { TextAreaField } from '../../components/TextAreaField';
import { useCreateItinerary } from './hooks';
import { useApproveItineraryDraft, useGenerateItineraryDraft, useProductAssist } from './aiHooks';
import { useProduct, useProducts } from '../products/hooks';
import { Badge } from '../../components/Badge';
import { CreateItineraryRequest } from '../../types/itinerary';
import { ItineraryDraft, ItineraryDraftItem, ProductAssistRequest } from '../../types/itineraryAi';
import { ProductType } from '../../types/product';
import { formatCurrency, formatDateOnly, formatDateTime, formatPercent } from '../../utils/formatters';

interface AssistFormState {
  destination: string;
  region: string;
  startDate: string;
  endDate: string;
  duration: number;
  season: string;
  travellerCount: number;
  budgetLevel: string;
  preferredCurrency: string;
  travelStyle: string;
  interests: string[];
  accommodationPreference: string;
  specialConstraints: string[];
  customerBrief: string;
  productTypes: ProductType[];
  maxResults: number;
}

interface WorkingItem {
  localId: string;
  dayNumber: number;
  productId: string;
  productName: string;
  quantity: number;
  notes: string;
  source: 'AI recommendation' | 'AI draft';
  unresolved: boolean;
  reason: string;
}

const emptyForm: AssistFormState = {
  destination: '',
  region: '',
  startDate: '',
  endDate: '',
  duration: 5,
  season: '',
  travellerCount: 2,
  budgetLevel: '',
  preferredCurrency: 'USD',
  travelStyle: '',
  interests: [],
  accommodationPreference: '',
  specialConstraints: [],
  customerBrief: '',
  productTypes: ['Tour', 'Hotel', 'Transport'],
  maxResults: 10
};

const buildProductAssistRequest = (form: AssistFormState): ProductAssistRequest => ({
  destination: form.destination || undefined,
  region: form.region || undefined,
  startDate: form.startDate || undefined,
  endDate: form.endDate || undefined,
  season: form.season || undefined,
  travellerCount: form.travellerCount || undefined,
  budgetLevel: form.budgetLevel || undefined,
  preferredCurrency: form.preferredCurrency || undefined,
  travelStyle: form.travelStyle || undefined,
  interests: form.interests,
  accommodationPreference: form.accommodationPreference || undefined,
  specialConstraints: form.specialConstraints,
  productTypes: form.productTypes,
  customerBrief: form.customerBrief || undefined,
  maxResults: form.maxResults
});

const toWorkingItem = (item: ItineraryDraftItem): WorkingItem => ({
  localId: item.id,
  dayNumber: item.dayNumber,
  productId: item.productId ?? '',
  productName: item.productName ?? item.title,
  quantity: item.quantity,
  notes: item.notes ?? '',
  source: 'AI draft',
  unresolved: item.isUnresolved || !item.productId,
  reason: item.reason
});

export const AiItineraryAssistPage = () => {
  const [form, setForm] = useState<AssistFormState>(emptyForm);
  const [selectedProductId, setSelectedProductId] = useState<string>();
  const [workingItems, setWorkingItems] = useState<WorkingItem[]>([]);
  const [draft, setDraft] = useState<ItineraryDraft | null>(null);
  const [draftItems, setDraftItems] = useState<WorkingItem[]>([]);
  const [decisionNotes, setDecisionNotes] = useState('');
  const [draftTouched, setDraftTouched] = useState(false);

  const { data: products = [] } = useProducts();
  const selectedProduct = useProduct(selectedProductId);
  const assistMutation = useProductAssist();
  const draftMutation = useGenerateItineraryDraft();
  const approveMutation = useApproveItineraryDraft();
  const createItineraryMutation = useCreateItinerary();

  const productOptions = useMemo(
    () => [{ label: 'Select product', value: '' }, ...products.map((product) => ({ label: product.name, value: product.id }))],
    [products]
  );

  const addRecommendationToWorkingDraft = (productId: string, productName: string, reason: string) => {
    setWorkingItems((current) => [
      ...current,
      {
        localId: crypto.randomUUID(),
        dayNumber: current.length + 1,
        productId,
        productName,
        quantity: 1,
        notes: '',
        source: 'AI recommendation',
        unresolved: false,
        reason
      }
    ]);
  };

  const onAssistSubmit = async (event: FormEvent) => {
    event.preventDefault();
    await assistMutation.mutateAsync(buildProductAssistRequest(form));
  };

  const onGenerateDraft = async () => {
    const generated = await draftMutation.mutateAsync({
      destination: form.destination || undefined,
      region: form.region || undefined,
      startDate: form.startDate || undefined,
      endDate: form.endDate || undefined,
      duration: form.duration || undefined,
      season: form.season || undefined,
      travellerCount: form.travellerCount || undefined,
      budgetLevel: form.budgetLevel || undefined,
      preferredCurrency: form.preferredCurrency || undefined,
      travelStyle: form.travelStyle || undefined,
      interests: form.interests,
      accommodationPreference: form.accommodationPreference || undefined,
      specialConstraints: form.specialConstraints,
      customerBrief: form.customerBrief || undefined
    });

    setDraft(generated);
    setDraftItems(generated.items.map(toWorkingItem));
    setDecisionNotes('');
    setDraftTouched(false);
  };

  const updateDraftItem = (localId: string, updates: Partial<WorkingItem>) => {
    setDraftTouched(true);
    setDraftItems((current) => current.map((item) => (item.localId === localId ? { ...item, ...updates } : item)));
  };

  const moveDraftItem = (localId: string, direction: -1 | 1) => {
    setDraftTouched(true);
    setDraftItems((current) => {
      const index = current.findIndex((item) => item.localId === localId);
      const nextIndex = index + direction;

      if (index < 0 || nextIndex < 0 || nextIndex >= current.length) {
        return current;
      }

      const next = [...current];
      const [removed] = next.splice(index, 1);
      next.splice(nextIndex, 0, removed);

      return next.map((item, itemIndex) => ({
        ...item,
        dayNumber: itemIndex + 1
      }));
    });
  };

  const removeDraftItem = (localId: string) => {
    setDraftTouched(true);
    setDraftItems((current) =>
      current
        .filter((item) => item.localId !== localId)
        .map((item, index) => ({
          ...item,
          dayNumber: index + 1
        }))
    );
  };

  const unresolvedCount = draftItems.filter((item) => !item.productId).length;

  const saveWorkingDraftManually = async () => {
    const payload: CreateItineraryRequest = {
      startDate: form.startDate,
      duration: form.duration,
      items: workingItems
        .filter((item) => item.productId)
        .map((item) => ({
          dayNumber: item.dayNumber,
          productId: item.productId,
          quantity: item.quantity,
          notes: item.notes || undefined
        }))
    };

    await createItineraryMutation.mutateAsync(payload);
  };

  const approveDraft = async () => {
    if (!draft) {
      return;
    }

    await approveMutation.mutateAsync({
      draftId: draft.id,
      payload: {
        startDate: form.startDate || draft.proposedStartDate || undefined,
        duration: form.duration || draft.duration,
        decisionNotes: decisionNotes || undefined,
        items: draftItems
          .filter((item) => item.productId)
          .map((item) => ({
            dayNumber: item.dayNumber,
            productId: item.productId,
            quantity: item.quantity,
            notes: item.notes || undefined
          }))
      }
    });
  };

  return (
    <div className="space-y-6">
      <Card title="AI Itinerary Assist">
        <div className="mb-5 flex flex-wrap items-center gap-2">
          <Badge tone="ai">AI generated suggestions</Badge>
          <Badge tone={draftTouched ? 'warning' : 'info'}>{draftTouched ? 'Operator reviewed' : 'Awaiting operator review'}</Badge>
          {approveMutation.isSuccess ? <Badge tone="success">Saved as itinerary</Badge> : null}
        </div>

        <div className="grid gap-6 xl:grid-cols-[0.95fr_1.2fr_0.85fr]">
          <div className="space-y-4">
            <form className="space-y-4 rounded-2xl border border-slate-200 bg-slate-50 p-4" onSubmit={onAssistSubmit}>
              <h3 className="text-sm font-semibold text-slate-900">Trip brief</h3>
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-1">
                <FormInput label="Destination" value={form.destination} onChange={(event) => setForm((current) => ({ ...current, destination: event.target.value }))} />
                <FormInput label="Region" value={form.region} onChange={(event) => setForm((current) => ({ ...current, region: event.target.value }))} />
                <FormInput label="Start Date" type="date" value={form.startDate} onChange={(event) => setForm((current) => ({ ...current, startDate: event.target.value }))} />
                <FormInput label="End Date" type="date" value={form.endDate} onChange={(event) => setForm((current) => ({ ...current, endDate: event.target.value }))} />
                <FormInput label="Duration" type="number" min={1} value={form.duration} onChange={(event) => setForm((current) => ({ ...current, duration: Math.max(1, Number(event.target.value) || 1) }))} />
                <FormInput label="Travellers" type="number" min={1} value={form.travellerCount} onChange={(event) => setForm((current) => ({ ...current, travellerCount: Math.max(1, Number(event.target.value) || 1) }))} />
                <FormInput label="Budget" value={form.budgetLevel} onChange={(event) => setForm((current) => ({ ...current, budgetLevel: event.target.value }))} />
                <FormInput label="Currency" value={form.preferredCurrency} onChange={(event) => setForm((current) => ({ ...current, preferredCurrency: event.target.value.toUpperCase() }))} />
                <FormInput label="Travel Style" value={form.travelStyle} onChange={(event) => setForm((current) => ({ ...current, travelStyle: event.target.value }))} />
                <FormInput label="Accommodation" value={form.accommodationPreference} onChange={(event) => setForm((current) => ({ ...current, accommodationPreference: event.target.value }))} />
                <FormInput label="Season" value={form.season} onChange={(event) => setForm((current) => ({ ...current, season: event.target.value }))} />
                <FormInput label="Max Results" type="number" min={1} max={25} value={form.maxResults} onChange={(event) => setForm((current) => ({ ...current, maxResults: Math.max(1, Number(event.target.value) || 1) }))} />
              </div>

              <MultiValueEditor label="Interests" values={form.interests} onChange={(values) => setForm((current) => ({ ...current, interests: values }))} placeholder="safari, wine, family" />
              <MultiValueEditor
                label="Special Constraints"
                values={form.specialConstraints}
                onChange={(values) => setForm((current) => ({ ...current, specialConstraints: values }))}
                placeholder="wheelchair, no red meat"
              />
              <TextAreaField label="Customer Brief" rows={5} value={form.customerBrief} onChange={(event) => setForm((current) => ({ ...current, customerBrief: event.target.value }))} />

              <div className="space-y-2">
                <p className="text-sm font-medium text-slate-700">Product types</p>
                <div className="flex flex-wrap gap-2">
                  {(['Tour', 'Hotel', 'Transport'] as ProductType[]).map((type) => {
                    const active = form.productTypes.includes(type);
                    return (
                      <button
                        key={type}
                        type="button"
                        className={`rounded-full px-3 py-1.5 text-sm font-medium ${active ? 'bg-slate-900 text-white' : 'bg-white text-slate-700 ring-1 ring-slate-200'}`}
                        onClick={() =>
                          setForm((current) => ({
                            ...current,
                            productTypes: current.productTypes.includes(type)
                              ? current.productTypes.filter((item) => item !== type)
                              : [...current.productTypes, type]
                          }))
                        }
                      >
                        {type}
                      </button>
                    );
                  })}
                </div>
              </div>

              <div className="flex flex-wrap gap-3">
                <Button isLoading={assistMutation.isPending}>Find products</Button>
                <Button type="button" className="bg-indigo-600 hover:bg-indigo-500" isLoading={draftMutation.isPending} onClick={() => void onGenerateDraft()}>
                  Generate AI draft
                </Button>
              </div>
            </form>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-slate-900">Working itinerary basket</h3>
                <Badge tone="info">{workingItems.length} items</Badge>
              </div>
              {workingItems.length === 0 ? <p className="text-sm text-slate-500">Add products from AI recommendations.</p> : null}
              {workingItems.length > 0 ? (
                <div className="space-y-3">
                  {workingItems.map((item) => (
                    <div key={item.localId} className="rounded-xl border border-slate-200 bg-slate-50 p-3">
                      <div className="flex items-center justify-between gap-3">
                        <div>
                          <p className="text-sm font-semibold text-slate-900">Day {item.dayNumber}: {item.productName}</p>
                          <p className="text-xs text-slate-500">{item.reason}</p>
                        </div>
                        <Badge tone="ai">{item.source}</Badge>
                      </div>
                    </div>
                  ))}
                  <Button
                    type="button"
                    className="bg-emerald-600 hover:bg-emerald-500"
                    disabled={!form.startDate || workingItems.length === 0}
                    isLoading={createItineraryMutation.isPending}
                    onClick={() => void saveWorkingDraftManually()}
                  >
                    Save Working Draft Manually
                  </Button>
                  {createItineraryMutation.isSuccess ? (
                    <p className="text-sm text-emerald-700">Saved itinerary {createItineraryMutation.data.id}</p>
                  ) : null}
                </div>
              ) : null}
            </div>
          </div>

          <div className="space-y-4">
            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-slate-900">Product recommendations</h3>
                {assistMutation.data ? <Badge tone="ai">{assistMutation.data.returnedCount} shown</Badge> : null}
              </div>
              {assistMutation.isError ? <p className="mb-3 text-sm text-rose-600">{(assistMutation.error as Error).message}</p> : null}
              {!assistMutation.data ? <p className="text-sm text-slate-500">Run AI product assist to rank matching catalog products.</p> : null}
              {assistMutation.data ? (
                <Table headers={['Product', 'Type', 'Match', 'Reason', 'Action']}>
                  {assistMutation.data.recommendations.map((recommendation) => (
                    <tr key={recommendation.productId} className="border-t border-slate-200 align-top">
                      <td className="px-3 py-3">
                        <button
                          type="button"
                          className="text-left"
                          onClick={() => setSelectedProductId(recommendation.productId)}
                        >
                          <p className="font-medium text-slate-900">{recommendation.productName}</p>
                          <p className="text-xs text-slate-500">{recommendation.supplierName}</p>
                        </button>
                      </td>
                      <td className="px-3 py-3 text-slate-700">{recommendation.productType}</td>
                      <td className="px-3 py-3">
                        <Badge tone={recommendation.matchScore >= 0.75 ? 'success' : recommendation.matchScore >= 0.45 ? 'warning' : 'danger'}>
                          {formatPercent(recommendation.matchScore)}
                        </Badge>
                      </td>
                      <td className="px-3 py-3">
                        <p className="text-sm text-slate-700">{recommendation.reason}</p>
                        <div className="mt-2 flex flex-wrap gap-1">
                          {recommendation.warnings.map((warning) => <Badge key={warning} tone="warning">{warning}</Badge>)}
                          {recommendation.missingData.map((item) => <Badge key={item} tone="danger">{item}</Badge>)}
                        </div>
                      </td>
                      <td className="px-3 py-3">
                        <Button
                          type="button"
                          className="bg-slate-200 text-slate-800 hover:bg-slate-300"
                          onClick={() => addRecommendationToWorkingDraft(recommendation.productId, recommendation.productName, recommendation.reason)}
                        >
                          Add to draft
                        </Button>
                      </td>
                    </tr>
                  ))}
                </Table>
              ) : null}
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="text-sm font-semibold text-slate-900">AI draft canvas</h3>
                {draft ? <Badge tone="ai">{draft.status}</Badge> : null}
              </div>
              {!draft ? <p className="text-sm text-slate-500">Generate a draft itinerary to start operator review.</p> : null}
              {draft ? (
                <div className="space-y-4">
                  <div className="grid gap-3 md:grid-cols-3">
                    <p className="text-sm text-slate-700"><strong>Draft:</strong> <span className="font-mono text-xs">{draft.id}</span></p>
                    <p className="text-sm text-slate-700"><strong>Proposed Start:</strong> {formatDateOnly(draft.proposedStartDate)}</p>
                    <p className="text-sm text-slate-700"><strong>Duration:</strong> {draft.duration} days</p>
                  </div>

                  {draftItems.map((item, index) => (
                    <div key={item.localId} className={`rounded-2xl border p-4 ${item.productId ? 'border-slate-200 bg-slate-50' : 'border-rose-200 bg-rose-50/60'}`}>
                      <div className="mb-3 flex flex-wrap items-start justify-between gap-3">
                        <div>
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="text-sm font-semibold text-slate-900">Day {item.dayNumber}: {item.productName}</p>
                            {item.unresolved || !item.productId ? <Badge tone="danger">Review required</Badge> : <Badge tone="ai">AI mapped</Badge>}
                          </div>
                          <p className="mt-1 text-xs text-slate-500">{item.reason}</p>
                        </div>
                        <div className="flex gap-2">
                          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" disabled={index === 0} onClick={() => moveDraftItem(item.localId, -1)}>
                            Up
                          </Button>
                          <Button type="button" className="bg-slate-200 text-slate-800 hover:bg-slate-300" disabled={index === draftItems.length - 1} onClick={() => moveDraftItem(item.localId, 1)}>
                            Down
                          </Button>
                          <Button type="button" className="bg-rose-600 hover:bg-rose-500" onClick={() => removeDraftItem(item.localId)}>
                            Remove
                          </Button>
                        </div>
                      </div>

                      <div className="grid gap-3 md:grid-cols-4">
                        <FormInput label="Day" type="number" min={1} value={item.dayNumber} onChange={(event) => updateDraftItem(item.localId, { dayNumber: Math.max(1, Number(event.target.value) || 1) })} />
                        <SelectDropdown
                          label="Mapped Product"
                          value={item.productId}
                          options={productOptions}
                          onChange={(event) => {
                            const product = products.find((entry) => entry.id === event.target.value);
                            updateDraftItem(item.localId, {
                              productId: event.target.value,
                              productName: product?.name ?? item.productName,
                              unresolved: !event.target.value
                            });
                          }}
                        />
                        <FormInput label="Quantity" type="number" min={1} value={item.quantity} onChange={(event) => updateDraftItem(item.localId, { quantity: Math.max(1, Number(event.target.value) || 1) })} />
                        <FormInput label="Notes" value={item.notes} onChange={(event) => updateDraftItem(item.localId, { notes: event.target.value })} />
                      </div>

                      <div className="mt-3 flex flex-wrap gap-2">
                        {item.productId ? <Badge tone="success">Ready</Badge> : <Badge tone="danger">Needs product mapping</Badge>}
                        {item.unresolved ? <Badge tone="warning">Unresolved placeholder</Badge> : null}
                      </div>
                    </div>
                  ))}

                  <TextAreaField label="Decision Notes" rows={3} value={decisionNotes} onChange={(event) => setDecisionNotes(event.target.value)} />

                  <div className="flex flex-wrap items-center gap-3">
                    <Button
                      type="button"
                      className="bg-emerald-600 hover:bg-emerald-500"
                      disabled={!draft || unresolvedCount > 0 || draftItems.length === 0}
                      isLoading={approveMutation.isPending}
                      onClick={() => void approveDraft()}
                    >
                      Save As Itinerary
                    </Button>
                    <p className={`text-sm ${unresolvedCount > 0 ? 'text-rose-600' : 'text-slate-500'}`}>
                      {unresolvedCount > 0 ? `${unresolvedCount} unresolved items must be mapped or removed before save.` : 'All items resolved.'}
                    </p>
                  </div>

                  {approveMutation.isSuccess ? (
                    <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                      Saved itinerary <span className="font-mono">{approveMutation.data.itinerary.id}</span> at {formatDateTime(approveMutation.data.approvedAt)}
                    </div>
                  ) : null}
                  {approveMutation.isError ? <p className="text-sm text-rose-600">{(approveMutation.error as Error).message}</p> : null}
                </div>
              ) : null}
            </div>
          </div>

          <div className="space-y-4">
            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">AI context</h3>
              {assistMutation.data?.assumptions.length ? (
                <div className="mb-3">
                  <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Product assist assumptions</p>
                  <div className="flex flex-wrap gap-2">
                    {assistMutation.data.assumptions.map((assumption) => <Badge key={assumption} tone="warning">{assumption}</Badge>)}
                  </div>
                </div>
              ) : null}
              {draft ? (
                <div className="space-y-3">
                  <div>
                    <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Assumptions</p>
                    <div className="flex flex-wrap gap-2">{draft.assumptions.map((assumption) => <Badge key={assumption} tone="warning">{assumption}</Badge>)}</div>
                  </div>
                  <div>
                    <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Caveats</p>
                    <div className="flex flex-wrap gap-2">{draft.caveats.map((caveat) => <Badge key={caveat} tone="danger">{caveat}</Badge>)}</div>
                  </div>
                  <div>
                    <p className="mb-2 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Data gaps</p>
                    <div className="flex flex-wrap gap-2">{draft.dataGaps.map((gap) => <Badge key={gap} tone="danger">{gap}</Badge>)}</div>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-slate-500">AI assumptions and caveats appear here after generation.</p>
              )}
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
              <h3 className="mb-3 text-sm font-semibold text-slate-900">Product detail</h3>
              {selectedProduct.isLoading ? <p className="text-sm text-slate-500">Loading product...</p> : null}
              {selectedProduct.data ? (
                <div className="space-y-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="text-sm font-semibold text-slate-900">{selectedProduct.data.name}</p>
                    <Badge tone="info">{selectedProduct.data.type}</Badge>
                  </div>
                  <p className="text-sm text-slate-700"><strong>Supplier:</strong> <span className="font-mono text-xs">{selectedProduct.data.supplierId}</span></p>
                  <p className="text-sm text-slate-700"><strong>Contract:</strong> {selectedProduct.data.contractValidityPeriod ?? '-'}</p>
                  <p className="text-sm text-slate-700"><strong>Check-in/out:</strong> {selectedProduct.data.checkInTime ?? '-'} / {selectedProduct.data.checkOutTime ?? '-'}</p>
                  <p className="text-sm text-slate-700"><strong>Commission:</strong> {selectedProduct.data.commission ?? '-'}</p>
                  <p className="text-sm text-slate-700"><strong>Tourism levy:</strong> {selectedProduct.data.tourismLevy.amount ? formatCurrency(Number(selectedProduct.data.tourismLevy.amount), selectedProduct.data.tourismLevy.currency ?? form.preferredCurrency ?? 'USD') : 'Not set'}</p>
                  <p className="text-sm text-slate-700"><strong>Inclusions:</strong> {selectedProduct.data.inclusions ?? '-'}</p>
                  <p className="text-sm text-slate-700"><strong>Exclusions:</strong> {selectedProduct.data.exclusions ?? '-'}</p>
                </div>
              ) : (
                <p className="text-sm text-slate-500">Select a recommendation to inspect product details.</p>
              )}
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};
