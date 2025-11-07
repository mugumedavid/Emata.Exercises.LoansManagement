using Emata.Exercise.LoansManagement.Borrowers.Domain;
using Emata.Exercise.LoansManagement.Borrowers.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Contracts.Borrowers.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Exceptions;
using Emata.Exercise.LoansManagement.Shared;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Emata.Exercise.LoansManagement.Borrowers.UseCases.Borrowers;

internal class AddBorrowerCommandHandler : ICommandHandler<AddBorrowerCommand, BorrowerDTO>
{
    private readonly BorrowersDbContext _dbContext;

    public AddBorrowerCommandHandler(BorrowersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BorrowerDTO> Handle(AddBorrowerCommand request, CancellationToken cancellationToken = default)
    {
        //get partner by Id
        var partner = await _dbContext.Partners.FindAsync([request.PartnerId], cancellationToken);
        if (partner == null)
        {
            throw new LoansManagementNotFoundException($"Partner with Id {request.PartnerId} not found."); //In real scenario, use a custom exception
        }

        if (string.IsNullOrWhiteSpace(request.Surname)) throw new LoansManagementValueException("Surname is required");
        
        if (string.IsNullOrWhiteSpace(request.GivenName)) throw new LoansManagementValueException("GivenName is required");
        
        if (!string.IsNullOrWhiteSpace(request.IdentificationNumber) &&
                request.IdentificationNumber.Length != 14) throw new LoansManagementValueException("Identification Number (NIN) must be 14 characters long.");
        
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneRegex = @"^0\d{9}$"; // 10 digits starting with 0
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.PhoneNumber, phoneRegex))
            {
                throw new LoansManagementValueException("Phone nummber should be ten digits e.g. 0712345678");
            }
        }
        
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            try
            {
                var email = new System.Net.Mail.MailAddress(request.Email);
                if (email.Address != request.Email) throw new Exception();
            }
            catch
            {
                throw new LoansManagementValueException("Email address is not valid");
            }
        }

        var borrower = BorrowerBuilder.Create()
            .SetSurname(request.Surname)
            .SetGivenName(request.GivenName)
            .SetGender(request.Gender)
            .SetPhoneNumber(request.PhoneNumber)
            .SetEmail(request.Email)
            .SetDateOfBirth(request.DateOfBirth)
            .SetIdentificationNumber(request.IdentificationNumber)
            .SetPartnerId(request.PartnerId)
            .SetTown(request.Town)
            .Build();

        _dbContext.Borrowers.Add(borrower);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return borrower.ToDTO();
    }
}
