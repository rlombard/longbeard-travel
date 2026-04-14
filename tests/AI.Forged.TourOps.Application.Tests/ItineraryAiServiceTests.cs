using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Application.Services.Ai;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class ItineraryAiServiceTests
{
    [Fact]
    public async Task ProductAssist_IgnoresUnknownAiProductIds()
    {
        var hotel = BuildProduct("Cape Grace Lodge", ProductType.Hotel, "Cape Town", "South Africa", 240m, "USD");
        var transport = BuildProduct("Airport Shuttle", ProductType.Transport, "Johannesburg", "South Africa", 80m, "USD");
        var productRepository = new FakeProductRepository([hotel, transport]);
        var llmService = new FakeGenericLlmService(request => request.Operation switch
        {
            "itinerary.product-assist" => new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    matchScore = 0.99m,
                    reason = "Invalid result",
                    warnings = Array.Empty<string>(),
                    assumptionFlags = Array.Empty<string>(),
                    missingData = Array.Empty<string>()
                }
            },
            _ => Array.Empty<object>()
        });

        var service = CreateService(productRepository, new FakeItineraryRepository(), llmService);

        var result = await service.GetProductAssistanceAsync(new ItineraryProductAssistRequest
        {
            Destination = "Cape Town",
            StartDate = new DateOnly(2026, 6, 10),
            EndDate = new DateOnly(2026, 6, 14),
            TravellerCount = 2,
            AccommodationPreference = "boutique hotel",
            CustomerBrief = "Need a Cape Town hotel with boutique style",
            MaxResults = 5
        });

        Assert.NotEmpty(result.Recommendations);
        Assert.Equal(hotel.Id, result.Recommendations[0].ProductId);
        Assert.DoesNotContain(result.Recommendations, x => x.ProductId == transport.Id && x.MatchScore > result.Recommendations[0].MatchScore);
    }

    [Fact]
    public async Task GenerateDraft_ConvertsUnknownAiProductIdsIntoUnresolvedPlaceholders()
    {
        var hotel = BuildProduct("Cape Grace Lodge", ProductType.Hotel, "Cape Town", "South Africa", 240m, "USD");
        var productRepository = new FakeProductRepository([hotel]);
        var itineraryRepository = new FakeItineraryRepository();
        var llmService = new FakeGenericLlmService(request => request.Operation switch
        {
            "itinerary.draft-generate" => new
            {
                assumptions = new[] { "Test assumption" },
                caveats = new[] { "Review required" },
                dataGaps = new[] { "Tour match missing" },
                items = new[]
                {
                    new
                    {
                        dayNumber = 1,
                        sequence = 1,
                        title = "Accommodation stay",
                        productId = Guid.NewGuid(),
                        quantity = 1,
                        notes = "AI tried a product outside shortlist",
                        confidence = 0.88m,
                        reason = "Mapped outside shortlist",
                        warnings = new[] { "Invalid product id" },
                        missingData = new[] { "Product mapping" }
                    }
                }
            },
            _ => Array.Empty<object>()
        });

        var service = CreateService(productRepository, itineraryRepository, llmService);

        var draft = await service.GenerateDraftAsync(new GenerateItineraryDraftRequest
        {
            Destination = "Cape Town",
            Duration = 3,
            CustomerBrief = "Three night city stay"
        });

        Assert.Equal(ItineraryDraftStatus.Draft, draft.Status);
        Assert.Single(draft.Items);
        Assert.True(draft.Items[0].IsUnresolved);
        Assert.Null(draft.Items[0].ProductId);
        Assert.Single(itineraryRepository.Drafts);
    }

    [Fact]
    public async Task ApproveDraft_RejectsUnresolvedStoredItemsWithoutOverrides()
    {
        var draftId = Guid.NewGuid();
        var itineraryRepository = new FakeItineraryRepository();
        itineraryRepository.Drafts[draftId] = new ItineraryDraft
        {
            Id = draftId,
            RequestedByUserId = "operator-1",
            ProposedStartDate = new DateOnly(2026, 7, 1),
            Duration = 2,
            InputJson = "{}",
            Status = ItineraryDraftStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items =
            [
                new ItineraryDraftItem
                {
                    Id = Guid.NewGuid(),
                    ItineraryDraftId = draftId,
                    DayNumber = 1,
                    Sequence = 1,
                    Title = "Need hotel",
                    ProductId = null,
                    Quantity = 1,
                    Confidence = 0.4m,
                    Reason = "No hotel mapped",
                    IsUnresolved = true
                }
            ]
        };

        var service = CreateService(new FakeProductRepository([]), itineraryRepository, new FakeGenericLlmService(_ => Array.Empty<object>()));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApproveDraftAsync(draftId, new ApproveItineraryDraftRequest()));
    }

    [Fact]
    public async Task ApproveDraft_PersistsItineraryWhenOperatorSuppliesOverrides()
    {
        var draftId = Guid.NewGuid();
        var product = BuildProduct("Cape Grace Lodge", ProductType.Hotel, "Cape Town", "South Africa", 240m, "USD");
        var productRepository = new FakeProductRepository([product]);
        var itineraryRepository = new FakeItineraryRepository();
        itineraryRepository.Drafts[draftId] = new ItineraryDraft
        {
            Id = draftId,
            RequestedByUserId = "operator-1",
            ProposedStartDate = new DateOnly(2026, 7, 1),
            Duration = 2,
            InputJson = "{}",
            Status = ItineraryDraftStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items =
            [
                new ItineraryDraftItem
                {
                    Id = Guid.NewGuid(),
                    ItineraryDraftId = draftId,
                    DayNumber = 1,
                    Sequence = 1,
                    Title = "Need hotel",
                    ProductId = null,
                    Quantity = 1,
                    Confidence = 0.4m,
                    Reason = "No hotel mapped",
                    IsUnresolved = true
                }
            ]
        };

        var itineraryService = new FakeItineraryService();
        var humanApprovalService = new FakeHumanApprovalService();
        var service = CreateService(productRepository, itineraryRepository, new FakeGenericLlmService(_ => Array.Empty<object>()), itineraryService, humanApprovalService);

        var result = await service.ApproveDraftAsync(draftId, new ApproveItineraryDraftRequest
        {
            Items =
            [
                new ApproveItineraryDraftItemModel
                {
                    DayNumber = 1,
                    ProductId = product.Id,
                    Quantity = 1,
                    Notes = "Approved by operator"
                }
            ],
            DecisionNotes = "Looks good."
        });

        Assert.Equal(itineraryService.CreatedItinerary!.Id, result.Itinerary.Id);
        Assert.Single(humanApprovalService.CreatedRequests);
        Assert.Equal(ItineraryDraftStatus.Approved, itineraryRepository.Drafts[draftId].Status);
        Assert.Equal(result.Itinerary.Id, itineraryRepository.Drafts[draftId].PersistedItineraryId);
    }

    private static ItineraryAiService CreateService(
        FakeProductRepository productRepository,
        FakeItineraryRepository itineraryRepository,
        FakeGenericLlmService llmService,
        FakeItineraryService? itineraryService = null,
        FakeHumanApprovalService? humanApprovalService = null)
    {
        return new ItineraryAiService(
            productRepository,
            itineraryRepository,
            itineraryService ?? new FakeItineraryService(),
            new FakeCurrentUserContext(),
            humanApprovalService ?? new FakeHumanApprovalService(),
            llmService);
    }

    private static Product BuildProduct(string name, ProductType type, string city, string country, decimal baseCost, string currency)
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = $"{name} Supplier",
            CreatedAt = DateTime.UtcNow
        };

        return new Product
        {
            Id = Guid.NewGuid(),
            SupplierId = supplier.Id,
            Supplier = supplier,
            Name = name,
            Type = type,
            PhysicalTownOrCity = city,
            PhysicalCountry = country,
            Inclusions = type == ProductType.Hotel ? "Boutique stay with breakfast" : "Transfer support",
            CreatedAt = DateTime.UtcNow,
            Rates =
            [
                new Rate
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    SeasonStart = new DateOnly(2026, 1, 1),
                    SeasonEnd = new DateOnly(2026, 12, 31),
                    PricingModel = PricingModel.PerPerson,
                    BaseCost = baseCost,
                    Currency = currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            ]
        };
    }

    private sealed class FakeProductRepository(List<Product> planningProducts) : IProductRepository
    {
        public List<Product> PlanningProducts { get; } = planningProducts;

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default) => Task.FromResult(product);
        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Product>>(PlanningProducts);
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(PlanningProducts.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Product>>(PlanningProducts.Where(x => ids.Contains(x.Id)).ToList());
        public Task<IReadOnlyList<Product>> SearchForItineraryPlanningAsync(ProductCatalogFilter filter, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Product>>(PlanningProducts.Take(filter.MaxResults).ToList());
        public Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default) => Task.FromResult(product);
    }

    private sealed class FakeItineraryRepository : IItineraryRepository
    {
        public Dictionary<Guid, ItineraryDraft> Drafts { get; } = [];

        public Task<Itinerary> AddAsync(Itinerary itinerary, IEnumerable<ItineraryItem> items, CancellationToken cancellationToken = default)
        {
            itinerary.Items = items.ToList();
            return Task.FromResult(itinerary);
        }

        public Task<ItineraryDraft> AddDraftAsync(ItineraryDraft draft, IEnumerable<ItineraryDraftItem> items, CancellationToken cancellationToken = default)
        {
            draft.Items = items.ToList();
            Drafts[draft.Id] = draft;
            return Task.FromResult(draft);
        }

        public Task<ItineraryDraft?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Drafts.TryGetValue(id, out var draft) ? CloneDraft(draft) : null);

        public Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Itinerary?>(null);

        public Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateDraftAsync(ItineraryDraft draft, CancellationToken cancellationToken = default)
        {
            Drafts[draft.Id] = CloneDraft(draft);
            return Task.CompletedTask;
        }

        private static ItineraryDraft CloneDraft(ItineraryDraft draft) => new()
        {
            Id = draft.Id,
            RequestedByUserId = draft.RequestedByUserId,
            ProposedStartDate = draft.ProposedStartDate,
            Duration = draft.Duration,
            InputJson = draft.InputJson,
            CustomerBrief = draft.CustomerBrief,
            AssumptionsJson = draft.AssumptionsJson,
            CaveatsJson = draft.CaveatsJson,
            DataGapsJson = draft.DataGapsJson,
            LlmProvider = draft.LlmProvider,
            LlmModel = draft.LlmModel,
            AuditMetadataJson = draft.AuditMetadataJson,
            Status = draft.Status,
            PersistedItineraryId = draft.PersistedItineraryId,
            ApprovedByUserId = draft.ApprovedByUserId,
            ApprovedAt = draft.ApprovedAt,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt,
            Items = draft.Items.Select(item => new ItineraryDraftItem
            {
                Id = item.Id,
                ItineraryDraftId = item.ItineraryDraftId,
                DayNumber = item.DayNumber,
                Sequence = item.Sequence,
                Title = item.Title,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SupplierName = item.SupplierName,
                Quantity = item.Quantity,
                Notes = item.Notes,
                Confidence = item.Confidence,
                Reason = item.Reason,
                IsUnresolved = item.IsUnresolved,
                WarningFlagsJson = item.WarningFlagsJson,
                MissingDataJson = item.MissingDataJson
            }).ToList()
        };
    }

    private sealed class FakeItineraryService : IItineraryService
    {
        public ItineraryModel? CreatedItinerary { get; private set; }

        public Task<ItineraryModel> CreateItineraryAsync(CreateItineraryModel request, CancellationToken cancellationToken = default)
        {
            CreatedItinerary = new ItineraryModel
            {
                Id = Guid.NewGuid(),
                StartDate = request.StartDate,
                Duration = request.Duration,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.Select(item => new ItineraryItemModel
                {
                    Id = Guid.NewGuid(),
                    DayNumber = item.DayNumber,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Notes = item.Notes
                }).ToList()
            };

            return Task.FromResult(CreatedItinerary);
        }

        public Task<ItineraryModel?> GetItineraryAsync(Guid itineraryId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ItineraryModel?>(CreatedItinerary);
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public string GetRequiredUserId() => "operator-1";
    }

    private sealed class FakeHumanApprovalService : IHumanApprovalService
    {
        public List<HumanApprovalRequest> CreatedRequests { get; } = [];

        public Task<HumanApprovalRequest> CreateApprovalRequestAsync(string actionType, string entityType, Guid entityId, string? payloadJson, CancellationToken cancellationToken = default)
        {
            var request = new HumanApprovalRequest
            {
                Id = Guid.NewGuid(),
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                RequestedByUserId = "operator-1",
                Status = HumanApprovalStatus.Pending,
                PayloadJson = payloadJson,
                CreatedAt = DateTime.UtcNow
            };

            CreatedRequests.Add(request);
            return Task.FromResult(request);
        }

        public Task<HumanApprovalRequest> ApproveActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default)
        {
            var request = CreatedRequests.First(x => x.Id == approvalRequestId);
            request.Status = HumanApprovalStatus.Approved;
            request.DecisionNotes = decisionNotes;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = "operator-1";
            return Task.FromResult(request);
        }

        public Task<HumanApprovalRequest> RejectActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeGenericLlmService(Func<LlmRequest, object> structuredResponseFactory) : IGenericLlmService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public Task<LlmClassificationResult<T>> ClassifyAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<LlmDraftResult> DraftReplyAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<LlmStructuredResult<T>> ExtractStructuredDataAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) =>
            GenerateStructuredAsync<T>(request, cancellationToken);

        public Task<LlmStructuredResult<T>> GenerateStructuredAsync<T>(LlmRequest request, CancellationToken cancellationToken = default)
        {
            var payload = structuredResponseFactory(request);
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var data = JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new InvalidOperationException("Fake LLM could not deserialize payload.");

            return Task.FromResult(new LlmStructuredResult<T>
            {
                Data = data,
                RawContent = json,
                Provider = "Fake",
                Model = "fake-model"
            });
        }

        public Task<LlmTextResult> GenerateTextAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<LlmTextResult> SummarizeAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
