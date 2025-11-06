using System;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Loans.Domain;

namespace Emata.Exercise.LoansManagement.Loans.UseCases;

internal static class MappingExtensions
{
    public static LoanItem ToDTO(this Loan loan) => new()
    {
        Id = loan.Id,
        BorrowerId = loan.BorrowerId,
        LoanAmount = loan.LoanAmount,
        IssueDate = loan.IssueDate,
        Reference = loan.Reference,
        Reason = loan.Reason,
        Duration = loan.Duration is not null
            ? new DurationDto
            {
                Length = loan.Duration.Length,
                Period = loan.Duration.Period
            }
            : null,
        InterestRate = new InterestRateDto
        {
            PercentageRate = loan.InterestRate.PercentageRate,
            Period = loan.InterestRate.Period
        },
        CreatedOn = loan.CreatedOn
    };

}
