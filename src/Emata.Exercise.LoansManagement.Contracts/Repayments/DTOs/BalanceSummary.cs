namespace Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs
{
    public record BalanceSummary
    {
        public decimal LoanBalance { get; set; }

        public decimal LoanOutstanding { get; set; }

        public decimal TotalInterestReceived { get; set; }
    }
}
