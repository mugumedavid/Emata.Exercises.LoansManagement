using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Shared;

namespace Emata.Exercise.LoansManagement.Loans.Presentation
{
    internal class LoanService(IQueryHandler<GetLoanByIdQuery, LoanItem?> getLoanByIdQueryHandler) : ILoanService
    {
        private readonly IQueryHandler<GetLoanByIdQuery, LoanItem?> _getLoanByIdQueryHandler = getLoanByIdQueryHandler;

        public Task<LoanItem?> GetLoanByIdAsync(Guid loanId, CancellationToken cancellationToken = default)
        {
            return _getLoanByIdQueryHandler.Handle(new GetLoanByIdQuery(loanId), cancellationToken);
        }
    }
}
