using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Repayments;

public record class AddPaymentCommand : ICommand<PaymentSummaryDTO>
{
    public Guid LoanId { get; set; }

    public decimal AmountToPrinciple { get; set; }

    public decimal AmountToInterest { get; set; }

    public DateOnly EntryDate { get; set; }
}