using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Invoices;
using AI.Forged.TourOps.Application.Services;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using Xunit;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Tests;

public class InvoiceServiceTests
{
    [Fact]
    public async Task IngestInvoiceAsync_MatchesSupplierAndBookingItem()
    {
        var fixture = new InvoiceFixture();
        var service = fixture.CreateService();

        var result = await service.IngestInvoiceAsync(new InvoiceIngestionRequestModel
        {
            SourceSystem = "AiForged",
            ExternalSourceReference = "inv-001",
            InvoiceNumber = "INV-001",
            SupplierId = fixture.Supplier.Id,
            BookingItemId = fixture.BookingItem.Id,
            InvoiceDate = new DateOnly(2026, 4, 14),
            DueDate = new DateOnly(2026, 4, 21),
            Currency = "USD",
            SubtotalAmount = 100m,
            TaxAmount = 15m,
            TotalAmount = 115m,
            ExtractionConfidence = 0.95m,
            LineItems =
            [
                new InvoiceLineItemInputModel
                {
                    Description = "Safari lodge stay",
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxAmount = 15m,
                    TotalAmount = 115m
                }
            ]
        });

        Assert.False(result.WasExisting);
        Assert.Equal(InvoiceStatus.Matched, result.FinalStatus);
        Assert.Equal(fixture.Supplier.Id, result.SupplierId);
        Assert.Equal(fixture.Booking.Id, result.BookingId);
        Assert.Equal(fixture.BookingItem.Id, result.BookingItemId);
        Assert.Empty(result.UnresolvedFields);
        Assert.Single(fixture.InvoiceRepository.Invoices);
    }

    [Fact]
    public async Task IngestInvoiceAsync_PendingReviewCreatesTaskWhenBookingExists()
    {
        var fixture = new InvoiceFixture();
        var service = fixture.CreateService();

        var result = await service.IngestInvoiceAsync(new InvoiceIngestionRequestModel
        {
            SourceSystem = "AiForged",
            ExternalSourceReference = "inv-002",
            InvoiceNumber = "INV-002",
            BookingId = fixture.Booking.Id,
            SupplierName = "Unknown Supplier",
            InvoiceDate = new DateOnly(2026, 4, 14),
            DueDate = new DateOnly(2026, 4, 18),
            Currency = "USD",
            SubtotalAmount = 50m,
            TaxAmount = 0m,
            TotalAmount = 50m,
            ExtractionConfidence = 0.6m
        });

        Assert.Equal(InvoiceStatus.PendingReview, result.FinalStatus);
        Assert.NotNull(result.ReviewTaskId);
        Assert.Single(fixture.TaskRepository.Tasks);
        Assert.Contains("SupplierName", result.UnresolvedFields);
    }

    [Fact]
    public async Task UpdateInvoiceStatusAsync_ApprovedCreatesApprovalRecord()
    {
        var fixture = new InvoiceFixture();
        var service = fixture.CreateService();
        var invoiceId = await fixture.CreateStoredInvoiceAsync(InvoiceStatus.Matched);

        var invoice = await service.UpdateInvoiceStatusAsync(invoiceId, new UpdateInvoiceStatusModel
        {
            Status = InvoiceStatus.Approved,
            Notes = "Ready for payment prep."
        });

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
        Assert.Single(fixture.HumanApprovalService.CreatedRequests);
        Assert.Equal(HumanApprovalStatus.Approved, fixture.HumanApprovalService.CreatedRequests[0].Status);
    }

    [Fact]
    public async Task RecordPaymentAsync_MovesInvoiceToPartialThenPaid()
    {
        var fixture = new InvoiceFixture();
        var service = fixture.CreateService();
        var invoiceId = await fixture.CreateStoredInvoiceAsync(InvoiceStatus.Unpaid, totalAmount: 120m);

        var partial = await service.RecordPaymentAsync(invoiceId, new RecordInvoicePaymentModel
        {
            Amount = 20m,
            Currency = "USD",
            PaidAt = DateTime.UtcNow,
            PaymentMethod = "Bank Transfer"
        });

        Assert.Equal(InvoiceStatus.PartiallyPaid, partial.Status);
        Assert.Equal(20m, partial.AmountPaid);
        Assert.Equal(100m, partial.OutstandingAmount);

        var paid = await service.RecordPaymentAsync(invoiceId, new RecordInvoicePaymentModel
        {
            Amount = 100m,
            Currency = "USD",
            PaidAt = DateTime.UtcNow,
            PaymentMethod = "Bank Transfer"
        });

        Assert.Equal(InvoiceStatus.Paid, paid.Status);
        Assert.Equal(120m, paid.AmountPaid);
        Assert.Equal(0m, paid.OutstandingAmount);
    }

