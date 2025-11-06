namespace Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

public record class Payment
{
    public Guid Id { get; set; }

    public Guid LoanId { get; set; }

    public decimal AmountToPrinciple { get; set; }

    public decimal AmountToInterest { get; set; }

    public DateOnly EntryDate { get; set; }

    public DateTime CreatedOn { get; init; }
}