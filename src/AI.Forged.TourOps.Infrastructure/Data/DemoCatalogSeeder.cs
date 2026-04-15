using System.Text.Json;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AI.Forged.TourOps.Infrastructure.Data;

public class DemoCatalogSeeder(AppDbContext dbContext, ILogger<DemoCatalogSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var demoSuppliersExist = await dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(x => x.Email != null && x.Email.EndsWith("@demo.tourops.local"), cancellationToken);

        var demoCustomersExist = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(x => x.Email != null && x.Email.EndsWith("@demo.clients.tourops.local"), cancellationToken);

        var addedSupplierCount = 0;
        var addedProductCount = 0;
        var addedRateCount = 0;
        var addedCustomerCount = 0;

        if (!demoSuppliersExist)
        {
            var suppliers = BuildSuppliers();
            dbContext.Suppliers.AddRange(suppliers);
            await dbContext.SaveChangesAsync(cancellationToken);

            addedSupplierCount = suppliers.Count;
            addedProductCount = suppliers.Sum(x => x.Products.Count);
            addedRateCount = suppliers.Sum(x => x.Products.Sum(p => p.Rates.Count));
        }
        else
        {
            logger.LogInformation("Demo supplier catalog seed skipped. Demo suppliers already exist.");
        }

        if (!demoCustomersExist)
        {
            var customers = BuildCustomers();
            dbContext.Customers.AddRange(customers);
            await dbContext.SaveChangesAsync(cancellationToken);
            addedCustomerCount = customers.Count;
        }
        else
        {
            logger.LogInformation("Demo customer seed skipped. Demo customers already exist.");
        }

        logger.LogInformation(
            "Demo seed complete. Added {SupplierCount} suppliers, {ProductCount} products, {RateCount} rates, and {CustomerCount} customers.",
            addedSupplierCount,
            addedProductCount,
            addedRateCount,
            addedCustomerCount);
    }

    private static List<Customer> BuildCustomers()
    {
        var createdAt = DateTime.UtcNow;
        var customers = new List<Customer>();

        var blueprints = new[]
        {
            new CustomerBlueprint("Emma", "Harrington", "LeisureOneTime", "United Kingdom", "United Kingdom", "Email", "Economy", "Balanced", "Value", "Boutique beach stays", "Quiet room away from bars", "One-time anniversary traveller. Wants simple airport-to-resort flow with one premium experience.", new[] { "snorkelling", "sunset cruise" }, new[] { "seafood" }, new[] { "Nungwi", "Stone Town" }, new[] { "crowded nightlife" }, "Birthday trip"),
            new CustomerBlueprint("Luca", "Bianchi", "LeisureOneTime", "Italy", "Italy", "WhatsApp", "Premium", "Relaxed", "Luxury", "Ocean-view suites", "King bed", "Honeymoon enquiry. Strong preference for private dinners and spa add-ons.", new[] { "romance", "private dhow", "spa" }, new[] { "late check-out" }, new[] { "Kendwa", "Michamvi" }, new[] { "group tours" }, "Honeymoon"),
            new CustomerBlueprint("Megan", "Brooks", "LeisureOneTime", "United States", "United States", "Email", "Standard", "Balanced", "Balanced", "Family resort", "Interleading family suite", "Travelling with two children. Wants safe swimming beaches and soft adventure.", new[] { "family activities", "reef snorkel", "spice farm" }, new[] { "nut allergy" }, new[] { "Kiwengwa", "Jambiani" }, new[] { "long transfers" }, "School holiday"),
            new CustomerBlueprint("Ravi", "Mehta", "LeisureOneTime", "India", "United Arab Emirates", "Phone", "Luxury", "Fast", "Luxury", "High-end villa", "Private pool villa", "Short-stay VIP leisure stopover from Dubai. Prefers privacy and airport fast-track.", new[] { "private transfer", "fine dining", "stone town" }, new[] { "vegetarian meals" }, new[] { "Michamvi", "Stone Town" }, new[] { "shared boats" }, "Weekend escape"),
            new CustomerBlueprint("Sophie", "van der Merwe", "LeisureOneTime", "South Africa", "South Africa", "WhatsApp", "Premium", "Balanced", "Balanced", "Stylish beach resort", "Sea-facing room", "Girls trip lead traveller. Wants watersports mixed with one heritage day.", new[] { "kite surfing", "cocktail cruise", "stone town" }, new[] { "gluten-free" }, new[] { "Paje", "Stone Town" }, new[] { "family resorts" }, "Friends trip"),
            new CustomerBlueprint("Jonas", "Eriksen", "LeisureOneTime", "Norway", "Norway", "Email", "Standard", "Relaxed", "Value", "Quiet eco lodge", "Twin bed request", "Birding and reef photography focus. Likes smaller lodges and early departures.", new[] { "birding", "reef snorkel", "forest" }, new[] { "early breakfast" }, new[] { "Matemwe", "Kizimkazi" }, new[] { "all-inclusive" }, "Photography trip"),
            new CustomerBlueprint("Layla", "Haddad", "LeisureOneTime", "Jordan", "Qatar", "Email", "Premium", "Balanced", "Luxury", "Wellness retreat", "Suite close to spa", "Solo female traveller. Wants spa, yoga, and secure property with smooth transfers.", new[] { "wellness", "spa", "private guide" }, new[] { "women-led guide if possible" }, new[] { "Kendwa", "Jambiani" }, new[] { "late-night arrivals without meet-and-greet" }, "Reset trip"),
            new CustomerBlueprint("Daniel", "Moyo", "LeisureOneTime", "Zimbabwe", "Botswana", "Phone", "Economy", "Fast", "Value", "Value beach hotel", "No stairs if possible", "Short family extension after safari. Cost-conscious but wants a clean beach base.", new[] { "airport transfer", "day cruise" }, new[] { "ground floor room" }, new[] { "Uroa", "Nungwi" }, new[] { "festive supplements" }, "Safari extension"),
            new CustomerBlueprint("Alicia", "Romero", "LeisureOneTime", "Spain", "Spain", "Email", "Standard", "Balanced", "Balanced", "Historic hotel plus beach split", "Queen bed", "Wants 2 nights Stone Town and 3 nights beach. Loves food and culture.", new[] { "heritage", "spice farm", "walking tours" }, new[] { "sea-facing dining" }, new[] { "Stone Town", "Kendwa" }, new[] { "isolated properties" }, "Culture and coast"),
            new CustomerBlueprint("Tariq", "Rahman", "LeisureOneTime", "Bangladesh", "Kenya", "WhatsApp", "Premium", "Balanced", "Value", "Modern resort", "High-floor room", "Couple trip. Wants reliable Wi-Fi plus one marine safari and one private dinner.", new[] { "marine safari", "private dinner", "airport transfer" }, new[] { "halal dining" }, new[] { "Kizimkazi", "Nungwi" }, new[] { "party beaches" }, "Couple holiday"),

            new CustomerBlueprint("Isabelle", "Durand", "SeasonalRepeat", "France", "France", "Email", "Premium", "Relaxed", "Luxury", "Boutique resort", "Ocean-view room", "Seasonal repeat winter-sun client. Returns every January with close friends.", new[] { "snorkelling", "spa", "wine dinners" }, new[] { "January availability" }, new[] { "Kendwa", "Matemwe" }, new[] { "large family properties" }, "Winter sun"),
            new CustomerBlueprint("Noah", "Klein", "SeasonalRepeat", "Germany", "Germany", "WhatsApp", "Standard", "Balanced", "Balanced", "Diving base hotel", "Twin or double flexible", "Repeat dive season traveller. Prioritises house reef, boat logistics, and compressor reliability.", new[] { "diving", "reef house", "marine park" }, new[] { "extra gear rinse area" }, new[] { "Matemwe", "Kizimkazi" }, new[] { "city hotels" }, "Dive season"),
            new CustomerBlueprint("Priya", "Narayanan", "SeasonalRepeat", "India", "Singapore", "Email", "Luxury", "Relaxed", "Luxury", "Villa resort", "Private villa", "Annual Diwali family escape. Strong food requirements and nanny room requests.", new[] { "private villa", "family dining", "private transfer" }, new[] { "vegetarian kitchen support" }, new[] { "Jambiani", "Michamvi" }, new[] { "shared holiday apartments" }, "Festive family"),
            new CustomerBlueprint("Ethan", "Cooper", "SeasonalRepeat", "United States", "United States", "Phone", "Premium", "Fast", "Balanced", "Corporate-leisure hybrid resort", "Work-friendly suite", "Returns twice a year. Mixes remote work with 3-day leisure add-on.", new[] { "fast wifi", "private transfer", "sunset cruise" }, new[] { "desk in room" }, new[] { "Stone Town", "Nungwi" }, new[] { "slow ferry connections" }, "Work and unwind"),
            new CustomerBlueprint("Mila", "Petrovic", "SeasonalRepeat", "Serbia", "United Kingdom", "Email", "Standard", "Balanced", "Value", "Midscale resort", "Triple room option", "Brings different friends each shoulder season. Price-sensitive but loyal.", new[] { "beach break", "spice farm", "stone town" }, new[] { "good breakfast buffet" }, new[] { "Uroa", "Kiwengwa" }, new[] { "premium room-only offers" }, "Repeat shoulder season"),
            new CustomerBlueprint("Amina", "Omondi", "SeasonalRepeat", "Kenya", "Kenya", "WhatsApp", "Premium", "Balanced", "Balanced", "Beachfront resort", "Sea-facing room", "East Africa festive repeat guest. Likes easy access to beach bars but not loud rooms.", new[] { "sunset dhow", "shopping", "spa" }, new[] { "airport VIP meet and greet" }, new[] { "Paje", "Kendwa" }, new[] { "long inland drives" }, "Festive repeat"),
            new CustomerBlueprint("Oliver", "Bennett", "SeasonalRepeat", "United Kingdom", "Kenya", "Phone", "Luxury", "Relaxed", "Luxury", "Privacy-first villa", "Two-bedroom villa", "Hosts extended family each July. Needs villa inventory and flexible dining options.", new[] { "villa", "private chef", "private boat" }, new[] { "child-friendly pool fencing" }, new[] { "Jambiani", "Michamvi" }, new[] { "room-only city stays" }, "Family host"),
            new CustomerBlueprint("Farah", "Al Mansoori", "SeasonalRepeat", "United Arab Emirates", "United Arab Emirates", "WhatsApp", "Luxury", "Fast", "Luxury", "Premium resort", "Interleading suites", "Eid repeat client. Short, high-spend stay with airport fast-track and premium dining.", new[] { "VIP transfer", "spa", "private dining" }, new[] { "Arabic-speaking guide if available" }, new[] { "Nungwi", "Stone Town" }, new[] { "group shuttles" }, "Eid leisure"),

            new CustomerBlueprint("Helen", "Morrison", "BusinessRegular", "United Kingdom", "Kenya", "Email", "Premium", "Fast", "Balanced", "Executive hotel with meeting space", "Quiet king room", "EA for a Nairobi investment group. Books Zanzibar incentive stays for visiting investors.", new[] { "airport transfer", "executive dinners", "stone town" }, new[] { "late arrival handling" }, new[] { "Stone Town", "Michamvi" }, new[] { "shared arrivals" }, "Investor hosting"),
            new CustomerBlueprint("Ahmed", "Kassim", "BusinessRegular", "Tanzania", "Tanzania", "Phone", "Standard", "Fast", "Value", "Business-friendly beach hotel", "Twin rooms for teams", "Regional sales director. Often hosts East African distributors and international principals.", new[] { "meeting rooms", "group transfers", "team dinners" }, new[] { "invoice split by department" }, new[] { "Stone Town", "Uroa" }, new[] { "complex villa setups" }, "Quarterly partner hosting"),
            new CustomerBlueprint("Leila", "Santos", "BusinessRegular", "Portugal", "Mozambique", "Email", "Premium", "Balanced", "Balanced", "Executive beach retreat", "Sea-facing suite", "Hospitality consultant who entertains hotel owners and overseas asset managers.", new[] { "executive retreat", "private guide", "cocktail cruise" }, new[] { "vegetarian and pescatarian menus" }, new[] { "Kendwa", "Stone Town" }, new[] { "mass tourism feel" }, "Owner retreat"),
            new CustomerBlueprint("Greg", "Wilson", "BusinessRegular", "United States", "South Africa", "Phone", "Premium", "Fast", "Balanced", "Reliable conference-capable resort", "Suite with workspace", "Tech company travel lead. Brings mixed US and EU teams for short offsites.", new[] { "wifi", "airport transfer", "team excursion" }, new[] { "invoice reconciliation detail" }, new[] { "Kiwengwa", "Stone Town" }, new[] { "properties without backup power" }, "Team offsite"),
            new CustomerBlueprint("Njeri", "Kamau", "BusinessRegular", "Kenya", "United Arab Emirates", "WhatsApp", "Luxury", "Balanced", "Luxury", "Premium executive villa", "Villa with lounge area", "Family office host. Needs polished guest handling for international principals.", new[] { "vip meet and greet", "private transfer", "fine dining" }, new[] { "privacy, security, and flexible billing" }, new[] { "Michamvi", "Nungwi" }, new[] { "shared excursions" }, "Principal hosting"),
            new CustomerBlueprint("Samuel", "Okeke", "BusinessRegular", "Nigeria", "Ghana", "Email", "Standard", "Fast", "Value", "Well-run resort with boardroom access", "Executive room", "Oil and gas procurement lead. Uses Zanzibar for incentive extensions after Dar es Salaam meetings.", new[] { "boardroom", "airport shuttle", "cultural evening" }, new[] { "multiple arrivals same booking" }, new[] { "Stone Town", "Nungwi" }, new[] { "spa-centric packages" }, "Post-meeting extension"),
            new CustomerBlueprint("Chloe", "Everett", "BusinessRegular", "Australia", "Singapore", "Email", "Premium", "Balanced", "Balanced", "Executive retreat resort", "Suite with terrace", "APAC customer success lead. Often mixes work sessions with guest entertainment.", new[] { "sunset cruise", "private dining", "reef snorkel" }, new[] { "strong wifi and projector support" }, new[] { "Paje", "Stone Town" }, new[] { "transfer uncertainty" }, "Client retreat"),
            new CustomerBlueprint("Yusuf", "Dlamini", "BusinessRegular", "Eswatini", "South Africa", "Phone", "Premium", "Fast", "Balanced", "Beach resort near airport corridor", "Quiet executive room", "Mining services client. Hosts rotating international engineers and board guests.", new[] { "private transfer", "team dinner", "rest day resort" }, new[] { "early breakfast and airport timing precision" }, new[] { "Uroa", "Kizimkazi" }, new[] { "disconnective split stays" }, "Board and engineering visits")
        };

        foreach (var blueprint in blueprints)
        {
            customers.Add(BuildCustomer(blueprint, createdAt));
        }

        return customers;
    }

    private static List<Supplier> BuildSuppliers()
    {
        var blueprints = new[]
        {
            new SupplierBlueprint("Bahari Nungwi Collection", "Nungwi", "North Coast", "reef-front barefoot luxury", 142m, 46m, 58m, "HB", "bahari-nungwi"),
            new SupplierBlueprint("Kendwa Horizon Retreat", "Kendwa", "North Coast", "sunset beach resort", 155m, 52m, 62m, "BB", "kendwa-horizon"),
            new SupplierBlueprint("Matemwe Reef House", "Matemwe", "North-East Coast", "reef lodge and diving base", 168m, 55m, 68m, "BB", "matemwe-reef"),
            new SupplierBlueprint("Kiwengwa Palm Resort", "Kiwengwa", "East Coast", "family-friendly lagoon resort", 133m, 43m, 57m, "AI", "kiwengwa-palm"),
            new SupplierBlueprint("Paje Breeze Hideaway", "Paje", "South-East Coast", "kite beach retreat", 128m, 49m, 59m, "BB", "paje-breeze"),
            new SupplierBlueprint("Jambiani Tide Villas", "Jambiani", "South-East Coast", "villa-led ocean escape", 149m, 47m, 61m, "HB", "jambiani-tide"),
            new SupplierBlueprint("Michamvi Sunset Point", "Michamvi", "Chwaka Bay", "peninsula hideaway", 161m, 51m, 63m, "HB", "michamvi-sunset"),
            new SupplierBlueprint("Uroa Bay Gardens", "Uroa", "East Coast", "value-forward beach resort", 118m, 41m, 54m, "BB", "uroa-bay"),
            new SupplierBlueprint("Stone Town Spice Courtyard", "Stone Town", "Historic Centre", "heritage hotel and city experience house", 124m, 44m, 45m, "BB", "stone-town-spice"),
            new SupplierBlueprint("Kizimkazi Ocean Lodge & Safaris", "Kizimkazi", "South Coast", "marine safari lodge", 139m, 58m, 64m, "HB", "kizimkazi-ocean")
        };

        return blueprints.Select(BuildSupplier).ToList();
    }

    private static Supplier BuildSupplier(SupplierBlueprint blueprint)
    {
        var createdAt = DateTime.UtcNow;

        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = blueprint.Name,
            Email = $"reservations@{blueprint.EmailSlug}.demo.tourops.local",
            Phone = $"+255 24 {Random.Shared.Next(200, 899)} {Random.Shared.Next(100000, 999999)}",
            CreatedAt = createdAt,
            Products = BuildProducts(blueprint, createdAt)
        };
    }

    private static List<Product> BuildProducts(SupplierBlueprint blueprint, DateTime createdAt)
    {
        var products = new List<Product>();

        products.Add(BuildHotelProduct(blueprint, "Garden Room", "Garden Room", 2, blueprint.HotelBase, "King or twin bedding near landscaped courtyards.", createdAt, 1.00m));
        products.Add(BuildHotelProduct(blueprint, "Ocean View Room", "Ocean View Room", 2, blueprint.HotelBase, "Upper-floor room with a full Indian Ocean view.", createdAt, 1.15m));
        products.Add(BuildHotelProduct(blueprint, "Family Suite", "Family Suite", 4, blueprint.HotelBase, "Interleading suite ideal for small families.", createdAt, 1.32m));
        products.Add(BuildHotelProduct(blueprint, "Beachfront Villa", "Beachfront Villa", 4, blueprint.HotelBase, "Private villa a short walk from the sand.", createdAt, 1.58m));
        products.Add(BuildHotelProduct(blueprint, "Honeymoon Pool Suite", "Honeymoon Pool Suite", 2, blueprint.HotelBase, "Private plunge pool, turn-down, and romantic dinner setup.", createdAt, 1.74m));

        products.Add(BuildTourProduct(blueprint, "Reef Snorkel And Sandbank Day", blueprint.TourBase, 1.00m, "shared boat", 2, 12, createdAt));
        products.Add(BuildTourProduct(blueprint, "Sunset Dhow Cruise And Seafood Dinner", blueprint.TourBase, 1.18m, "shared dhow", 2, 18, createdAt));
        products.Add(BuildTourProduct(blueprint, "Jozani Forest, Spice Farm, And Stone Town", blueprint.TourBase, 1.26m, "full-day land excursion", 2, 14, createdAt));

        products.Add(BuildTransportProduct(blueprint, "Zanzibar Airport Private Transfer", blueprint.TransportBase, 1.00m, 5, createdAt));
        products.Add(BuildTransportProduct(blueprint, "Coast-To-Coast Resort Shuttle", blueprint.TransportBase, 1.24m, 8, createdAt));

        return products;
    }

    private static Product BuildHotelProduct(
        SupplierBlueprint blueprint,
        string name,
        string roomName,
        int capacity,
        decimal baseSto,
        string roomNarrative,
        DateTime createdAt,
        decimal multiplier)
    {
        var productId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var stoBase = decimal.Round(baseSto * multiplier, 2);

        return new Product
        {
            Id = productId,
            Name = $"{blueprint.Village} {name}",
            Type = ProductType.Hotel,
            ContractValidityPeriod = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1} seasonal contract",
            Commission = "12% standard trade commission guidance",
            PhysicalStreetAddress = $"{blueprint.Village} Beach Road",
            PhysicalSuburb = blueprint.Village,
            PhysicalTownOrCity = "Zanzibar",
            PhysicalStateOrProvince = "Unguja",
            PhysicalCountry = "Tanzania",
            PhysicalPostCode = "73107",
            MailingStreetAddress = $"{blueprint.Village} Beach Road",
            MailingSuburb = blueprint.Village,
            MailingTownOrCity = "Zanzibar",
            MailingStateOrProvince = "Unguja",
            MailingCountry = "Tanzania",
            MailingPostCode = "73107",
            CheckInTime = "14:00",
            CheckOutTime = "10:00",
            BlockOutDates = "24 Dec - 02 Jan subject to festive supplements",
            TourismLevyAmount = "5",
            TourismLevyCurrency = "USD",
            TourismLevyUnit = "Per person per night",
            TourismLevyAgeApplicability = "12 years and older",
            TourismLevyEffectiveDates = "Year-round",
            TourismLevyConditions = "Collected locally unless confirmed as prepaid.",
            TourismLevyRawText = "Government tourism levy may be collected locally.",
            TourismLevyIncluded = false,
            RoomPolicies = "Quoted rooms are subject to allocation at check-in. Adjacent rooms on request only.",
            RatePolicies = "Net STO rates loaded. Rack and promo guidance retained in variation notes for demos.",
            ChildPolicies = "Children under 6 sharing with adults stay free on accommodation only. Meal supplements apply.",
            CancellationPolicies = "Low season 14 days. High season 30 days. Festive 45 days.",
            Inclusions = $"{blueprint.Theme}; breakfast and afternoon tea included; Wi-Fi; bottled water; room service setup.",
            Exclusions = "Tourism levy, premium beverages, laundry, spa treatments, private guide fees.",
            Specials = "Early Bird 12% rack reduction at 60+ days | Green Season Stay 5 Pay 4 | Honeymoon flowers, bubbly, and one private dinner",
            CreatedAt = createdAt,
            Contacts =
            {
                new ProductContact
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ContactType = "Reservations",
                    ContactName = $"{blueprint.Village} Reservations Desk",
                    ContactEmail = $"res@{blueprint.EmailSlug}.demo.tourops.local",
                    ContactPhoneNumber = $"+255 24 {Random.Shared.Next(200, 899)} {Random.Shared.Next(100000, 999999)}"
                }
            },
            Extras =
            {
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "Half-board supplement",
                    ChargeUnit = "Per person per night",
                    Charge = $"{decimal.Round(22m * multiplier, 2)} USD"
                },
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "Private romantic dinner setup",
                    ChargeUnit = "Per setup",
                    Charge = $"{decimal.Round(85m * multiplier, 2)} USD"
                }
            },
            Rooms =
            {
                new ProductRoom
                {
                    Id = roomId,
                    ProductId = productId,
                    Name = roomName,
                    MinimumOccupancy = "1",
                    MaximumOccupancy = capacity.ToString(),
                    AdditionalNotes = roomNarrative,
                    RateConditions = "Net STO seasonal rates. Rack guidance stored on rate variation."
                }
            },
            RateTypes =
            {
                new ProductRateType { Id = Guid.NewGuid(), ProductId = productId, Name = "Contracted STO" }
            },
            RateBases =
            {
                new ProductRateBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "Per Room Per Night" }
            },
            MealBases =
            {
                new ProductMealBasis { Id = Guid.NewGuid(), ProductId = productId, Name = blueprint.MealBasis },
                new ProductMealBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "HB" },
                new ProductMealBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "AI" }
            },
            ValidityPeriods =
            {
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Green" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Shoulder" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "High" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Festive" }
            },
            Rates = BuildSeasonalRates(
                productId,
                roomId,
                PricingModel.PerUnit,
                stoBase,
                1,
                capacity,
                "Per Room Per Night",
                capacity <= 2 ? "Double/Twin" : "Family",
                blueprint.MealBasis,
                "2 nights")
        };
    }

    private static Product BuildTourProduct(
        SupplierBlueprint blueprint,
        string name,
        decimal baseSto,
        decimal multiplier,
        string basis,
        int minPax,
        int maxPax,
        DateTime createdAt)
    {
        var productId = Guid.NewGuid();
        var stoBase = decimal.Round(baseSto * multiplier, 2);

        return new Product
        {
            Id = productId,
            Name = $"{blueprint.Village} {name}",
            Type = ProductType.Tour,
            ContractValidityPeriod = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1} excursion contract",
            Commission = "15% retail markup headroom on loaded STO cost",
            PhysicalStreetAddress = $"{blueprint.Village} Jetty Road",
            PhysicalSuburb = blueprint.Village,
            PhysicalTownOrCity = "Zanzibar",
            PhysicalStateOrProvince = "Unguja",
            PhysicalCountry = "Tanzania",
            PhysicalPostCode = "73107",
            MailingStreetAddress = $"{blueprint.Village} Jetty Road",
            MailingSuburb = blueprint.Village,
            MailingTownOrCity = "Zanzibar",
            MailingStateOrProvince = "Unguja",
            MailingCountry = "Tanzania",
            MailingPostCode = "73107",
            CheckInTime = "08:00",
            CheckOutTime = "17:30",
            RoomPolicies = "N/A",
            RatePolicies = "STO net cost loaded. Variation notes include rack and promo guidance.",
            ChildPolicies = "Children 6-11 typically receive 25% discount unless marine permit fees apply.",
            CancellationPolicies = "24 hours for land tours. 48 hours for marine tours.",
            Inclusions = $"Guide, bottled water, permits where applicable, and {basis}.",
            Exclusions = "Premium drinks, marine park camera fees, gratuities, optional dive upgrades.",
            Specials = "Book with hotel stay for combo STO reduction | Private guide upgrade available | Shoulder season marine equipment credit",
            CreatedAt = createdAt,
            Contacts =
            {
                new ProductContact
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ContactType = "Operations",
                    ContactName = $"{blueprint.Village} Excursions Desk",
                    ContactEmail = $"ops@{blueprint.EmailSlug}.demo.tourops.local",
                    ContactPhoneNumber = $"+255 24 {Random.Shared.Next(200, 899)} {Random.Shared.Next(100000, 999999)}"
                }
            },
            Extras =
            {
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "Private guide upgrade",
                    ChargeUnit = "Per day",
                    Charge = $"{decimal.Round(78m * multiplier, 2)} USD"
                },
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "Premium seafood lunch",
                    ChargeUnit = "Per person",
                    Charge = $"{decimal.Round(24m * multiplier, 2)} USD"
                }
            },
            RateTypes =
            {
                new ProductRateType { Id = Guid.NewGuid(), ProductId = productId, Name = "Contracted STO" }
            },
            RateBases =
            {
                new ProductRateBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "Per Person" }
            },
            MealBases =
            {
                new ProductMealBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "N/A" }
            },
            ValidityPeriods =
            {
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Green" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Shoulder" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "High" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Festive" }
            },
            Rates = BuildSeasonalRates(
                productId,
                null,
                PricingModel.PerPerson,
                stoBase,
                minPax,
                maxPax,
                "Per Person",
                basis,
                "N/A",
                "1 day",
                0.25m)
        };
    }

    private static Product BuildTransportProduct(
        SupplierBlueprint blueprint,
        string name,
        decimal baseSto,
        decimal multiplier,
        int vehicleCapacity,
        DateTime createdAt)
    {
        var productId = Guid.NewGuid();
        var stoBase = decimal.Round(baseSto * multiplier, 2);

        return new Product
        {
            Id = productId,
            Name = $"{blueprint.Village} {name}",
            Type = ProductType.Transport,
            ContractValidityPeriod = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1} transport contract",
            Commission = "15% markup headroom from STO private transfer cost",
            PhysicalStreetAddress = $"{blueprint.Village} Driver Base",
            PhysicalSuburb = blueprint.Village,
            PhysicalTownOrCity = "Zanzibar",
            PhysicalStateOrProvince = "Unguja",
            PhysicalCountry = "Tanzania",
            PhysicalPostCode = "73107",
            MailingStreetAddress = $"{blueprint.Village} Driver Base",
            MailingSuburb = blueprint.Village,
            MailingTownOrCity = "Zanzibar",
            MailingStateOrProvince = "Unguja",
            MailingCountry = "Tanzania",
            MailingPostCode = "73107",
            CheckInTime = "Flexible",
            CheckOutTime = "Flexible",
            RoomPolicies = "N/A",
            RatePolicies = "Private vehicle STO cost loaded. Variation notes include rack guide and shoulder promo guidance.",
            ChildPolicies = "Child seats on request and subject to reconfirmation.",
            CancellationPolicies = "12 hours prior for standard transfers. 24 hours for premium or late-night transfers.",
            Inclusions = $"Driver, meet-and-greet, luggage handling, and one bottle of water per guest. Vehicle capacity {vehicleCapacity}.",
            Exclusions = "Extra stops, waiting beyond 30 minutes, ferry tickets, and guide service.",
            Specials = "Arrival combo discount with hotel stay | Return transfer bundle saving | Shoulder season complimentary cooler box on long routes",
            CreatedAt = createdAt,
            Contacts =
            {
                new ProductContact
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ContactType = "Dispatch",
                    ContactName = $"{blueprint.Village} Dispatch Team",
                    ContactEmail = $"dispatch@{blueprint.EmailSlug}.demo.tourops.local",
                    ContactPhoneNumber = $"+255 24 {Random.Shared.Next(200, 899)} {Random.Shared.Next(100000, 999999)}"
                }
            },
            Extras =
            {
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "Child seat",
                    ChargeUnit = "Per seat",
                    Charge = "8 USD"
                },
                new ProductExtra
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Description = "VIP fast-track airport assistance",
                    ChargeUnit = "Per arrival",
                    Charge = "45 USD"
                }
            },
            RateTypes =
            {
                new ProductRateType { Id = Guid.NewGuid(), ProductId = productId, Name = "Contracted STO" }
            },
            RateBases =
            {
                new ProductRateBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "Per Vehicle" }
            },
            MealBases =
            {
                new ProductMealBasis { Id = Guid.NewGuid(), ProductId = productId, Name = "N/A" }
            },
            ValidityPeriods =
            {
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Green" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Shoulder" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "High" },
                new ProductValidityPeriod { Id = Guid.NewGuid(), ProductId = productId, Value = "Festive" }
            },
            Rates = BuildSeasonalRates(
                productId,
                null,
                PricingModel.PerUnit,
                stoBase,
                1,
                null,
                "Per Vehicle",
                "Private transfer",
                "N/A",
                "N/A",
                null,
                vehicleCapacity)
        };
    }

    private static List<Rate> BuildSeasonalRates(
        Guid productId,
        Guid? roomId,
        PricingModel pricingModel,
        decimal stoBase,
        int? minPax,
        int? maxPax,
        string rateBasis,
        string occupancyType,
        string mealBasis,
        string minimumStay,
        decimal? childDiscount = null,
        int? capacity = null)
    {
        var currentYear = DateTime.UtcNow.Year;
        var seasons = new[]
        {
            new SeasonDefinition("Green", new DateOnly(currentYear, 1, 6), new DateOnly(currentYear, 3, 31), 0.92m, "Stay 5 Pay 4"),
            new SeasonDefinition("Shoulder", new DateOnly(currentYear, 4, 1), new DateOnly(currentYear, 6, 30), 1.00m, "Early bird 12%"),
            new SeasonDefinition("High", new DateOnly(currentYear, 7, 1), new DateOnly(currentYear, 10, 31), 1.18m, "Long stay resort credit"),
            new SeasonDefinition("Festive", new DateOnly(currentYear, 11, 1), new DateOnly(currentYear, 12, 31), 1.28m, "Festive value add")
        };

        var rates = new List<Rate>();

        foreach (var season in seasons)
        {
            var rack = decimal.Round(stoBase * season.Multiplier * 1.36m, 2);
            var sto = decimal.Round(stoBase * season.Multiplier, 2);
            var promo = decimal.Round(sto * 0.92m, 2);

            rates.Add(new Rate
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductRoomId = roomId,
                SeasonStart = season.Start,
                SeasonEnd = season.End,
                PricingModel = pricingModel,
                BaseCost = sto,
                Currency = "USD",
                MinPax = minPax,
                MaxPax = maxPax,
                ChildDiscount = childDiscount,
                SingleSupplement = pricingModel == PricingModel.PerUnit ? decimal.Round(sto * 0.35m, 2) : null,
                Capacity = capacity,
                ValidityPeriod = season.Name,
                ValidityPeriodDescription = $"{season.Name} seasonal STO cost. Rack guide {rack:0.00} USD. Promo guide {promo:0.00} USD.",
                RateVariation = $"Rack {rack:0.00} USD | STO {sto:0.00} USD | Promo {promo:0.00} USD ({season.PromoLabel})",
                RateTypeName = "Contracted STO",
                RateBasis = rateBasis,
                OccupancyType = occupancyType,
                MealBasis = mealBasis,
                MinimumStay = minimumStay,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        return rates;
    }

    private static Customer BuildCustomer(CustomerBlueprint blueprint, DateTime createdAt)
    {
        var customerId = Guid.NewGuid();
        var passportNumber = $"{blueprint.Segment[..1].ToUpperInvariant()}{blueprint.LastName[..2].ToUpperInvariant()}{Random.Shared.Next(100000, 999999)}";
        var isBusiness = blueprint.Segment == "BusinessRegular";
        var isSeasonal = blueprint.Segment == "SeasonalRepeat";

        return new Customer
        {
            Id = customerId,
            FirstName = blueprint.FirstName,
            LastName = blueprint.LastName,
            Email = $"{blueprint.FirstName.ToLowerInvariant()}.{blueprint.LastName.ToLowerInvariant()}@demo.clients.tourops.local",
            Phone = $"+{GetCountryDialCode(blueprint.CountryOfResidence)} {Random.Shared.Next(10, 99)} {Random.Shared.Next(100, 999)} {Random.Shared.Next(1000, 9999)}",
            Nationality = blueprint.Nationality,
            CountryOfResidence = blueprint.CountryOfResidence,
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-Random.Shared.Next(31, 58)).AddDays(Random.Shared.Next(0, 365))),
            PreferredContactMethod = ParseContactMethod(blueprint.PreferredContactMethod),
            Notes = $"{blueprint.Notes} Segment: {blueprint.Segment}.",
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            KycProfile = new CustomerKycProfile
            {
                CustomerId = customerId,
                PassportNumber = passportNumber,
                DocumentReference = $"{blueprint.LastName[..Math.Min(3, blueprint.LastName.Length)].ToUpperInvariant()}-{Random.Shared.Next(10000, 99999)}",
                PassportExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(Random.Shared.Next(2, 8))),
                IssuingCountry = blueprint.Nationality,
                VisaNotes = isBusiness
                    ? "Often requires invitation-letter support, airport fast-track, and VAT-ready invoicing."
                    : isSeasonal
                        ? "Usually travels visa-ready but requests expiry reminders before repeat season."
                        : "Progressive capture only. Verify visa need when routing via mainland Tanzania.",
                EmergencyContactName = $"Emergency {blueprint.LastName}",
                EmergencyContactPhone = $"+{GetCountryDialCode(blueprint.Nationality)} {Random.Shared.Next(10, 99)} {Random.Shared.Next(100, 999)} {Random.Shared.Next(1000, 9999)}",
                EmergencyContactRelationship = isBusiness ? "Colleague / travel coordinator" : "Spouse / sibling",
                VerificationStatus = isBusiness ? CustomerVerificationStatus.Verified : isSeasonal ? CustomerVerificationStatus.Pending : CustomerVerificationStatus.NotStarted,
                VerificationNotes = isBusiness
                    ? "Corporate repeat traveller. Passport and billing identity checked for hosted stays."
                    : isSeasonal
                        ? "Partial KYC on file from prior trips. Needs fresh passport expiry check."
                        : "Basic identity captured at enquiry stage only.",
                ProfileDataConsentGranted = true,
                KycDataConsentGranted = isBusiness || isSeasonal,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            PreferenceProfile = new CustomerPreferenceProfile
            {
                CustomerId = customerId,
                BudgetBand = ParseBudgetBand(blueprint.BudgetBand),
                AccommodationPreference = blueprint.AccommodationPreference,
                RoomPreference = blueprint.RoomPreference,
                DietaryRequirementsJson = SerializeList(blueprint.DietaryRequirements),
                ActivityPreferencesJson = SerializeList(blueprint.ActivityPreferences),
                AccessibilityRequirementsJson = SerializeList(blueprint.AccessibilityRequirements),
                PaceOfTravel = ParseTravelPace(blueprint.Pace),
                ValueLeaning = ParseValueLeaning(blueprint.ValueLeaning),
                TransportPreferencesJson = SerializeList(isBusiness
                    ? ["private SUV transfer", "timed airport meet-and-greet", "return shuttle support"]
                    : ["private airport transfer", "short coastal hops"]),
                SpecialOccasionsJson = SerializeList([blueprint.SpecialOccasion]),
                DislikedExperiencesJson = SerializeList(blueprint.AvoidedExperiences),
                PreferredDestinationsJson = SerializeList(blueprint.PreferredDestinations),
                AvoidedDestinationsJson = SerializeList(blueprint.AvoidedExperiences),
                OperatorNotes = isBusiness
                    ? "Use for hosted guest programs, executive dinners, and invoice-ready itinerary versions."
                    : isSeasonal
                        ? "Repeat pattern client. Reuse prior stay logic, then refresh seasonal rates and promos."
                        : "New / occasional client. Keep recommendations curated and easy to approve.",
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            }
        };
    }

    private static string SerializeList(IEnumerable<string> values) =>
        JsonSerializer.Serialize(values.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());

    private static PreferredContactMethod ParseContactMethod(string value) => value switch
    {
        "Phone" => PreferredContactMethod.Phone,
        "WhatsApp" => PreferredContactMethod.WhatsApp,
        "Any" => PreferredContactMethod.Any,
        _ => PreferredContactMethod.Email
    };

    private static CustomerBudgetBand ParseBudgetBand(string value) => value switch
    {
        "Economy" => CustomerBudgetBand.Economy,
        "Standard" => CustomerBudgetBand.Standard,
        "Premium" => CustomerBudgetBand.Premium,
        "Luxury" => CustomerBudgetBand.Luxury,
        _ => CustomerBudgetBand.Unknown
    };

    private static TravelPace ParseTravelPace(string value) => value switch
    {
        "Relaxed" => TravelPace.Relaxed,
        "Fast" => TravelPace.Fast,
        "Balanced" => TravelPace.Balanced,
        _ => TravelPace.Unknown
    };

    private static TravelValueLeaning ParseValueLeaning(string value) => value switch
    {
        "Value" => TravelValueLeaning.Value,
        "Luxury" => TravelValueLeaning.Luxury,
        "Balanced" => TravelValueLeaning.Balanced,
        _ => TravelValueLeaning.Unknown
    };

    private static string GetCountryDialCode(string country) => country switch
    {
        "United Kingdom" => "44",
        "Italy" => "39",
        "United States" => "1",
        "India" => "91",
        "South Africa" => "27",
        "Norway" => "47",
        "Jordan" => "962",
        "Zimbabwe" => "263",
        "Spain" => "34",
        "Bangladesh" => "880",
        "France" => "33",
        "Germany" => "49",
        "Singapore" => "65",
        "Kenya" => "254",
        "United Arab Emirates" => "971",
        "Mozambique" => "258",
        "Portugal" => "351",
        "Tanzania" => "255",
        "Nigeria" => "234",
        "Ghana" => "233",
        "Australia" => "61",
        "Serbia" => "381",
        "Botswana" => "267",
        "Qatar" => "974",
        "Eswatini" => "268",
        _ => "255"
    };

    private sealed record SupplierBlueprint(
        string Name,
        string Village,
        string Coast,
        string Theme,
        decimal HotelBase,
        decimal TourBase,
        decimal TransportBase,
        string MealBasis,
        string EmailSlug);

    private sealed record SeasonDefinition(
        string Name,
        DateOnly Start,
        DateOnly End,
        decimal Multiplier,
        string PromoLabel);

    private sealed record CustomerBlueprint(
        string FirstName,
        string LastName,
        string Segment,
        string Nationality,
        string CountryOfResidence,
        string PreferredContactMethod,
        string BudgetBand,
        string Pace,
        string ValueLeaning,
        string AccommodationPreference,
        string RoomPreference,
        string Notes,
        IReadOnlyList<string> ActivityPreferences,
        IReadOnlyList<string> DietaryRequirements,
        IReadOnlyList<string> PreferredDestinations,
        IReadOnlyList<string> AvoidedExperiences,
        string SpecialOccasion)
    {
        public IReadOnlyList<string> AccessibilityRequirements =>
            Segment == "BusinessRegular"
                ? ["invoice-ready planning", "tight arrival timing"]
                : Segment == "SeasonalRepeat"
                    ? ["repeat-preference retention"]
                    : [];
    }
}
