using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Repayments.UseCases;

internal class GetSingleRepaymentQueryHandler : IQueryHandler<GetSingleRepaymentQuery, PaymentSummaryDTO>
{
    private readonly PaymentsDbContext _dbContext;
    private readonly ILoanService _loansQueryService;
    private readonly ILoanCalculator _loanCalculator;

    public GetSingleRepaymentQueryHandler(PaymentsDbContext dbContext, ILoanService loansQueryService, ILoanCalculator loanCalculator)
    {
        _loansQueryService = loansQueryService;
        _dbContext = dbContext;
        _loanCalculator = loanCalculator;
    }

    public async Task<PaymentSummaryDTO> Handle(GetSingleRepaymentQuery request, CancellationToken cancellationToken = default)
    {
        if (request.PaymentId == null)
        {
            throw new Exception("Payment Id was not provided when retriving payment summaries.");
        }

        var thisPayment = await _dbContext.Repayments.FindAsync(request.PaymentId, cancellationToken) ?? 
            throw new Exception($"Payment with Id {request.PaymentId} not found");
        
        var loan = await _loansQueryService.GetLoanByIdAsync(thisPayment.LoanId, cancellationToken) ?? 
            throw new Exception($"Loan with Id {thisPayment.LoanId} not found");

        // Compute expected interest (simple interest formula)
        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = _loanCalculator.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        // Get all payments on this loan
        var allPayments = await _dbContext.Repayments
            .Where(p => p.LoanId == loan.Id)
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

            var summary = _loanCalculator.CreatePaymentSummary(
                payment,
                loan,
                totalPrincipalPaid,
                totalInterestPaid,
                expectedInterest
            );

            summaries.Add(summary);
        }

        return summaries.First(s => s.Id == request.PaymentId);
    }
}
