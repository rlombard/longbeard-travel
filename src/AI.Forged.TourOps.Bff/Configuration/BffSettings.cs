using System.ComponentModel.DataAnnotations;

namespace AI.Forged.TourOps.Bff.Configuration;

public sealed class BffSettings
{
    [Required]
    [Url]
    public string ApiBaseUrl { get; init; } = string.Empty;

    [Range(1, 300)]
    public int UpstreamTimeoutSeconds { get; init; } = 30;

    public IReadOnlyList<string> AllowedPathPrefixes { get; init; } =
    [
        "admin",
        "booking-items",
        "bookings",
        "customers",
        "email-integrations",
        "email-threads",
        "invoices",
        "itineraries",
        "platform",
        "products",
        "quotes",
        "rates",
        "session",
        "signup",
        "suppliers",
        "task-suggestions",
        "tasks"
    ];
}