    private sealed class InvoiceFixture
    {
        public Supplier Supplier { get; } = new()
        {
            Id = Guid.NewGuid(),
            Name = "Safari Lodge Supplier",
            CreatedAt = DateTime.UtcNow
        };

        public Booking Booking { get; }
        public BookingItem BookingItem { get; }
        public FakeInvoiceRepository InvoiceRepository { get; } = new();
        public FakeTaskRepository TaskRepository { get; } = new();
        public FakeHumanApprovalService HumanApprovalService { get; } = new();
        public FakeSupplierRepository SupplierRepository { get; }
        public FakeBookingRepository BookingRepository { get; }
        public FakeBookingItemRepository BookingItemRepository { get; }

        public InvoiceFixture()
        {
            Booking = new Booking
            {
                Id = Guid.NewGuid(),
                QuoteId = Guid.NewGuid(),
                Status = BookingStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            BookingItem = new BookingItem
            {
                Id = Guid.NewGuid(),
                BookingId = Booking.Id,
                ProductId = Guid.NewGuid(),
                SupplierId = Supplier.Id,
                Supplier = Supplier,
                Product = new Product
                {
                    Id = Guid.NewGuid(),
                    SupplierId = Supplier.Id,
                    Supplier = Supplier,
                    Name = "Safari Lodge",
                    Type = ProductType.Hotel,
                    CreatedAt = DateTime.UtcNow
                },
                Status = BookingItemStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            Booking.Items.Add(BookingItem);
            SupplierRepository = new FakeSupplierRepository([Supplier]);
            BookingRepository = new FakeBookingRepository([Booking]);
            BookingItemRepository = new FakeBookingItemRepository([BookingItem]);
        }

        public InvoiceService CreateService()
        {
            return new InvoiceService(
                InvoiceRepository,
                SupplierRepository,
                BookingRepository,
                BookingItemRepository,
                new FakeQuoteRepository(),
                new FakeEmailRepository(),
                TaskRepository,
                new FakeCurrentUserContext(),
                HumanApprovalService);
        }

        public async Task<Guid> CreateStoredInvoiceAsync(InvoiceStatus status, decimal totalAmount = 100m)
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                SourceSystem = "AiForged",
                ExternalSourceReference = $"stored-{Guid.NewGuid():N}",
                InvoiceNumber = $"INV-{Random.Shared.Next(100, 999)}",
                SupplierId = Supplier.Id,
                SupplierName = Supplier.Name,
                BookingId = Booking.Id,
                BookingItemId = BookingItem.Id,
                InvoiceDate = new DateOnly(2026, 4, 14),
                DueDate = new DateOnly(2026, 4, 21),
                Currency = "USD",
                SubtotalAmount = totalAmount,
                TaxAmount = 0m,
                TotalAmount = totalAmount,
                ExtractionConfidence = 0.95m,
                Status = status,
                ReceivedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Supplier = Supplier,
                Booking = Booking,
                BookingItem = BookingItem
            };

            await InvoiceRepository.AddAsync(invoice, [], [], CancellationToken.None);
            return invoice.Id;
        }
    }

    private sealed class FakeInvoiceRepository : IInvoiceRepository
    {
        public Dictionary<Guid, Invoice> Invoices { get; } = [];

        public Task<Invoice> AddAsync(Invoice invoice, IEnumerable<InvoiceLineItem> lineItems, IEnumerable<InvoiceAttachment> attachments, CancellationToken cancellationToken = default)
        {
            invoice.LineItems = lineItems.ToList();
            invoice.Attachments = attachments.ToList();
            Invoices[invoice.Id] = invoice;
            return Task.FromResult(invoice);
        }

        public Task<PaymentRecord> AddPaymentRecordAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default)
        {
            var invoice = Invoices[paymentRecord.InvoiceId];
            invoice.PaymentRecords.Add(paymentRecord);
            return Task.FromResult(paymentRecord);
        }

