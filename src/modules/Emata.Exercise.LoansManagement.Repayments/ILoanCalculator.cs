using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Domain;

namespace Emata.Exercise.LoansManagement.Repayments
{
    public interface ILoanCalculator
    {
        decimal GetDurationInYears(DurationDto? duration);
        
        decimal CalculateExpectedInterest(decimal principal, decimal rate, DurationDto? duration);

        PaymentSummaryDTO CreatePaymentSummary(Repayment payment, LoanItem loan, 
            decimal totalPrincipalPaid, decimal totalInterestPaid, decimal expectedInterest);
    }
}
