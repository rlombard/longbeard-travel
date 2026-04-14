namespace AI.Forged.TourOps.Domain.Enums;

public enum EmailClassificationType
{
    ConfirmationReceived = 0,
    PartialConfirmation = 1,
    NeedsMoreInformation = 2,
    PricingChanged = 3,
    AvailabilityIssue = 4,
    NoActionNeeded = 5,
    HumanDecisionRequired = 6,
    Unclear = 7
}