        public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Invoices.TryGetValue(id, out var invoice) ? invoice : null);

        public Task<Invoice?> GetBySourceAsync(string sourceSystem, string externalSourceReference, CancellationToken cancellationToken = default) =>
            Task.FromResult(Invoices.Values.FirstOrDefault(x => x.SourceSystem == sourceSystem && x.ExternalSourceReference == externalSourceReference));

        public Task<IReadOnlyList<Invoice>> GetInvoicesAsync(InvoiceListQueryModel query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Invoice>>(Invoices.Values.ToList());

        public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            Invoices[invoice.Id] = invoice;
            return Task.CompletedTask;
        }

        public Task UpdateStatusAsync(Guid id, InvoiceStatus status, string? notes, DateTime updatedAt, CancellationToken cancellationToken = default)
        {
            var invoice = Invoices[id];
            invoice.Status = status;
            invoice.Notes = notes;
            invoice.UpdatedAt = updatedAt;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSupplierRepository(List<Supplier> suppliers) : ISupplierRepository
    {
        public Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default) => Task.FromResult(supplier);
        public Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Supplier>>(suppliers);
        public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(suppliers.FirstOrDefault(x => x.Id == id));
        public Task<Supplier?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(suppliers.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
        public Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default) => Task.FromResult(supplier);
    }

    private sealed class FakeBookingRepository(List<Booking> bookings) : IBookingRepository
    {
        public Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default) => Task.FromResult(booking);
        public Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Booking>>(bookings);
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(bookings.FirstOrDefault(x => x.Id == id));
        public Task<Booking?> GetByQuoteIdAsync(Guid quoteId, CancellationToken cancellationToken = default) => Task.FromResult(bookings.FirstOrDefault(x => x.QuoteId == quoteId));
        public Task UpdateLeadCustomerAsync(Guid id, Guid? customerId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeBookingItemRepository(List<BookingItem> bookingItems) : IBookingItemRepository
    {
        public Task<BookingItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(bookingItems.FirstOrDefault(x => x.Id == id));
        public Task UpdateStatusAsync(Guid id, BookingItemStatus status, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateNotesAsync(Guid id, string? notes, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeQuoteRepository : IQuoteRepository
    {
        public Task<Quote> AddAsync(Quote quote, CancellationToken cancellationToken = default) => Task.FromResult(quote);
        public Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Quote?>(null);
        public Task<Quote?> GetByIdForBookingAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Quote?>(null);
        public Task UpdateLeadCustomerAsync(Guid quoteId, Guid? customerId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeEmailRepository : IEmailRepository
    {
        public Task<EmailThread> AddThreadAsync(EmailThread thread, CancellationToken cancellationToken = default) => Task.FromResult(thread);
        public Task<EmailMessage> AddMessageAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.FromResult(message);
        public Task<EmailDraft> AddDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default) => Task.FromResult(draft);
        public Task<EmailThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default) => Task.FromResult<EmailThread?>(null);
        public Task<EmailThread?> GetThreadByExternalThreadIdAsync(string externalThreadId, CancellationToken cancellationToken = default) => Task.FromResult<EmailThread?>(null);
        public Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmailThread>>([]);
        public Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmailThread>>([]);
        public Task<IReadOnlyList<EmailThread>> GetThreadsPendingAutomationAsync(int take, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmailThread>>([]);
        public Task<EmailMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default) => Task.FromResult<EmailMessage?>(null);
        public Task<EmailDraft?> GetDraftByIdAsync(Guid draftId, CancellationToken cancellationToken = default) => Task.FromResult<EmailDraft?>(null);
        public Task UpdateThreadAsync(EmailThread thread, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateMessageAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        public List<OperationalTask> Tasks { get; } = [];

        public Task<OperationalTask> AddAsync(OperationalTask task, CancellationToken cancellationToken = default)
        {
            Tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<OperationalTask>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OperationalTask>>(Tasks);
        public Task<IReadOnlyList<OperationalTask>> GetByAssignedUserAsync(string userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OperationalTask>>(Tasks.Where(x => x.AssignedToUserId == userId).ToList());
        public Task<IReadOnlyList<OperationalTask>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OperationalTask>>(Tasks.Where(x => x.BookingId == bookingId || x.BookingItem?.BookingId == bookingId).ToList());
        public Task<OperationalTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Tasks.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(OperationalTask task, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
            request.ReviewedByUserId = "operator-1";
            return Task.FromResult(request);
        }

        public Task<HumanApprovalRequest> RejectActionAsync(Guid approvalRequestId, string? decisionNotes, CancellationToken cancellationToken = default)
        {
            var request = CreatedRequests.First(x => x.Id == approvalRequestId);
            request.Status = HumanApprovalStatus.Rejected;
            request.DecisionNotes = decisionNotes;
            request.ReviewedByUserId = "operator-1";
            return Task.FromResult(request);
        }
    }
}
