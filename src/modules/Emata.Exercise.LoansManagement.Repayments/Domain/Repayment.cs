
namespace Emata.Exercise.LoansManagement.Repayments.Domain;

public class Repayment
{
    public Guid Id { get; set; }

    public Guid LoanId { get; set; }

    public decimal AmountToPrinciple { get; set; }

    public decimal AmountToInterest { get; set; }

    public DateOnly EntryDate { get; set; }

    public DateTime CreatedOn { get; private set; }

    public static Repayment Create(
        Guid loanId,
        decimal amountToPrinciple,
        decimal amountToInterest,
        DateOnly entryDate
        ) => new Repayment
        {
            Id = Guid.CreateVersion7(),
            LoanId = loanId,
            AmountToPrinciple = amountToPrinciple,
            AmountToInterest = amountToInterest,
            EntryDate = entryDate,
            CreatedOn = DateTime.UtcNow
        };
}
