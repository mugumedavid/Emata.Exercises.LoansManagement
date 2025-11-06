using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Loans.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Loans.UseCases
{
    internal class GetLoanByIdQueryHandler : IQueryHandler<GetLoanByIdQuery, LoanItem?>
    {
        private readonly LoansDbContext _dbContext;

        public GetLoanByIdQueryHandler(LoansDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LoanItem?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var loan = await _dbContext.Loans
                .FirstOrDefaultAsync(b => b.Id == request.LoanId, cancellationToken);

            return loan?.ToDTO();
        }
    }
}
