using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Loans;

public record GetLoansQuery : IQuery<List<LoanItemDetails>>
{
    public Guid[]? BorrowerIds { get; set; }
    public decimal? MinLoanAmount { get; set; }
    public decimal? MaxLoanAmount { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
