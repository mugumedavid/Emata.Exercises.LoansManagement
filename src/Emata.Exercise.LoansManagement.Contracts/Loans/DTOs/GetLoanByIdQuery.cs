namespace Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;

public record GetLoanByIdQuery(Guid LoanId) : IQuery<LoanItem?>;