using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;

namespace Emata.Exercise.LoansManagement.Contracts.Repayments
{
    public record GetSingleRepaymentQuery : IQuery<PaymentSummaryDTO>
    {
        public Guid? PaymentId { get; set; }
    }
}
