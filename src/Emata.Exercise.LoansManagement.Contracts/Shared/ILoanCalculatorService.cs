using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Shared
{
    public interface ILoanCalculatorService
    {
        decimal GetDurationInYears(DurationDto? duration);
        
        decimal CalculateExpectedInterest(decimal principal, decimal rate, DurationDto? duration);

        PaymentSummaryDTO GetPaymentSummary(Payment payment, LoanItem loan, 
            decimal totalPrincipalPaid, decimal totalInterestPaid, decimal expectedInterest);

        Task<LoanItemDetails> GetBalanceSummaryAsync(LoanItem loan, CancellationToken cancellationToken);
    }
}
