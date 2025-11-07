using System;
using System.Threading.Tasks;
using Emata.Exercise.LoansManagement.Contracts.Borrowers.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Tests.Borrowers;
using Emata.Exercise.LoansManagement.Tests.Loans;
using Emata.Exercise.LoansManagement.Tests.Setup;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Emata.Exercise.LoansManagement.Tests.Repayments;

[Collection(RepaymentsCollectionFixture.CollectionName)]
public class AddRepaymentTests : IAsyncLifetime
{
    private readonly IRepaymentsRefitApi _repaymentsApi;
    private readonly ILoansRefitApi _loansApi;
    private readonly IBorrowersRefitApi _borrowersApi;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Func<Task> _resetDatabaseAsync;
    private BorrowerDTO _borrower = default!;

    public AddRepaymentTests(ApiFactory apiFactory, ITestOutputHelper testOutputHelper)
    {
        _repaymentsApi = apiFactory.RepaymentsApi;
        _loansApi = apiFactory.LoansApi;
        _borrowersApi = apiFactory.BorrowersApi;
        _testOutputHelper = testOutputHelper;
        _resetDatabaseAsync = apiFactory.ResetDatabaseAsync;
    }

    public Task DisposeAsync() => _resetDatabaseAsync();

    public async Task InitializeAsync()
    {
        // Create a partner first
        var partner = BorrowerFakers.AddPartnerCommandFaker.Generate();
        var partnerResponse = await _borrowersApi.AddPartnerAsync(partner);
        await partnerResponse.EnsureSuccessfulAsync();

        // Create a borrower for testing loans and repayments
        var addBorrowerCommand = BorrowerFakers.AddBorrowerCommandFaker.Generate();
        addBorrowerCommand = addBorrowerCommand with { PartnerId = partnerResponse.Content!.Id };

        var borrowerResponse = await _borrowersApi.AddBorrowerAsync(addBorrowerCommand);
        await borrowerResponse.EnsureSuccessfulAsync();
        _borrower = borrowerResponse.Content!;
    }

    [Fact]
    public async Task AddRepayment_ShouldCreatePaymentSuccessfully()
    {
        // Arrange - create a loan first
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        var loanResponse = await _loansApi.AddLoanAsync(addLoanCommand);
        await loanResponse.EnsureSuccessfulAsync();
        var loan = loanResponse.Content!;

        // Prepare a valid payment (entry date after loan issue date)
        var addPaymentCommand = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = Math.Min(100m, loan.LoanAmount),
            AmountToInterest = 5m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        // Act
        var response = await _repaymentsApi.AddPaymentAsync(addPaymentCommand);

        // Assert
        response.IsSuccessful.ShouldBeTrue();
        response.Content.ShouldNotBeNull();

        PaymentSummaryDTO summary = response.Content!;
        summary.Principal.ShouldBe(addPaymentCommand.AmountToPrinciple);
        summary.Interest.ShouldBe(addPaymentCommand.AmountToInterest);
        summary.TotalPayment.ShouldBe(addPaymentCommand.AmountToPrinciple + addPaymentCommand.AmountToInterest);
        summary.Id.ShouldNotBe(Guid.Empty);

        _testOutputHelper.WriteLine("Created Payment ID: {0}", summary.Id);
    }

