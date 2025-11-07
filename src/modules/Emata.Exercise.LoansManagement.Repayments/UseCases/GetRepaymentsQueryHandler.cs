using Emata.Exercise.LoansManagement.Contracts.Exceptions;
using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Shared;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Repayments.UseCases;

internal class GetRepaymentsQueryHandler : IQueryHandler<GetRepaymentsQuery, List<PaymentSummaryDTO>>
{
    private readonly PaymentsDbContext _dbContext;
    private readonly ILoanService _loansQueryService;
    private readonly ILoanCalculatorService _loanCalculator;

    public GetRepaymentsQueryHandler(PaymentsDbContext dbContext, ILoanService loansQueryService, ILoanCalculatorService loanCalculator)
    {
        _loansQueryService = loansQueryService;
        _dbContext = dbContext;
        _loanCalculator = loanCalculator;
    }

    public async Task<List<PaymentSummaryDTO>> Handle(GetRepaymentsQuery request, CancellationToken cancellationToken = default)
    {
        if (request.LoanId == null)
        {
            throw new Exception("Loan Id was not provided when retriving payment summaries.");
        }

        var loan = await _loansQueryService.GetLoanByIdAsync(request.LoanId.Value, cancellationToken) ?? 
            throw new LoansManagementNotFoundException($"Loan with Id {request.LoanId} not found when retriving payment summaries.");

        // Compute expected interest (simple interest formula)
        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = _loanCalculator.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        // Get all payments for this loan
        var allPayments = await _dbContext.Repayments
            .AsNoTracking()
            .Where(p => p.LoanId == request.LoanId.Value)
            .OrderBy(p => p.EntryDate)
            .Take(1000)
            .ToListAsync(cancellationToken);

        // Calculate cumulative values
        decimal totalPrincipalPaid = 0m;
        decimal totalInterestPaid = 0m;

        var summaries = new List<PaymentSummaryDTO>();

        foreach (var payment in allPayments)
        {
            totalPrincipalPaid += payment.AmountToPrinciple;
            totalInterestPaid += payment.AmountToInterest;

            var summary = _loanCalculator.GetPaymentSummary(
                payment.ToDTO(),
                loan,
                totalPrincipalPaid,
                totalInterestPaid,
                expectedInterest
            );

            summaries.Add(summary);
        }

        // Present most recent first
        return summaries.OrderByDescending(s => s.PaymentCreatedOn).ToList();
    }
}
