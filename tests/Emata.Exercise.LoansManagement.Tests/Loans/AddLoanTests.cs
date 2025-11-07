using System;
using Emata.Exercise.LoansManagement.Contracts.Borrowers.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Tests.Borrowers;
using Emata.Exercise.LoansManagement.Tests.Setup;
using Shouldly;
using Xunit.Abstractions;

namespace Emata.Exercise.LoansManagement.Tests.Loans;

[Collection(LoansCollectionFixture.CollectionName)]
public class AddLoanTests : IAsyncLifetime
{
    private readonly ILoansRefitApi _loansApi;
    private readonly IBorrowersRefitApi _borrowersApi;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Func<Task> _resetDatabaseAsync;
    private BorrowerDTO _borrower = default!;

    public AddLoanTests(ApiFactory apiFactory, ITestOutputHelper testOutputHelper)
    {
        _loansApi = apiFactory.LoansApi;
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
        // Create a partner first
        var partner = BorrowerFakers.AddPartnerCommandFaker.Generate();
        var partnerResponse = await _borrowersApi.AddPartnerAsync(partner);
        await partnerResponse.EnsureSuccessfulAsync();

        // Create a borrower for testing loans
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = partnerResponse.Content!.Id };

        var borrowerResponse = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);
        await borrowerResponse.EnsureSuccessfulAsync();
        _borrower = borrowerResponse.Content!;
    }

    [Fact]
    public async Task AddLoan_ShouldCreateLoanSuccessfully()
    {
        // Arrange
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);
        var loan = response.Content;

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        loan.ShouldNotBeNull();
        loan.Id.ShouldNotBe(Guid.Empty);
        loan.BorrowerId.ShouldBe(addLoanCommand.BorrowerId);
        loan.LoanAmount.ShouldBe(addLoanCommand.LoanAmount);
        loan.IssueDate.ShouldBe(addLoanCommand.IssueDate);
        loan.Reference.ShouldBe(addLoanCommand.Reference);
        loan.Reason.ShouldBe(addLoanCommand.Reason);

        _testOutputHelper.WriteLine("Created Loan ID: {0}", loan.Id);
    }

    [Fact]
    public async Task AddLoan_ShouldHandleMinimumLoanAmount()
    {
        // Arrange
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;
        addLoanCommand.LoanAmount = 100_000m; // Minimum amount

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();
        response.Content.LoanAmount.ShouldBe(100_000m);

        _testOutputHelper.WriteLine("Created Loan with minimum amount: {0}", response.Content.Id);
    }

    [Fact]
    public async Task AddLoan_ShouldHandleLargeLoanAmount()
    {
        // Arrange
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;
        addLoanCommand.LoanAmount = 999999999.99m; // Large amount

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();
        response.Content.LoanAmount.ShouldBe(999999999.99m);

        _testOutputHelper.WriteLine("Created Loan with large amount: {0}", response.Content.Id);
    }

    [Fact]
    public async Task AddLoan_ShouldHandleTodayIssueDate()
    {
        // Arrange
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;
        addLoanCommand.IssueDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();
        response.Content.IssueDate.ShouldBe(addLoanCommand.IssueDate);
        response.Content.IssueDate.ShouldBeLessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now));

        _testOutputHelper.WriteLine("Created Loan with today's issue date: {0}", response.Content.Id);
    }

    [Fact]
    public async Task AddLoan_ShouldHandleNullOptionalFields()
    {
        // Arrange
        var addLoanCommand = new AddLoanCommand
        {
            BorrowerId = _borrower.Id,
            LoanAmount = 200_000m,
            IssueDate = DateOnly.FromDateTime(DateTime.Now),
            Reference = null,
            Reason = null,
            Duration = new DurationDto { Length = 12, Period = Period.Monthly },
            InterestRate = new InterestRateDto { PercentageRate = 5.5m, Period = Period.Annual }
        };

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();
        response.Content.Reference.ShouldBeNull();
        response.Content.Reason.ShouldBeNull();

        _testOutputHelper.WriteLine("Created Loan with null optional fields: {0}", response.Content.Id);
    }

    [Fact]
    public async Task AddLoan_ShouldFail_WhenBorrowerDoesNotExist()
    {
        // Arrange
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = Guid.NewGuid(); // Non-existent borrower

        // Act
        var response = await _loansApi.AddLoanAsync(addLoanCommand);

        // Assert
        response.IsSuccessful.ShouldBeFalse();

        _testOutputHelper.WriteLine("Correctly failed to create loan for non-existent borrower. Status: {0}", response.StatusCode);
    }

    [Fact]
    public async Task AddLoan_MinimumAmount_EdgeValues_ShouldEnforceMinimum()
    {
        // Arrange - just below minimum (should fail)
        var belowMinimum = LoanFakers.AddLoanCommandFaker.Generate();
        belowMinimum.BorrowerId = _borrower.Id;
        belowMinimum.LoanAmount = 100_000m - 1m; // 99,999

        var belowResp = await _loansApi.AddLoanAsync(belowMinimum);

        // Assert - below minimum must be rejected
        belowResp.IsSuccessful.ShouldBeFalse();
        _testOutputHelper.WriteLine("Loan with amount {0} was correctly rejected (below minimum). Status: {1}", belowMinimum.LoanAmount, belowResp.StatusCode);

        // Arrange - just above minimum (should pass)
        var aboveMinimum = LoanFakers.AddLoanCommandFaker.Generate();
        aboveMinimum.BorrowerId = _borrower.Id;
        aboveMinimum.LoanAmount = 100_000m + 1m; // 100,001

        var aboveResp = await _loansApi.AddLoanAsync(aboveMinimum);

        // Assert - above minimum must be accepted
        aboveResp.IsSuccessful.ShouldBeTrue();
        aboveResp.Content.ShouldNotBeNull();
        aboveResp.Content.LoanAmount.ShouldBe(100_000m + 1m);
        _testOutputHelper.WriteLine("Loan with amount {0} was created successfully. Id: {1}", aboveMinimum.LoanAmount, aboveResp.Content.Id);
    }
}
