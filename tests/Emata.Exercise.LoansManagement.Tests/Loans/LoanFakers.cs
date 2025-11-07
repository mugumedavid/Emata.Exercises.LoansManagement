using System;
using Bogus;
using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;

namespace Emata.Exercise.LoansManagement.Tests.Loans;

public static class LoanFakers
{
    public static Faker<AddLoanCommand> AddLoanCommandFaker => new Faker<AddLoanCommand>()
        .CustomInstantiator(faker => new AddLoanCommand()
        {
            BorrowerId = Guid.NewGuid(),
            LoanAmount = faker.Finance.Amount(100000, 1000000),
            IssueDate = DateOnly.FromDateTime(faker.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now)),
            Reference = faker.Random.AlphaNumeric(10),
            Reason = faker.PickRandom("Business", "Education", "Medical", "Home", "Personal"),
            Duration = new DurationDto
            {
                Length = faker.Random.Int(1, 60),
                Period = faker.PickRandom<Period>()
            },
            InterestRate = new InterestRateDto
            {
                PercentageRate = faker.Random.Decimal(1, 25),
                Period = faker.PickRandom<Period>()
            }
        });
}
