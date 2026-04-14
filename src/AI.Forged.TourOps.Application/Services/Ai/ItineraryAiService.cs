using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Ai;

public class ItineraryAiService(
    IProductRepository productRepository,
    IItineraryRepository itineraryRepository,
    IItineraryService itineraryService,
    ICurrentUserContext currentUserContext,
    IHumanApprovalService humanApprovalService,
    IGenericLlmService genericLlmService) : IItineraryAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ItineraryProductAssistResult> GetProductAssistanceAsync(ItineraryProductAssistRequest request, CancellationToken cancellationToken = default)
    {
        var plan = BuildPlanningContext(request);
        var candidates = await LoadCandidatesAsync(plan, plan.ProductTypes, cancellationToken);
        var scored = ScoreCandidates(candidates, plan);
        ApplyBudgetSignal(scored, plan);

        var shortlist = scored
            .OrderByDescending(x => x.DeterministicScore)
            .ThenBy(x => x.Product.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(plan.MaxResults, 12))
            .ToList();

        var aiRecommendations = await RankCandidatesAsync(plan, shortlist, cancellationToken);
        var recommendations = BuildRecommendationResponse(shortlist, aiRecommendations, plan.MaxResults);

        return new ItineraryProductAssistResult
        {
            CandidateCount = candidates.Count,
            ReturnedCount = recommendations.Count,
            Assumptions = plan.Assumptions
                .Append("AI ranking only considered the deterministic shortlist.")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Recommendations = recommendations
        };
    }

    public async Task<ItineraryDraftModel> GenerateDraftAsync(GenerateItineraryDraftRequest request, CancellationToken cancellationToken = default)
    {
        var plan = BuildPlanningContext(request);
        var candidates = await LoadCandidatesAsync(plan, [], cancellationToken);
        var scored = ScoreCandidates(candidates, plan);
        ApplyBudgetSignal(scored, plan);

        var shortlist = BuildDraftShortlist(scored);
        var aiDraft = await GenerateDraftProposalAsync(plan, shortlist, cancellationToken);
        var draftContent = MapDraftContent(plan, shortlist, aiDraft);
        var userId = currentUserContext.GetRequiredUserId();
        var now = DateTime.UtcNow;

        var draft = new ItineraryDraft
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = userId,
            ProposedStartDate = plan.StartDate,
            Duration = plan.Duration,
            InputJson = Serialize(new
            {
                request.Destination,
                request.Region,
                request.StartDate,
                request.EndDate,
                request.Duration,
                request.Season,
                request.TravellerCount,
                request.BudgetLevel,
                request.PreferredCurrency,
                request.TravelStyle,
                request.Interests,
                request.AccommodationPreference,
                request.SpecialConstraints,
                request.CustomerBrief
            }),
            CustomerBrief = NormalizeOptional(request.CustomerBrief, 4000),
            AssumptionsJson = Serialize(draftContent.Assumptions),
            CaveatsJson = Serialize(draftContent.Caveats),
            DataGapsJson = Serialize(draftContent.DataGaps),
            LlmProvider = aiDraft?.Provider,
            LlmModel = aiDraft?.Model,
            AuditMetadataJson = Serialize(new
            {
                candidateCount = candidates.Count,
                shortlistCount = shortlist.Count,
                unresolvedItemCount = draftContent.Items.Count(x => x.IsUnresolved),
                generatedAt = now,
                requestedByUserId = userId
            }),
            Status = ItineraryDraftStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        var draftItems = draftContent.Items.Select(item => new ItineraryDraftItem
        {
            Id = Guid.NewGuid(),
            ItineraryDraftId = draft.Id,
            DayNumber = item.DayNumber,
            Sequence = item.Sequence,
            Title = TruncateRequired(item.Title, "Draft item title is required.", 200),
            ProductId = item.ProductId,
            ProductName = NormalizeOptional(item.ProductName, 200),
            SupplierName = NormalizeOptional(item.SupplierName, 200),
            Quantity = item.Quantity,
            Notes = NormalizeOptional(item.Notes, 2000),
            Confidence = Clamp(item.Confidence),
            Reason = TruncateRequired(item.Reason, "Draft item reason is required.", 2000),
            IsUnresolved = item.IsUnresolved,
            WarningFlagsJson = Serialize(item.Warnings),
            MissingDataJson = Serialize(item.MissingData)
        }).ToList();

        await itineraryRepository.AddDraftAsync(draft, draftItems, cancellationToken);
        draft.Items = draftItems;
        return MapDraft(draft);
    }

    public async Task<ItineraryDraftApprovalResult> ApproveDraftAsync(Guid draftId, ApproveItineraryDraftRequest request, CancellationToken cancellationToken = default)
    {
        var draft = await itineraryRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Itinerary draft not found.");

        if (draft.Status == ItineraryDraftStatus.Approved && draft.PersistedItineraryId.HasValue)
        {
            throw new InvalidOperationException("Itinerary draft was already approved.");
        }

        var startDate = request.StartDate ?? draft.ProposedStartDate
            ?? throw new InvalidOperationException("A start date is required before approval.");
        var duration = request.Duration ?? draft.Duration;

        if (duration <= 0)
        {
            throw new InvalidOperationException("Duration must be greater than zero.");
        }

        var approvedItems = request.Items.Count > 0
            ? request.Items.Select(MapApprovedItem).ToList()
            : draft.Items
                .OrderBy(x => x.DayNumber)
                .ThenBy(x => x.Sequence)
                .Select(x => new CreateItineraryItemModel
                {
                    DayNumber = x.DayNumber,
                    ProductId = x.ProductId ?? Guid.Empty,
                    Quantity = x.Quantity,
                    Notes = x.Notes
                })
                .ToList();

        if (approvedItems.Count == 0)
        {
            throw new InvalidOperationException("At least one itinerary item is required for approval.");
        }

        if (approvedItems.Any(x => x.ProductId == Guid.Empty))
        {
            throw new InvalidOperationException("All approved itinerary items must reference real products.");
        }

        foreach (var item in approvedItems)
        {
            if (item.DayNumber <= 0 || item.DayNumber > duration)
            {
                throw new InvalidOperationException("Approved item day numbers must fall within the itinerary duration.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Approved item quantity must be greater than zero.");
            }
        }

        var approvedProducts = await productRepository.GetByIdsAsync(approvedItems.Select(x => x.ProductId), cancellationToken);
        var approvedProductIds = approvedProducts.Select(x => x.Id).ToHashSet();
        var missingProductIds = approvedItems
            .Select(x => x.ProductId)
            .Distinct()
            .Where(x => !approvedProductIds.Contains(x))
            .ToList();

        if (missingProductIds.Count > 0)
        {
            throw new InvalidOperationException("One or more approved itinerary products do not exist.");
        }

        var approvalPayload = Serialize(new
        {
            draftId = draft.Id,
            startDate,
            duration,
            items = approvedItems,
            request.DecisionNotes
        });

        var approvalRequest = await humanApprovalService.CreateApprovalRequestAsync(
            "ApproveAiItineraryDraft",
            nameof(ItineraryDraft),
            draft.Id,
            approvalPayload,
            cancellationToken);

        await humanApprovalService.ApproveActionAsync(
            approvalRequest.Id,
            NormalizeOptional(request.DecisionNotes, 4000) ?? "AI itinerary draft approved and persisted.",
            cancellationToken);

        var itinerary = await itineraryService.CreateItineraryAsync(new CreateItineraryModel
        {
            StartDate = startDate,
            Duration = duration,
            Items = approvedItems
        }, cancellationToken);

        var approvedAt = DateTime.UtcNow;
        draft.Status = ItineraryDraftStatus.Approved;
        draft.PersistedItineraryId = itinerary.Id;
        draft.ApprovedByUserId = currentUserContext.GetRequiredUserId();
        draft.ApprovedAt = approvedAt;
        draft.UpdatedAt = approvedAt;
        await itineraryRepository.UpdateDraftAsync(draft, cancellationToken);

        return new ItineraryDraftApprovalResult
        {
            DraftId = draft.Id,
            ApprovalRequestId = approvalRequest.Id,
            ApprovedAt = approvedAt,
            Itinerary = itinerary
        };
    }

    private async Task<IReadOnlyList<Product>> LoadCandidatesAsync(PlanningContext plan, IReadOnlyList<ProductType> productTypes, CancellationToken cancellationToken)
    {
        var products = await productRepository.SearchForItineraryPlanningAsync(new ProductCatalogFilter
        {
            LocationTerms = plan.LocationTerms,
            SearchTerms = plan.SearchTerms,
            ProductTypes = productTypes,
            MaxResults = 80
        }, cancellationToken);

        if (products.Count > 0)
        {
            return products;
        }

        return await productRepository.SearchForItineraryPlanningAsync(new ProductCatalogFilter
        {
            LocationTerms = [],
            SearchTerms = plan.SearchTerms,
            ProductTypes = productTypes,
            MaxResults = 80
        }, cancellationToken);
    }

    private static List<CandidateScore> ScoreCandidates(IReadOnlyList<Product> candidates, PlanningContext plan)
    {
        var scores = new List<CandidateScore>(candidates.Count);

        foreach (var product in candidates)
        {
            var warnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var assumptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var missingData = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var signals = new List<string>();
            var score = 0.08m;

            var productText = BuildProductText(product);
            var locationText = BuildLocationText(product);

            if (plan.LocationTerms.Count > 0)
            {
                var locationHits = plan.LocationTerms.Count(term => Contains(locationText, term));
                if (locationHits > 0)
                {
                    score += Math.Min(0.24m, locationHits * 0.12m);
                    signals.Add($"Location metadata matched {locationHits} destination signal(s).");
                }
                else
                {
                    warnings.Add("Destination match was weak in current catalog metadata.");
                    missingData.Add("More precise destination tags on products would improve ranking.");
                }
            }
            else
            {
                assumptions.Add("Destination details were limited, so location scoring stayed broad.");
            }

            if (plan.SearchTerms.Count > 0)
            {
                var searchHits = plan.SearchTerms.Count(term => Contains(productText, term));
                if (searchHits > 0)
                {
                    score += Math.Min(0.22m, searchHits * 0.05m);
                    signals.Add($"Product metadata matched {searchHits} brief signal(s).");
                }
                else
                {
                    warnings.Add("Brief details matched only weakly against current product metadata.");
                }
            }

            if (MatchesPreferredType(product.Type, plan))
            {
                score += 0.10m;
                signals.Add("Product type aligns with stated trip preferences.");
            }

            var relevantRates = GetRelevantRates(product, plan);
            var indicativeRate = relevantRates
                .OrderBy(x => x.BaseCost)
                .FirstOrDefault();

            if (relevantRates.Count > 0)
            {
                score += 0.18m;
                signals.Add("Active rate data exists for the requested timing.");
            }
            else if (product.Rates.Count > 0)
            {
                warnings.Add("No active rate matched the requested timing.");
                missingData.Add("Date-aligned rate availability.");
            }
            else
            {
                warnings.Add("No rate data is loaded for this product.");
                missingData.Add("Rate data.");
            }

            if (plan.TravellerCount.HasValue)
            {
                var travellerFit = relevantRates.Count > 0 && relevantRates.Any(x => RateSupportsTravellers(x, plan.TravellerCount.Value));
                if (travellerFit)
                {
                    score += 0.12m;
                    signals.Add("Traveller count fits rate constraints.");
                }
                else if (relevantRates.Count > 0)
                {
                    warnings.Add("Traveller count may not fit current rate constraints.");
                    missingData.Add("Clear pax and capacity metadata.");
                }
                else
                {
                    assumptions.Add("Traveller fit could not be validated without relevant rates.");
                }
            }

            if (!string.IsNullOrWhiteSpace(plan.AccommodationPreference))
            {
                if (Contains(productText, plan.AccommodationPreference))
                {
                    score += 0.08m;
                    signals.Add("Accommodation preference appears in product metadata.");
                }
                else if (product.Type == ProductType.Hotel)
                {
                    score += 0.04m;
                    assumptions.Add("Accommodation fit inferred from hotel product type.");
                }
            }

            if (!string.IsNullOrWhiteSpace(product.Supplier?.Name))
            {
                score += 0.04m;
            }

            if (string.IsNullOrWhiteSpace(product.PhysicalCountry) &&
                string.IsNullOrWhiteSpace(product.PhysicalTownOrCity) &&
                string.IsNullOrWhiteSpace(product.PhysicalStateOrProvince))
            {
                missingData.Add("Product location metadata.");
            }

            if (string.IsNullOrWhiteSpace(product.Inclusions) &&
                string.IsNullOrWhiteSpace(product.Specials) &&
                product.Extras.Count == 0)
            {
                missingData.Add("Descriptive product content.");
            }

            scores.Add(new CandidateScore(
                product,
                Clamp(score),
                signals,
                warnings.ToList(),
                assumptions.ToList(),
                missingData.ToList(),
                indicativeRate?.BaseCost,
                indicativeRate?.Currency));
        }

        return scores;
    }

    private static void ApplyBudgetSignal(List<CandidateScore> scores, PlanningContext plan)
    {
        var normalizedBudget = NormalizeBudget(plan.BudgetLevel);
        if (normalizedBudget is null)
        {
            return;
        }

        var comparableCurrency = ResolveComparableCurrency(scores, plan.PreferredCurrency);
        if (comparableCurrency is null)
        {
            foreach (var score in scores)
            {
                score.Warnings.Add("Budget fit could not be compared across mixed or missing currencies.");
                score.MissingData.Add("Comparable rate currency.");
            }

            return;
        }

        var comparableCosts = scores
            .Where(x => x.IndicativeCost.HasValue && string.Equals(x.IndicativeCurrency, comparableCurrency, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.IndicativeCost!.Value)
            .OrderBy(x => x)
            .ToList();

        if (comparableCosts.Count < 2)
        {
            foreach (var score in scores.Where(x => !x.IndicativeCost.HasValue))
            {
                score.Warnings.Add("Budget fit could not be checked because indicative rates are missing.");
                score.MissingData.Add("Indicative rate data.");
            }

            return;
        }

        var lowThreshold = comparableCosts[(int)Math.Floor((comparableCosts.Count - 1) * 0.33m)];
        var highThreshold = comparableCosts[(int)Math.Floor((comparableCosts.Count - 1) * 0.66m)];

        foreach (var score in scores)
        {
            if (!score.IndicativeCost.HasValue || !string.Equals(score.IndicativeCurrency, comparableCurrency, StringComparison.OrdinalIgnoreCase))
            {
                score.Warnings.Add("Budget fit could not be checked on this product.");
                score.MissingData.Add("Comparable budget rate.");
                continue;
            }

            var budgetBonus = normalizedBudget switch
            {
                "economy" when score.IndicativeCost.Value <= lowThreshold => 0.08m,
                "standard" when score.IndicativeCost.Value > lowThreshold && score.IndicativeCost.Value < highThreshold => 0.08m,
                "premium" when score.IndicativeCost.Value >= highThreshold => 0.08m,
                "luxury" when score.IndicativeCost.Value >= highThreshold => 0.10m,
                _ => 0.02m
            };

            score.DeterministicScore = Clamp(score.DeterministicScore + budgetBonus);
            score.Signals.Add($"Budget fit estimated using {comparableCurrency} rate data.");
        }
    }

    private async Task<List<LlmProductRecommendation>> RankCandidatesAsync(PlanningContext plan, IReadOnlyList<CandidateScore> shortlist, CancellationToken cancellationToken)
    {
        if (shortlist.Count == 0)
        {
            return [];
        }

        try
        {
            var contextJson = Serialize(new
            {
                plan.Destination,
                plan.Region,
                plan.StartDate,
                plan.EndDate,
                plan.Duration,
                plan.Season,
                plan.TravellerCount,
                plan.BudgetLevel,
                plan.PreferredCurrency,
                plan.TravelStyle,
                plan.Interests,
                plan.AccommodationPreference,
                plan.SpecialConstraints,
                plan.CustomerBrief,
                candidates = shortlist.Select(x => new
                {
                    productId = x.Product.Id,
                    productName = x.Product.Name,
                    supplierName = x.Product.Supplier.Name,
                    productType = x.Product.Type.ToString(),
                    deterministicScore = x.DeterministicScore,
                    reasons = x.Signals,
                    warnings = x.Warnings,
                    missingData = x.MissingData,
                    indicativeCost = x.IndicativeCost,
                    indicativeCurrency = x.IndicativeCurrency
                })
            });

            var result = await genericLlmService.GenerateStructuredAsync<List<LlmProductRecommendation>>(new LlmRequest
            {
                Category = "itinerary",
                Operation = "itinerary.product-assist",
                SystemInstruction = """
                    You rank travel products for itinerary planning.
                    Only use product ids from the supplied candidate list.
                    Never invent ids.
                    Return concise reasons.
                    Keep scores between 0 and 1.
                    Surface uncertainty and missing data.
                    """,
                Prompt = BuildProductAssistPrompt(plan, shortlist),
                Temperature = 0.1,
                MaxTokens = 1800,
                PreferStructuredOutput = true,
                Metadata = new Dictionary<string, string>
                {
                    ["contextJson"] = contextJson
                }
            }, cancellationToken);

            return result.Data
                .Where(x => x.ProductId != Guid.Empty)
                .GroupBy(x => x.ProductId)
                .Select(x => x.First() with
                {
                    MatchScore = Clamp(x.First().MatchScore)
                })
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private async Task<LlmDraftEnvelope?> GenerateDraftProposalAsync(PlanningContext plan, IReadOnlyList<CandidateScore> shortlist, CancellationToken cancellationToken)
    {
        if (shortlist.Count == 0)
        {
            return null;
        }

        try
        {
            var contextJson = Serialize(new
            {
                plan.Destination,
                plan.Region,
                plan.StartDate,
                plan.EndDate,
                plan.Duration,
                plan.Season,
                plan.TravellerCount,
                plan.BudgetLevel,
                plan.PreferredCurrency,
                plan.TravelStyle,
                plan.Interests,
                plan.AccommodationPreference,
                plan.SpecialConstraints,
                plan.CustomerBrief,
                candidates = shortlist.Select(x => new
                {
                    productId = x.Product.Id,
                    productName = x.Product.Name,
                    supplierName = x.Product.Supplier.Name,
                    productType = x.Product.Type.ToString(),
                    deterministicScore = x.DeterministicScore,
                    reasons = x.Signals,
                    warnings = x.Warnings,
                    missingData = x.MissingData
                })
            });

            var result = await genericLlmService.GenerateStructuredAsync<LlmDraftEnvelope>(new LlmRequest
            {
                Category = "itinerary",
                Operation = "itinerary.draft-generate",
                SystemInstruction = """
                    You build a draft itinerary for a travel operator.
                    Only use product ids from the supplied candidate list.
                    If no product fits, set productId to null and mark the item unresolved.
                    Keep outputs structured.
                    Each item must include a day number, sequence, title, quantity, reason, warnings, and missing data.
                    """,
                Prompt = BuildDraftPrompt(plan, shortlist),
                Temperature = 0.1,
                MaxTokens = 2200,
                PreferStructuredOutput = true,
                Metadata = new Dictionary<string, string>
                {
                    ["contextJson"] = contextJson
                }
            }, cancellationToken);

            result.Data.Provider = result.Provider;
            result.Data.Model = result.Model;
            return result.Data;
        }
        catch
        {
            return null;
        }
    }

    private static List<CandidateScore> BuildDraftShortlist(IEnumerable<CandidateScore> scores)
    {
        var ordered = scores
            .OrderByDescending(x => x.DeterministicScore)
            .ThenBy(x => x.Product.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var shortlist = ordered
            .Where(x => x.Product.Type == ProductType.Hotel)
            .Take(4)
            .Concat(ordered.Where(x => x.Product.Type == ProductType.Tour).Take(5))
            .Concat(ordered.Where(x => x.Product.Type == ProductType.Transport).Take(3))
            .GroupBy(x => x.Product.Id)
            .Select(x => x.First())
            .ToList();

        if (shortlist.Count < 10)
        {
            shortlist = shortlist
                .Concat(ordered)
                .GroupBy(x => x.Product.Id)
                .Select(x => x.First())
                .Take(12)
                .ToList();
        }

        return shortlist;
    }

    private static DraftContent MapDraftContent(PlanningContext plan, IReadOnlyList<CandidateScore> shortlist, LlmDraftEnvelope? aiDraft)
    {
        var candidateMap = shortlist.ToDictionary(x => x.Product.Id);
        var mappedItems = new List<DraftContentItem>();

        if (aiDraft?.Items is { Count: > 0 })
        {
            foreach (var item in aiDraft.Items)
            {
                var normalizedDay = item.DayNumber <= 0 ? 1 : item.DayNumber;
                var normalizedSequence = item.Sequence <= 0 ? normalizedDay : item.Sequence;
                var normalizedQuantity = item.Quantity <= 0 ? 1 : item.Quantity;

                if (item.ProductId.HasValue && candidateMap.TryGetValue(item.ProductId.Value, out var candidate))
                {
                    mappedItems.Add(new DraftContentItem(
                        normalizedDay,
                        normalizedSequence,
                        string.IsNullOrWhiteSpace(item.Title) ? candidate.Product.Name : item.Title.Trim(),
                        candidate.Product.Id,
                        candidate.Product.Name,
                        candidate.Product.Supplier.Name,
                        normalizedQuantity,
                        NormalizeOptional(item.Notes, 2000),
                        Clamp(item.Confidence),
                        string.IsNullOrWhiteSpace(item.Reason) ? BuildDeterministicReason(candidate) : item.Reason.Trim(),
                        false,
                        DistinctStrings(item.Warnings).ToList(),
                        DistinctStrings(item.MissingData).ToList()));
                }
                else
                {
                    mappedItems.Add(new DraftContentItem(
                        normalizedDay,
                        normalizedSequence,
                        string.IsNullOrWhiteSpace(item.Title) ? "Operator review required" : item.Title.Trim(),
                        null,
                        null,
                        null,
                        normalizedQuantity,
                        NormalizeOptional(item.Notes, 2000),
                        Clamp(item.Confidence),
                        string.IsNullOrWhiteSpace(item.Reason) ? "No safe product match was found in the current shortlist." : item.Reason.Trim(),
                        true,
                        DistinctStrings(item.Warnings.Append("Product mapping needs operator review.")).ToList(),
                        DistinctStrings(item.MissingData).ToList()));
                }
            }
        }

        if (mappedItems.Count == 0)
        {
            mappedItems = BuildDeterministicDraft(plan, shortlist);
        }

        mappedItems = mappedItems
            .Where(x => x.DayNumber > 0 && x.DayNumber <= plan.Duration)
            .OrderBy(x => x.DayNumber)
            .ThenBy(x => x.Sequence)
            .ToList();

        if (mappedItems.Count == 0)
        {
            throw new InvalidOperationException("Draft generation did not produce any usable itinerary items.");
        }

        var assumptions = DistinctStrings((aiDraft?.Assumptions ?? []).Concat(plan.Assumptions)).ToList();
        var caveats = DistinctStrings(aiDraft?.Caveats ?? []).ToList();
        var dataGaps = DistinctStrings(aiDraft?.DataGaps ?? []).ToList();

        if (mappedItems.Any(x => x.IsUnresolved))
        {
            dataGaps.Add("One or more itinerary steps still need product resolution.");
        }

        if (string.IsNullOrWhiteSpace(plan.CustomerBrief))
        {
            assumptions.Add("Draft used structured fields only because no free-text customer brief was supplied.");
        }

        return new DraftContent(
            assumptions,
            caveats,
            DistinctStrings(dataGaps).ToList(),
            mappedItems,
            aiDraft?.Provider,
            aiDraft?.Model);
    }

    private static List<DraftContentItem> BuildDeterministicDraft(PlanningContext plan, IReadOnlyList<CandidateScore> shortlist)
    {
        var result = new List<DraftContentItem>();
        var topHotel = shortlist.FirstOrDefault(x => x.Product.Type == ProductType.Hotel);
        var topTour = shortlist.FirstOrDefault(x => x.Product.Type == ProductType.Tour);
        var topTransport = shortlist.FirstOrDefault(x => x.Product.Type == ProductType.Transport);

        for (var day = 1; day <= plan.Duration; day++)
        {
            var sequence = 1;

            if (day == 1)
            {
                result.Add(BuildDraftItemFromCandidate(day, sequence++, topTransport, "Arrival transfer", true));
            }

            if (topHotel is not null)
            {
                result.Add(BuildDraftItemFromCandidate(day, sequence++, topHotel, day == 1 ? "Accommodation check-in" : "Accommodation stay", false));
            }
            else
            {
                result.Add(BuildUnresolvedDraftItem(day, sequence++, "Accommodation to confirm", "No hotel product matched strongly enough."));
            }

            if (topTour is not null && day < plan.Duration)
            {
                result.Add(BuildDraftItemFromCandidate(day, sequence++, topTour, $"Suggested activity for day {day}", false));
            }
            else if (day < plan.Duration)
            {
                result.Add(BuildUnresolvedDraftItem(day, sequence++, $"Activity for day {day}", "No tour product matched strongly enough."));
            }

            if (day == plan.Duration)
            {
                result.Add(BuildDraftItemFromCandidate(day, sequence, topTransport, "Departure transfer", true));
            }
        }

        return result;
    }

    private static DraftContentItem BuildDraftItemFromCandidate(int day, int sequence, CandidateScore? candidate, string title, bool allowPlaceholderWhenMissing)
    {
        if (candidate is null)
        {
            return allowPlaceholderWhenMissing
                ? BuildUnresolvedDraftItem(day, sequence, title, "No matching transport product was found.")
                : BuildUnresolvedDraftItem(day, sequence, title, "No matching product was found.");
        }

        return new DraftContentItem(
            day,
            sequence,
            title,
            candidate.Product.Id,
            candidate.Product.Name,
            candidate.Product.Supplier.Name,
            1,
            null,
            candidate.DeterministicScore,
            BuildDeterministicReason(candidate),
            false,
            candidate.Warnings.ToList(),
            candidate.MissingData.ToList());
    }

    private static DraftContentItem BuildUnresolvedDraftItem(int day, int sequence, string title, string reason) =>
        new(
            day,
            sequence,
            title,
            null,
            null,
            null,
            1,
            null,
            0.35m,
            reason,
            true,
            ["Operator review required before save."],
            ["Product resolution."]);

    private static List<ItineraryProductRecommendationModel> BuildRecommendationResponse(
        IReadOnlyList<CandidateScore> shortlist,
        IReadOnlyList<LlmProductRecommendation> aiRecommendations,
        int maxResults)
    {
        var shortlistMap = shortlist.ToDictionary(x => x.Product.Id);
        var merged = new List<ItineraryProductRecommendationModel>();
        var seen = new HashSet<Guid>();

        foreach (var aiRecommendation in aiRecommendations)
        {
            if (!shortlistMap.TryGetValue(aiRecommendation.ProductId, out var candidate) || !seen.Add(aiRecommendation.ProductId))
            {
                continue;
            }

            merged.Add(new ItineraryProductRecommendationModel
            {
                ProductId = candidate.Product.Id,
                ProductName = candidate.Product.Name,
                SupplierName = candidate.Product.Supplier.Name,
                ProductType = candidate.Product.Type,
                MatchScore = Clamp((candidate.DeterministicScore * 0.4m) + (Clamp(aiRecommendation.MatchScore) * 0.6m)),
                Reason = string.IsNullOrWhiteSpace(aiRecommendation.Reason) ? BuildDeterministicReason(candidate) : aiRecommendation.Reason.Trim(),
                Warnings = DistinctStrings(candidate.Warnings.Concat(aiRecommendation.Warnings)).ToList(),
                AssumptionFlags = DistinctStrings(candidate.AssumptionFlags.Concat(aiRecommendation.AssumptionFlags)).ToList(),
                MissingData = DistinctStrings(candidate.MissingData.Concat(aiRecommendation.MissingData)).ToList()
            });
        }

        foreach (var candidate in shortlist.Where(x => !seen.Contains(x.Product.Id)))
        {
            merged.Add(new ItineraryProductRecommendationModel
            {
                ProductId = candidate.Product.Id,
                ProductName = candidate.Product.Name,
                SupplierName = candidate.Product.Supplier.Name,
                ProductType = candidate.Product.Type,
                MatchScore = candidate.DeterministicScore,
                Reason = BuildDeterministicReason(candidate),
                Warnings = candidate.Warnings,
                AssumptionFlags = candidate.AssumptionFlags,
                MissingData = candidate.MissingData
            });
        }

        return merged
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .ToList();
    }

    private static PlanningContext BuildPlanningContext(ItineraryProductAssistRequest request)
    {
        var destination = NormalizeOptional(request.Destination, 256);
        var region = NormalizeOptional(request.Region, 256);
        var startDate = request.StartDate;
        var endDate = request.EndDate;

        if (startDate.HasValue ^ endDate.HasValue)
        {
            throw new InvalidOperationException("Start date and end date must be supplied together.");
        }

        if (startDate.HasValue && endDate < startDate)
        {
            throw new InvalidOperationException("End date must be on or after start date.");
        }

        if (request.TravellerCount.HasValue && request.TravellerCount.Value <= 0)
        {
            throw new InvalidOperationException("Traveller count must be greater than zero.");
        }

        var maxResults = request.MaxResults <= 0 ? 10 : Math.Min(request.MaxResults, 20);
        var assumptions = new List<string>();

        if (!startDate.HasValue)
        {
            assumptions.Add("No exact travel dates were supplied, so season fit used broader metadata.");
        }

        return new PlanningContext(
            destination,
            region,
            startDate,
            endDate,
            startDate.HasValue && endDate.HasValue ? endDate.Value.DayNumber - startDate.Value.DayNumber + 1 : 1,
            NormalizeOptional(request.Season, 128),
            request.TravellerCount,
            NormalizeOptional(request.BudgetLevel, 64),
            NormalizeOptional(request.PreferredCurrency, 8)?.ToUpperInvariant(),
            NormalizeOptional(request.TravelStyle, 128),
            DistinctStrings(request.Interests.Select(x => NormalizeOptional(x, 128))).ToList(),
            NormalizeOptional(request.AccommodationPreference, 128),
            DistinctStrings(request.SpecialConstraints.Select(x => NormalizeOptional(x, 256))).ToList(),
            NormalizeOptional(request.CustomerBrief, 4000),
            request.ProductTypes.Distinct().ToList(),
            maxResults,
            DistinctStrings(new[] { destination, region }).ToList(),
            BuildSearchTerms(destination, region, request.CustomerBrief, request.TravelStyle, request.AccommodationPreference, request.Interests, request.SpecialConstraints),
            assumptions);
    }

    private static PlanningContext BuildPlanningContext(GenerateItineraryDraftRequest request)
    {
        var destination = NormalizeOptional(request.Destination, 256);
        var region = NormalizeOptional(request.Region, 256);
        var startDate = request.StartDate;
        var endDate = request.EndDate;
        var duration = request.Duration;

        if (startDate.HasValue ^ endDate.HasValue)
        {
            throw new InvalidOperationException("Start date and end date must be supplied together.");
        }

        if (startDate.HasValue && endDate < startDate)
        {
            throw new InvalidOperationException("End date must be on or after start date.");
        }

        if (!duration.HasValue && startDate.HasValue && endDate.HasValue)
        {
            duration = endDate.Value.DayNumber - startDate.Value.DayNumber + 1;
        }

        if (!duration.HasValue || duration.Value <= 0)
        {
            throw new InvalidOperationException("A positive duration is required to generate a draft itinerary.");
        }

        if (request.TravellerCount.HasValue && request.TravellerCount.Value <= 0)
        {
            throw new InvalidOperationException("Traveller count must be greater than zero.");
        }

        var assumptions = new List<string>();
        if (!startDate.HasValue)
        {
            assumptions.Add("Draft day structure was generated without exact calendar dates.");
        }

        return new PlanningContext(
            destination,
            region,
            startDate,
            endDate,
            duration.Value,
            NormalizeOptional(request.Season, 128),
            request.TravellerCount,
            NormalizeOptional(request.BudgetLevel, 64),
            NormalizeOptional(request.PreferredCurrency, 8)?.ToUpperInvariant(),
            NormalizeOptional(request.TravelStyle, 128),
            DistinctStrings(request.Interests.Select(x => NormalizeOptional(x, 128))).ToList(),
            NormalizeOptional(request.AccommodationPreference, 128),
            DistinctStrings(request.SpecialConstraints.Select(x => NormalizeOptional(x, 256))).ToList(),
            NormalizeOptional(request.CustomerBrief, 4000),
            [],
            12,
            DistinctStrings(new[] { destination, region }).ToList(),
            BuildSearchTerms(destination, region, request.CustomerBrief, request.TravelStyle, request.AccommodationPreference, request.Interests, request.SpecialConstraints),
            assumptions);
    }

    private static IReadOnlyList<string> BuildSearchTerms(
        string? destination,
        string? region,
        string? customerBrief,
        string? travelStyle,
        string? accommodationPreference,
        IEnumerable<string> interests,
        IEnumerable<string> specialConstraints)
    {
        var phrases = new List<string?>();
        phrases.Add(destination);
        phrases.Add(region);
        phrases.Add(customerBrief);
        phrases.Add(travelStyle);
        phrases.Add(accommodationPreference);
        phrases.AddRange(interests);
        phrases.AddRange(specialConstraints);

        return phrases
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .SelectMany(x => x!.Split([' ', ',', ';', '.', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Append(x))
            .Select(x => NormalizeOptional(x, 128))
            .Where(x => !string.IsNullOrWhiteSpace(x) && x!.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(24)
            .ToList()!;
    }

    private static IReadOnlyList<Rate> GetRelevantRates(Product product, PlanningContext plan)
    {
        if (plan.StartDate.HasValue && plan.EndDate.HasValue)
        {
            return product.Rates
                .Where(x => x.IsActive && x.SeasonStart <= plan.EndDate.Value && x.SeasonEnd >= plan.StartDate.Value)
                .ToList();
        }

        return product.Rates.Where(x => x.IsActive).ToList();
    }

    private static bool RateSupportsTravellers(Rate rate, int travellerCount)
    {
        var minPaxValid = !rate.MinPax.HasValue || travellerCount >= rate.MinPax.Value;
        var maxPaxValid = !rate.MaxPax.HasValue || travellerCount <= rate.MaxPax.Value;
        var capacityValid = !rate.Capacity.HasValue || travellerCount <= rate.Capacity.Value;
        return minPaxValid && maxPaxValid && capacityValid;
    }

    private static bool MatchesPreferredType(ProductType type, PlanningContext plan)
    {
        if (plan.ProductTypes.Count > 0)
        {
            return plan.ProductTypes.Contains(type);
        }

        if (!string.IsNullOrWhiteSpace(plan.AccommodationPreference))
        {
            return type == ProductType.Hotel;
        }

        return false;
    }

    private static string? ResolveComparableCurrency(IEnumerable<CandidateScore> scores, string? preferredCurrency)
    {
        if (!string.IsNullOrWhiteSpace(preferredCurrency))
        {
            return preferredCurrency;
        }

        var currencies = scores
            .Where(x => !string.IsNullOrWhiteSpace(x.IndicativeCurrency))
            .GroupBy(x => x.IndicativeCurrency!, StringComparer.OrdinalIgnoreCase)
            .Select(x => new { Currency = x.Key, Count = x.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        if (currencies.Count == 1)
        {
            return currencies[0].Currency;
        }

        return currencies.Count > 0 && currencies[0].Count >= currencies.Sum(x => x.Count) * 0.6m
            ? currencies[0].Currency
            : null;
    }

    private static string BuildProductAssistPrompt(PlanningContext plan, IEnumerable<CandidateScore> shortlist)
    {
        var candidateLines = shortlist.Select(candidate =>
            $"- {candidate.Product.Id} | {candidate.Product.Name} | {candidate.Product.Type} | supplier: {candidate.Product.Supplier.Name} | score: {candidate.DeterministicScore:F2} | notes: {BuildDeterministicReason(candidate)}");

        return $"""
            Customer brief:
            destination: {plan.Destination ?? "n/a"}
            region: {plan.Region ?? "n/a"}
            dates: {FormatTripWindow(plan)}
            travellers: {plan.TravellerCount?.ToString() ?? "n/a"}
            budget: {plan.BudgetLevel ?? "n/a"}
            style: {plan.TravelStyle ?? "n/a"}
            accommodation: {plan.AccommodationPreference ?? "n/a"}
            interests: {FormatList(plan.Interests)}
            constraints: {FormatList(plan.SpecialConstraints)}
            freeText: {plan.CustomerBrief ?? "n/a"}

            Candidate products:
            {string.Join(Environment.NewLine, candidateLines)}
            """;
    }

    private static string BuildDraftPrompt(PlanningContext plan, IEnumerable<CandidateScore> shortlist)
    {
        var candidateLines = shortlist.Select(candidate =>
            $"- {candidate.Product.Id} | {candidate.Product.Name} | {candidate.Product.Type} | supplier: {candidate.Product.Supplier.Name} | score: {candidate.DeterministicScore:F2} | notes: {BuildDeterministicReason(candidate)}");

        return $"""
            Build a {plan.Duration}-day itinerary draft.
            destination: {plan.Destination ?? "n/a"}
            region: {plan.Region ?? "n/a"}
            dates: {FormatTripWindow(plan)}
            season: {plan.Season ?? "n/a"}
            travellers: {plan.TravellerCount?.ToString() ?? "n/a"}
            budget: {plan.BudgetLevel ?? "n/a"}
            style: {plan.TravelStyle ?? "n/a"}
            accommodation: {plan.AccommodationPreference ?? "n/a"}
            interests: {FormatList(plan.Interests)}
            constraints: {FormatList(plan.SpecialConstraints)}
            freeText: {plan.CustomerBrief ?? "n/a"}

            Use only these catalog products:
            {string.Join(Environment.NewLine, candidateLines)}
            """;
    }

    private static ItineraryDraftModel MapDraft(ItineraryDraft draft) => new()
    {
        Id = draft.Id,
        Status = draft.Status,
        ProposedStartDate = draft.ProposedStartDate,
        Duration = draft.Duration,
        CustomerBrief = draft.CustomerBrief,
        LlmProvider = draft.LlmProvider,
        LlmModel = draft.LlmModel,
        PersistedItineraryId = draft.PersistedItineraryId,
        CreatedAt = draft.CreatedAt,
        UpdatedAt = draft.UpdatedAt,
        ApprovedAt = draft.ApprovedAt,
        Assumptions = DeserializeStringList(draft.AssumptionsJson),
        Caveats = DeserializeStringList(draft.CaveatsJson),
        DataGaps = DeserializeStringList(draft.DataGapsJson),
        Items = draft.Items
            .OrderBy(x => x.DayNumber)
            .ThenBy(x => x.Sequence)
            .Select(item => new ItineraryDraftItemModel
            {
                Id = item.Id,
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
                Warnings = DeserializeStringList(item.WarningFlagsJson),
                MissingData = DeserializeStringList(item.MissingDataJson)
            })
            .ToList()
    };

    private static CreateItineraryItemModel MapApprovedItem(ApproveItineraryDraftItemModel item) => new()
    {
        DayNumber = item.DayNumber,
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        Notes = NormalizeOptional(item.Notes, 1000)
    };

    private static string BuildDeterministicReason(CandidateScore candidate)
    {
        if (candidate.Signals.Count == 0)
        {
            return "Ranked from deterministic product metadata with limited matching signals.";
        }

        return string.Join(" ", candidate.Signals.Take(3));
    }

    private static string BuildProductText(Product product) =>
        string.Join(
            " ",
            new[]
            {
                product.Name,
                product.Supplier.Name,
                product.Inclusions,
                product.Exclusions,
                product.Specials,
                product.RoomPolicies,
                product.RatePolicies,
                product.ChildPolicies,
                product.CancellationPolicies,
                product.PhysicalTownOrCity,
                product.PhysicalStateOrProvince,
                product.PhysicalCountry,
                string.Join(" ", product.Extras.Select(x => x.Description)),
                string.Join(" ", product.Rooms.Select(x => x.Name)),
                string.Join(" ", product.RateTypes.Select(x => x.Name)),
                string.Join(" ", product.RateBases.Select(x => x.Name)),
                string.Join(" ", product.MealBases.Select(x => x.Name)),
                string.Join(" ", product.ValidityPeriods.Select(x => x.Value))
            }.Where(x => !string.IsNullOrWhiteSpace(x)))
            .ToLowerInvariant();

    private static string BuildLocationText(Product product) =>
        string.Join(
            " ",
            new[]
            {
                product.PhysicalSuburb,
                product.PhysicalTownOrCity,
                product.PhysicalStateOrProvince,
                product.PhysicalCountry,
                product.Supplier.Name
            }.Where(x => !string.IsNullOrWhiteSpace(x)))
            .ToLowerInvariant();

    private static string FormatTripWindow(PlanningContext plan)
    {
        if (plan.StartDate.HasValue && plan.EndDate.HasValue)
        {
            return $"{plan.StartDate:yyyy-MM-dd} to {plan.EndDate:yyyy-MM-dd}";
        }

        return plan.Season ?? "n/a";
    }

    private static string FormatList(IEnumerable<string> values)
    {
        var list = DistinctStrings(values).ToList();
        return list.Count == 0 ? "n/a" : string.Join(", ", list);
    }

    private static string NormalizeBudget(string? value)
    {
        var normalized = NormalizeOptional(value, 64)?.ToLowerInvariant();
        return normalized switch
        {
            null => null!,
            "economy" or "budget" or "low" => "economy",
            "standard" or "mid" or "midrange" or "mid-range" => "standard",
            "premium" or "high" => "premium",
            "luxury" => "luxury",
            _ => "standard"
        };
    }

    private static IEnumerable<string> DistinctStrings(IEnumerable<string?> values) =>
        values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static List<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string TruncateRequired(string? value, string message, int maxLength)
    {
        var normalized = NormalizeOptional(value, maxLength);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);

    private static decimal Clamp(decimal value) => Math.Min(1m, Math.Max(0m, value));

    private sealed record PlanningContext(
        string? Destination,
        string? Region,
        DateOnly? StartDate,
        DateOnly? EndDate,
        int Duration,
        string? Season,
        int? TravellerCount,
        string? BudgetLevel,
        string? PreferredCurrency,
        string? TravelStyle,
        IReadOnlyList<string> Interests,
        string? AccommodationPreference,
        IReadOnlyList<string> SpecialConstraints,
        string? CustomerBrief,
        IReadOnlyList<ProductType> ProductTypes,
        int MaxResults,
        IReadOnlyList<string> LocationTerms,
        IReadOnlyList<string> SearchTerms,
        IReadOnlyList<string> Assumptions);

    private sealed class CandidateScore(
        Product product,
        decimal deterministicScore,
        List<string> signals,
        List<string> warnings,
        List<string> assumptionFlags,
        List<string> missingData,
        decimal? indicativeCost,
        string? indicativeCurrency)
    {
        public Product Product { get; } = product;
        public decimal DeterministicScore { get; set; } = deterministicScore;
        public List<string> Signals { get; } = signals;
        public List<string> Warnings { get; } = warnings;
        public List<string> AssumptionFlags { get; } = assumptionFlags;
        public List<string> MissingData { get; } = missingData;
        public decimal? IndicativeCost { get; } = indicativeCost;
        public string? IndicativeCurrency { get; } = indicativeCurrency;
    }

    private sealed record DraftContent(
        IReadOnlyList<string> Assumptions,
        IReadOnlyList<string> Caveats,
        IReadOnlyList<string> DataGaps,
        IReadOnlyList<DraftContentItem> Items,
        string? Provider,
        string? Model);

    private sealed record DraftContentItem(
        int DayNumber,
        int Sequence,
        string Title,
        Guid? ProductId,
        string? ProductName,
        string? SupplierName,
        int Quantity,
        string? Notes,
        decimal Confidence,
        string Reason,
        bool IsUnresolved,
        IReadOnlyList<string> Warnings,
        IReadOnlyList<string> MissingData);

    private sealed record LlmProductRecommendation(
        Guid ProductId,
        decimal MatchScore,
        string Reason,
        IReadOnlyList<string> Warnings,
        IReadOnlyList<string> AssumptionFlags,
        IReadOnlyList<string> MissingData);

    private sealed class LlmDraftEnvelope
    {
        public List<string> Assumptions { get; set; } = [];
        public List<string> Caveats { get; set; } = [];
        public List<string> DataGaps { get; set; } = [];
        public List<LlmDraftItem> Items { get; set; } = [];
        public string? Provider { get; set; }
        public string? Model { get; set; }
    }

    private sealed class LlmDraftItem
    {
        public int DayNumber { get; set; }
        public int Sequence { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public decimal Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = [];
        public List<string> MissingData { get; set; } = [];
    }
}
