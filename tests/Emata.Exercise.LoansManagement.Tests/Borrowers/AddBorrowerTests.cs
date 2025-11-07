using System;
using Emata.Exercise.LoansManagement.Contracts.Borrowers.DTOs;
using Emata.Exercise.LoansManagement.Tests.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace Emata.Exercise.LoansManagement.Tests.Borrowers;

[Collection(BorrowersCollectionFixture.CollectionName)]
public class AddBorrowerTests : IAsyncLifetime
{
    private readonly IBorrowersRefitApi _borrowersApi;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Func<Task> _resetDatabaseAsync;
    private PartnerDTO _partner = default!;

    public AddBorrowerTests(ApiFactory apiFactory, ITestOutputHelper testOutputHelper)
    {
        _borrowersApi = apiFactory.BorrowersApi;
        _testOutputHelper = testOutputHelper;
        _resetDatabaseAsync = apiFactory.ResetDatabaseAsync;
    }

    public Task DisposeAsync()
    {
        return _resetDatabaseAsync();
    }

    public async Task InitializeAsync()
    {
        // Create a partner for testing borrowers
        var addPartnerCommand = BorrowerFakers.AddPartnerCommandFaker.Generate();
        var partnerResponse = await _borrowersApi.AddPartnerAsync(addPartnerCommand);
        await partnerResponse.EnsureSuccessfulAsync();
        _partner = partnerResponse.Content!;
    }

    [Fact]
    public async Task AddBorrower_ShouldCreateBorrowerSuccessfully()
    {
        // Arrange
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = _partner.Id };

        // Act
        var response = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);
        var borrower = response.Content;

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        borrower.ShouldNotBeNull();
        borrower.Id.ShouldNotBe(Guid.Empty);
        borrower.Surname.ShouldBe(addBorrowerCommand.Surname);
        borrower.GivenName.ShouldBe(addBorrowerCommand.GivenName);
        borrower.Gender.ShouldBe(addBorrowerCommand.Gender);
        borrower.DateOfBirth.ShouldBe(addBorrowerCommand.DateOfBirth);
        borrower.PhoneNumber.ShouldBe(addBorrowerCommand.PhoneNumber);

        _testOutputHelper.WriteLine("Created Borrower ID: {0}", borrower.Id);
    }

    [Fact]
    public async Task AddBorrower_ShouldHandleYoungAdult()
    {
        // Arrange
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with 
        { 
            PartnerId = _partner.Id,
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-18)) // 18 years old
        };

        // Act
        var response = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();
        response.Content.DateOfBirth.ShouldBe(addBorrowerCommand.DateOfBirth);

        _testOutputHelper.WriteLine("Created Young Adult Borrower ID: {0}", response.Content.Id);
    }

    [Fact]
    public async Task AddBorrower_ShouldHandleNonExistentPartner()
    {
        // Arrange
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = Guid.NewGuid() }; // Non-existent partner

        // Act
        var response = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);

        // Assert
        response.IsSuccessful.ShouldBeFalse();

        _testOutputHelper.WriteLine("Correctly failed to create borrower with non-existent partner. Status: {0}", response.StatusCode);
    }

    [Fact]
    public async Task AddBorrower_ShouldReturnBadRequest_WhenSurnameMissing()
    {
        // Arrange
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = _partner.Id, Surname = null };

        // Act
        var response = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);

        // Assert
        response.IsSuccessful.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        _testOutputHelper.WriteLine("Missing surname correctly resulted in BadRequest.");
    }

    [Fact]
    public async Task AddBorrower_ShouldReturnBadRequest_WhenGivenNameMissing()
    {
        // Arrange
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = _partner.Id, GivenName = null };

        // Act
        var response = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);

        // Assert
        response.IsSuccessful.ShouldBeFalse();
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);

        _testOutputHelper.WriteLine("Missing given name correctly resulted in BadRequest.");
    }

    [Fact]
    public async Task AddBorrower_IdentificationNumber_MustBe14Characters_WhenProvided()
    {
        // Arrange - invalid length (13)
        var invalid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        invalid = invalid with { PartnerId = _partner.Id, IdentificationNumber = "1234567890123" }; // 13 chars

        var invalidResp = await _borrowersApi.AddBorrowerAsync(invalid);
        invalidResp.IsSuccessful.ShouldBeFalse();
        invalidResp.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        _testOutputHelper.WriteLine("Identification number with 13 chars correctly rejected.");

        // Arrange - valid length (14)
        var valid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        valid = valid with { PartnerId = _partner.Id, IdentificationNumber = "12345678901234" }; // 14 chars

        var validResp = await _borrowersApi.AddBorrowerAsync(valid);
        validResp.IsSuccessful.ShouldBeTrue();
        validResp.Content.ShouldNotBeNull();
        validResp.Content.IdentificationNumber.ShouldBe("12345678901234");
        _testOutputHelper.WriteLine("Identification number with 14 chars accepted.");
    }

    [Fact]
    public async Task AddBorrower_PhoneNumber_Validation_TenDigitsStartingWithZero()
    {
        // Arrange - invalid phone (doesn't start with 0)
        var invalid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        invalid = invalid with { PartnerId = _partner.Id, PhoneNumber = "1234567890" };

        var invalidResp = await _borrowersApi.AddBorrowerAsync(invalid);
        invalidResp.IsSuccessful.ShouldBeFalse();
        invalidResp.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        _testOutputHelper.WriteLine("Invalid phone format correctly rejected.");

        // Arrange - valid phone
        var valid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        valid = valid with { PartnerId = _partner.Id, PhoneNumber = "0712345678" };

        var validResp = await _borrowersApi.AddBorrowerAsync(valid);
        validResp.IsSuccessful.ShouldBeTrue();
        validResp.Content.ShouldNotBeNull();
        validResp.Content.PhoneNumber.ShouldBe("0712345678");
        _testOutputHelper.WriteLine("Valid phone format accepted.");
    }

    [Fact]
    public async Task AddBorrower_Email_Validation_WhenProvided()
    {
        // Arrange - invalid email
        var invalid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        invalid = invalid with { PartnerId = _partner.Id, Email = "not-an-email" };

        var invalidResp = await _borrowersApi.AddBorrowerAsync(invalid);
        invalidResp.IsSuccessful.ShouldBeFalse();
        invalidResp.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        _testOutputHelper.WriteLine("Invalid email correctly rejected.");

        // Arrange - valid email
        var valid = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        valid = valid with { PartnerId = _partner.Id, Email = "john.doe@example.com" };

        var validResp = await _borrowersApi.AddBorrowerAsync(valid);
        validResp.IsSuccessful.ShouldBeTrue();
        validResp.Content.ShouldNotBeNull();
        validResp.Content.Email.ShouldBe("john.doe@example.com");
        _testOutputHelper.WriteLine("Valid email accepted.");
    }
}
