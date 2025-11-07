using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Shared;
using Emata.Exercise.LoansManagement.Loans.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Loans.UseCases;

internal class GetLoansQueryHandler : IQueryHandler<GetLoansQuery, List<LoanItemDetails>>
{
    private readonly LoansDbContext _dbContext;
    private readonly ILoanCalculatorService _loanCalculator;

    public GetLoansQueryHandler(LoansDbContext dbContext, ILoanCalculatorService loanCalculator)
    {
        _dbContext = dbContext;
        _loanCalculator = loanCalculator;
    }

    public async Task<List<LoanItemDetails>> Handle(GetLoansQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Loans.AsQueryable();
        if (request.BorrowerIds != null && request.BorrowerIds.Length != 0)
        {
            query = query.Where(loan => request.BorrowerIds.Contains(loan.BorrowerId));
        }

        if (request.MinLoanAmount.HasValue)
        {
            query = query.Where(loan => loan.LoanAmount >= request.MinLoanAmount.Value);
        }

        if (request.MaxLoanAmount.HasValue)
        {
            query = query.Where(loan => loan.LoanAmount <= request.MaxLoanAmount.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(loan => loan.IssueDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(loan => loan.IssueDate <= request.EndDate.Value);
        }

        var loanSummaries = new List<LoanItemDetails>();

        foreach (var loan in query)
        {
            var summary = await _loanCalculator.GetBalanceSummaryAsync(loan.ToDTO(), cancellationToken);
            loanSummaries.Add(summary);
        }

        return loanSummaries
            .OrderByDescending(l => l.IssueDate)
            .ToList();
    }
}
