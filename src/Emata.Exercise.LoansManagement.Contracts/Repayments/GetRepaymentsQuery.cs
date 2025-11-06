using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Repayments
{
    public record GetRepaymentsQuery : IQuery<List<PaymentSummaryDTO>>
    {
        public Guid? LoanId { get; set; }
    }
}
