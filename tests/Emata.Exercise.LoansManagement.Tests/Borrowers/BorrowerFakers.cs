using System;
using Bogus;
using Emata.Exercise.LoansManagement.Contracts.Borrowers.DTOs;

namespace Emata.Exercise.LoansManagement.Tests.Borrowers;

public static class BorrowerFakers
{
    public static Faker<AddPartnerCommand> AddPartnerCommandFaker => new Faker<AddPartnerCommand>()
        .CustomInstantiator(faker => new AddPartnerCommand()
        {
            Name = faker.Company.CompanyName(),
            Town = faker.Address.City()
        });

    public static Faker<AddBorrowerCommand> AddBorrowerCommandFaker => new Faker<AddBorrowerCommand>()
        .CustomInstantiator(faker => new AddBorrowerCommand()
        {
            Surname = faker.Name.LastName(),
            GivenName = faker.Name.FirstName(),
            Gender = faker.PickRandom<Gender>(),
            DateOfBirth = DateOnly.FromDateTime(faker.Date.Past(50, DateTime.Now.AddYears(-18))),
            IdentificationNumber = faker.Random.AlphaNumeric(14),
            PhoneNumber = faker.Random.Replace("0#########"),
            Email = faker.Internet.Email(),
            Town = faker.Address.City(),
            PartnerId = Guid.NewGuid()
        });
}
