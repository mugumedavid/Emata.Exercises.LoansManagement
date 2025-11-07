using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Shared;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Repayments.Domain
{
    public class LoanCalculatorService : ILoanCalculatorService
    {
        private readonly PaymentsDbContext _dbContext;

        public LoanCalculatorService(PaymentsDbContext dbContext, ILoanService loansQueryService)
        {
            _dbContext = dbContext;
        }

        public decimal GetDurationInYears(DurationDto? duration)
        {
            if (duration is null)
            {
                throw new Exception($"Loan duration not available");
            }

            return duration.Period switch
            {
                Period.Annual => duration.Length,
                Period.Monthly => duration.Length / 12m,
                Period.Weekly => duration.Length / 52m,
                Period.Daily => duration.Length / 365m,
                _ => duration.Length
            };
        }

        public decimal CalculateExpectedInterest(decimal principal, decimal rate, DurationDto? duration)
        {
            decimal time = GetDurationInYears(duration);
            return principal * rate * time;
        }

        public PaymentSummaryDTO GetPaymentSummary(
            Payment payment, 
            LoanItem loan, 
            decimal totalPrincipalPaid, 
            decimal totalInterestPaid, 
            decimal expectedInterest)
        {
            decimal loanOutstanding = loan.LoanAmount - totalPrincipalPaid;
            decimal loanBalance = (loan.LoanAmount + expectedInterest) - (totalPrincipalPaid + totalInterestPaid);

            return new PaymentSummaryDTO
            {
                Id = payment.Id,
                Principal = payment.AmountToPrinciple,
                Interest = payment.AmountToInterest,
                TotalPayment = payment.AmountToPrinciple + payment.AmountToInterest,
                EntryDate = payment.EntryDate,
                LoanCreatedOn = loan.CreatedOn,
                PaymentCreatedOn = payment.CreatedOn,
                LoanBalance = loanBalance,
                LoanOutstanding = loanOutstanding,
                TotalInterestReceived = totalInterestPaid
            };
        }

        public async Task<LoanItemDetails> GetBalanceSummaryAsync(LoanItem loan, CancellationToken cancellationToken)
        {
            var totals = await _dbContext.Repayments
                .AsNoTracking()
                .Where(p => p.LoanId == loan.Id)
                .GroupBy(p => p.LoanId)
                .Select(g => new
                {
                    TotalPrincipalPaid = g.Sum(p => p.AmountToPrinciple),
                    TotalInterestPaid = g.Sum(p => p.AmountToInterest)
                })
                .FirstOrDefaultAsync(cancellationToken);

            decimal rate = loan.InterestRate.PercentageRate / 100m;
            decimal expectedInterest = CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

            decimal totalPrincipalPaid = totals?.TotalPrincipalPaid ?? 0m;
            decimal totalInterestPaid = totals?.TotalInterestPaid ?? 0m;

            decimal loanOutstanding = loan.LoanAmount - totalPrincipalPaid;
            decimal loanBalance = (loan.LoanAmount + expectedInterest) - (totalPrincipalPaid + totalInterestPaid);

            return new LoanItemDetails
            {
                Id = loan.Id,
                BorrowerId = loan.BorrowerId,
                LoanAmount = loan.LoanAmount,
                IssueDate = loan.IssueDate,
                Reference = loan.Reference,
                Reason = loan.Reason,
                Duration = loan.Duration,
                InterestRate = loan.InterestRate,
                CreatedOn = loan.CreatedOn,
                LoanBalance = loanBalance,
                LoanOutstanding = loanOutstanding,
                TotalInterestReceived = totalInterestPaid
            };
        }
    }
}
