using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Shared;
using Emata.Exercise.LoansManagement.Repayments.Domain;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Tests.Setup;
using Shouldly;
using Xunit;

namespace Emata.Exercise.LoansManagement.Tests.Repayments;

[Collection(RepaymentsCollectionFixture.CollectionName)]
public class LoanCalculatorServiceTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public LoanCalculatorServiceTests(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Ensure a clean DB for each test class run
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetBalanceSummaryAsync_NoRepayments_ComputesExpectedValues()
    {
        using var scope = _factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
        var concreteSvc = svc as LoanCalculatorService;
        concreteSvc.ShouldNotBeNull();

        var loan = new LoanItem
        {
            Id = Guid.NewGuid(),
            BorrowerId = Guid.NewGuid(),
            LoanAmount = 1_000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Duration = new DurationDto { Length = 1, Period = Period.Annual },
            InterestRate = new InterestRateDto { PercentageRate = 10m, Period = Period.Annual },
            CreatedOn = DateTime.UtcNow
        };

        var result = await svc.GetBalanceSummaryAsync(loan, CancellationToken.None);

        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = concreteSvc.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        // a) Loan Outstanding = LoanAmount - repaid principal (0)
        result.LoanOutstanding.ShouldBe(loan.LoanAmount);

        // b) Loan Balance = LoanAmount + expectedInterest - repaid principal - repaid interest (both 0)
        result.LoanBalance.ShouldBe(loan.LoanAmount + expectedInterest);

        // c) Total interest received = 0
        result.TotalInterestReceived.ShouldBe(0m);

        // sanity
        result.LoanAmount.ShouldBe(loan.LoanAmount);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_WithRepayments_ComputesExpectedValues()
    {
        using var scope = _factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
        var concreteSvc = svc as LoanCalculatorService;
        concreteSvc.ShouldNotBeNull();

        var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var loanId = Guid.NewGuid();
        var loan = new LoanItem
        {
            Id = loanId,
            BorrowerId = Guid.NewGuid(),
            LoanAmount = 1_000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)),
            Duration = new DurationDto { Length = 1, Period = Period.Annual },
            InterestRate = new InterestRateDto { PercentageRate = 10m, Period = Period.Annual },
            CreatedOn = DateTime.UtcNow
        };

        // create repayments: principal 200 + 100 = 300, interest 5 + 10 = 15
        var r1 = Repayment.Create(loanId, 200m, 5m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var r2 = Repayment.Create(loanId, 100m, 10m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));

        paymentsDb.Repayments.AddRange(r1, r2);
        await paymentsDb.SaveChangesAsync();

        var result = await svc.GetBalanceSummaryAsync(loan, CancellationToken.None);

        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = concreteSvc.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        var totalPrincipalPaid = 200m + 100m;
        var totalInterestPaid = 5m + 10m;

        // a) Loan Outstanding = LoanAmount - repaid principal
        result.LoanOutstanding.ShouldBe(loan.LoanAmount - totalPrincipalPaid);

        // b) Loan Balance = LoanAmount + expectedInterest - repaid principal - repaid interest
        result.LoanBalance.ShouldBe((loan.LoanAmount + expectedInterest) - (totalPrincipalPaid + totalInterestPaid));

        // c) Total interest received is the sum of repaid interest
        result.TotalInterestReceived.ShouldBe(totalInterestPaid);
    }

    [Fact]
    public async Task GetPaymentSummary_ReturnsBalancesForEachPayment()
    {
        using var scope = _factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
        var concreteSvc = svc as LoanCalculatorService;
        concreteSvc.ShouldNotBeNull();

        var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var loanId = Guid.NewGuid();
        var loan = new LoanItem
        {
            Id = loanId,
            BorrowerId = Guid.NewGuid(),
            LoanAmount = 1_000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)),
            Duration = new DurationDto { Length = 1, Period = Period.Annual },
            InterestRate = new InterestRateDto { PercentageRate = 10m, Period = Period.Annual },
            CreatedOn = DateTime.UtcNow
        };

        var r1 = Repayment.Create(loanId, 200m, 5m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var r2 = Repayment.Create(loanId, 100m, 10m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));

        paymentsDb.Repayments.AddRange(r1, r2);
        await paymentsDb.SaveChangesAsync();

        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = concreteSvc.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        // Payment 1 cumulative totals (after payment 1)
        var totalPrincipalAfterP1 = r1.AmountToPrinciple;
        var totalInterestAfterP1 = r1.AmountToInterest;

        var paymentDto1 = new Payment
        {
            Id = r1.Id,
            LoanId = r1.LoanId,
            AmountToPrinciple = r1.AmountToPrinciple,
            AmountToInterest = r1.AmountToInterest,
            EntryDate = r1.EntryDate,
            CreatedOn = r1.CreatedOn
        };

        var summary1 = concreteSvc.GetPaymentSummary(paymentDto1, loan, totalPrincipalAfterP1, totalInterestAfterP1, expectedInterest);

        summary1.LoanOutstanding.ShouldBe(loan.LoanAmount - totalPrincipalAfterP1);
        summary1.LoanBalance.ShouldBe((loan.LoanAmount + expectedInterest) - (totalPrincipalAfterP1 + totalInterestAfterP1));
        summary1.TotalInterestReceived.ShouldBe(totalInterestAfterP1);

        // Payment 2 cumulative totals (after payment 2)
        var totalPrincipalAfterP2 = r1.AmountToPrinciple + r2.AmountToPrinciple;
        var totalInterestAfterP2 = r1.AmountToInterest + r2.AmountToInterest;

        var paymentDto2 = new Payment
        {
            Id = r2.Id,
            LoanId = r2.LoanId,
            AmountToPrinciple = r2.AmountToPrinciple,
            AmountToInterest = r2.AmountToInterest,
            EntryDate = r2.EntryDate,
            CreatedOn = r2.CreatedOn
        };

        var summary2 = concreteSvc.GetPaymentSummary(paymentDto2, loan, totalPrincipalAfterP2, totalInterestAfterP2, expectedInterest);

        summary2.LoanOutstanding.ShouldBe(loan.LoanAmount - totalPrincipalAfterP2);
        summary2.LoanBalance.ShouldBe((loan.LoanAmount + expectedInterest) - (totalPrincipalAfterP2 + totalInterestAfterP2));
        summary2.TotalInterestReceived.ShouldBe(totalInterestAfterP2);
    }
}