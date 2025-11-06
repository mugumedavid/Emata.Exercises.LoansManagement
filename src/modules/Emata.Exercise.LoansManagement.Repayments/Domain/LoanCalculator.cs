using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

namespace Emata.Exercise.LoansManagement.Repayments.Domain
{
    public class LoanCalculator : ILoanCalculator
    {
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

        public PaymentSummaryDTO CreatePaymentSummary(
            Repayment payment, 
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

        public BalanceSummary CreateBalanceSummary(
            LoanItem loan,
            decimal totalPrincipalPaid,
            decimal totalInterestPaid,
            decimal expectedInterest)
        {
            decimal loanOutstanding = loan.LoanAmount - totalPrincipalPaid;
            decimal loanBalance = (loan.LoanAmount + expectedInterest) - (totalPrincipalPaid + totalInterestPaid);

            return new BalanceSummary
            {
                LoanBalance = loanBalance,
                LoanOutstanding = loanOutstanding,
                TotalInterestReceived = totalInterestPaid
            };
        }
    }
}
