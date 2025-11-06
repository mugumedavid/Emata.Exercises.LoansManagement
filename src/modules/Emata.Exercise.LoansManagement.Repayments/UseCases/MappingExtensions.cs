using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Domain;

namespace Emata.Exercise.LoansManagement.Repayments.UseCases
{
    internal static class MappingExtensions
    {
        public static Payment ToDTO(this Repayment repayment)
        {
            ArgumentNullException.ThrowIfNull(repayment);
            return new Payment
            {
                Id = repayment.Id,
                AmountToPrinciple = repayment.AmountToPrinciple,
                AmountToInterest = repayment.AmountToInterest,
                EntryDate = repayment.EntryDate,
                CreatedOn = repayment.CreatedOn
            };
        }
    }
}
