using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Domain;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Emata.Exercise.LoansManagement.Repayments.UseCases;

internal class AddRepaymentCommandHandler : ICommandHandler<AddPaymentCommand, PaymentSummaryDTO>
{

    private readonly ILogger<AddRepaymentCommandHandler> _logger;
    private readonly ILoanCalculator _loanCalculator;
    private readonly ILoanService _loansQueryService;
    private readonly PaymentsDbContext _paymentsDbContext;


    public AddRepaymentCommandHandler(PaymentsDbContext dbContext, ILogger<AddRepaymentCommandHandler> logger, ILoanService loansQueryService, ILoanCalculator loanCalculator)
    {
        _loansQueryService = loansQueryService;
        _paymentsDbContext = dbContext;
        _logger = logger;
        _loanCalculator = loanCalculator;
    }

    public async Task<PaymentSummaryDTO> Handle(AddPaymentCommand request, CancellationToken cancellationToken = default)
    {
        // Check loan exists
        var loan = await _loansQueryService.GetLoanByIdAsync(request.LoanId, cancellationToken);

        if (loan is null)
        {
            _logger.LogWarning("Loan {LoanId} not found when trying to create payment", request.LoanId);
            throw new Exception($"Loan with ID {request.LoanId} not found.");
        }

        CheckThatAmountToPrincipleIsGreaterThanZero(request);
        CheckThatAmountToInterestIsMoreThanZero(request);
        CheckThatEntryDateIsAfterLoanIssueDate(request, loan);

        var payment = Repayment.Create(
            request.LoanId,
            request.AmountToPrinciple,
            request.AmountToInterest,
            request.EntryDate);

        await CheckThatAmountToPrincipleIsNotMoreThanLoanBalance(loan, payment);
        CheckThatAmountToInterestIsNotMoreThanExpectedInterest(loan, payment);

        var localPrincipalPaid = _paymentsDbContext.Repayments
            .Local
            .Where(p => p.LoanId == loan.Id)
            .Sum(p => p.AmountToPrinciple);

        var localInterestPaid = _paymentsDbContext.Repayments
            .Local
            .Where(p => p.LoanId == loan.Id)
            .Sum(p => p.AmountToInterest);

        var totalPrincipalPaid = localPrincipalPaid + payment.AmountToPrinciple;
        var totalInterestPaid = localInterestPaid + payment.AmountToInterest;

        _logger.LogInformation("Creating payment for loan {LoanId}", request.LoanId);
        _paymentsDbContext.Repayments.Add(payment);
        await _paymentsDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Payment {PaymentId} created for loan {LoanId}", payment.Id, payment.LoanId);

        decimal rate = loan.InterestRate.PercentageRate / 100m;
        decimal expectedInterest = _loanCalculator.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        var summary = _loanCalculator.CreatePaymentSummary(
            payment,
            loan,
            totalPrincipalPaid,
            totalInterestPaid,
            expectedInterest
        );

        return summary;
    }

    private void CheckThatAmountToPrincipleIsGreaterThanZero(AddPaymentCommand request)
    {
        if (request.AmountToPrinciple <= 0)
        {
            _logger.LogWarning("The amount going to principle has to be greater than zero. It was {AmountToPrinciple}",
                request.AmountToPrinciple);
            throw new Exception($"Amount to principle should be greater than zero.");
        }
    }

    private void CheckThatAmountToInterestIsMoreThanZero(AddPaymentCommand request)
    {
        if (request.AmountToInterest <= 0)
        {
            _logger.LogWarning("The amount going to interest has to be greater than zero. It was {AmountToInterest}",
                request.AmountToInterest);
            throw new Exception($"Amount to interest should be greater than zero.");
        }
    }

    private void CheckThatEntryDateIsAfterLoanIssueDate(AddPaymentCommand request, LoanItem loan)
    {
        if (request.EntryDate <= loan.IssueDate)
        {
            _logger.LogWarning("The payment entry date {EntryDate} is not greater than the loan issue date {IssueDate}",
                request.EntryDate, loan.IssueDate);
            throw new Exception($"Payment entry date should be greater than loan issue date");
        }
    }

    private void CheckThatAmountToInterestIsNotMoreThanExpectedInterest(LoanItem loan, Repayment payment)
    {
        // Compute expected interest
        // Assume simple interest: 
        //  Interest = Principal × Rate × Time
        //  Example: 10% annual for 1 year on 10,000 = 1,000
        decimal principal = loan.LoanAmount;
        decimal rate = loan.InterestRate.PercentageRate / 100m;

        decimal expectedInterest = _loanCalculator.CalculateExpectedInterest(loan.LoanAmount, rate, loan.Duration);

        // Enforce the constraint
        if (payment.AmountToInterest > expectedInterest)
        {
            throw new Exception($"Payment interest ({payment.AmountToInterest}) cannot exceed expected interest ({expectedInterest}).");
        }
    }

    private async Task CheckThatAmountToPrincipleIsNotMoreThanLoanBalance(LoanItem loan, Repayment payment)
    {
        //Compute total principal already paid
        var totalPrincipalPaid = await _paymentsDbContext.Repayments
            .Where(p => p.LoanId == loan.Id)
            .SumAsync(p => (decimal?)p.AmountToPrinciple) ?? 0m;

        //Compute remaining loan balance
        decimal loanBalance = loan.LoanAmount - totalPrincipalPaid;

        //Enforce the constraint
        if (payment.AmountToPrinciple > loanBalance)
        {
            _logger.LogWarning("Payment to principle was more than loan balance");
            throw new Exception($"Payment to principal ({payment.AmountToPrinciple}) " +
                                  $"must not be more than remaining balance ({loanBalance}).");
        }
    }
}