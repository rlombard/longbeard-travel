namespace AI.Forged.TourOps.Domain.Entities;

public class ProductContact
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
