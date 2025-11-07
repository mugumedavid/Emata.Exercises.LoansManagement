using System;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Loans;

public interface ILoanService
{
    Task<LoanItem?> GetLoanByIdAsync(Guid loanId, CancellationToken cancellationToken = default);
}