    [Fact]
    public async Task AddRepayment_ShouldFail_WhenLoanDoesNotExist()
    {
        // Arrange
        var addPaymentCommand = new AddPaymentCommand
        {
            LoanId = Guid.NewGuid(), // non-existent
            AmountToPrinciple = 50m,
            AmountToInterest = 5m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        // Act
        var response = await _repaymentsApi.AddPaymentAsync(addPaymentCommand);

        // Assert - handler throws when loan not found, API should return a non-successful response
        response.IsSuccessful.ShouldBeFalse();
        _testOutputHelper.WriteLine("Correctly failed to create payment for non-existent loan. Status: {0}", response.StatusCode);
    }

    [Fact]
    public async Task AddRepayment_ShouldFail_WhenAmountsAreInvalid()
    {
        // Arrange - create a loan first
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        var loanResponse = await _loansApi.AddLoanAsync(addLoanCommand);
        await loanResponse.EnsureSuccessfulAsync();
        var loan = loanResponse.Content!;

        // Amount to principle must be > 0
        var zeroPrincipalCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = 0m,
            AmountToInterest = 5m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        var zeroPrincipalResp = await _repaymentsApi.AddPaymentAsync(zeroPrincipalCmd);
        zeroPrincipalResp.IsSuccessful.ShouldBeFalse();

        // Amount to interest must be > 0
        var zeroInterestCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = 10m,
            AmountToInterest = 0m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        var zeroInterestResp = await _repaymentsApi.AddPaymentAsync(zeroInterestCmd);
        zeroInterestResp.IsSuccessful.ShouldBeFalse();

        _testOutputHelper.WriteLine("Invalid amount checks passed (requests rejected).");
    }

    [Fact]
    public async Task AddRepayment_ShouldFail_WhenPrincipalExceedsRemainingBalance()
    {
        // Arrange - create a loan first
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        var loanResponse = await _loansApi.AddLoanAsync(addLoanCommand);
        await loanResponse.EnsureSuccessfulAsync();
        var loan = loanResponse.Content!;

        // Attempt to pay more principal than the loan amount
        var overpayCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = loan.LoanAmount + 100m,
            AmountToInterest = 1m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        var overpayResp = await _repaymentsApi.AddPaymentAsync(overpayCmd);
        overpayResp.IsSuccessful.ShouldBeFalse();

        _testOutputHelper.WriteLine("Overpayment attempt correctly rejected. Status: {0}", overpayResp.StatusCode);
    }

    [Fact]
    public async Task AddRepayment_ShouldFail_WhenEntryDateIsOnOrBeforeLoanIssueDate()
    {
        // Arrange - create a loan first
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        var loanResponse = await _loansApi.AddLoanAsync(addLoanCommand);
        await loanResponse.EnsureSuccessfulAsync();
        var loan = loanResponse.Content!;

        // Use entry date equal to loan issue date (invalid)
        var invalidEntryCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = 10m,
            AmountToInterest = 1m,
            EntryDate = loan.IssueDate // same day => should be rejected
        };

        // Act
        var respEqual = await _repaymentsApi.AddPaymentAsync(invalidEntryCmd);

        // Assert
        respEqual.IsSuccessful.ShouldBeFalse();
        _testOutputHelper.WriteLine("Payment with entry date equal to issue date correctly rejected. Status: {0}", respEqual.StatusCode);

        // Also test entry date before issue date
        var beforeEntryCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = 10m,
            AmountToInterest = 1m,
            EntryDate = loan.IssueDate.AddDays(-1)
        };

        var respBefore = await _repaymentsApi.AddPaymentAsync(beforeEntryCmd);
        respBefore.IsSuccessful.ShouldBeFalse();
        _testOutputHelper.WriteLine("Payment with entry date before issue date correctly rejected. Status: {0}", respBefore.StatusCode);
    }

    [Fact]
    public async Task AddRepayment_ShouldFail_WhenAmountToInterestExceedsExpectedInterest()
    {
        // Arrange - create a loan first
        var addLoanCommand = LoanFakers.AddLoanCommandFaker.Generate();
        addLoanCommand.BorrowerId = _borrower.Id;

        var loanResponse = await _loansApi.AddLoanAsync(addLoanCommand);
        await loanResponse.EnsureSuccessfulAsync();
        var loan = loanResponse.Content!;

        // Build a payment where AmountToInterest is unreasonably large so it exceeds expected interest
        var excessiveInterestCmd = new AddPaymentCommand
        {
            LoanId = loan.Id,
            AmountToPrinciple = 1m,
            AmountToInterest = loan.LoanAmount * 10m, // intentionally large
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };

        // Act
        var resp = await _repaymentsApi.AddPaymentAsync(excessiveInterestCmd);

        // Assert
        resp.IsSuccessful.ShouldBeFalse();
        _testOutputHelper.WriteLine("Payment with excessive interest correctly rejected. Status: {0}", resp.StatusCode);
    }
}