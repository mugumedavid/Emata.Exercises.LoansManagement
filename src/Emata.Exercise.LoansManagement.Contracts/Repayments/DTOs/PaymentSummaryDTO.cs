namespace Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

public record PaymentSummaryDTO : BalanceSummary
{
    public Guid Id { get; init; }

    public decimal Principal { get; init; }

    public decimal Interest { get; init; }

    public decimal TotalPayment { get; init; }

    public DateOnly EntryDate { get; init; }

    public DateTime LoanCreatedOn { get; set; }

    public DateTime PaymentCreatedOn { get; set; }
}
