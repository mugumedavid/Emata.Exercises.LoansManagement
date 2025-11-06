namespace Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;

public record class LoanItem
{
    public Guid Id { get; set; }

    public Guid BorrowerId { get; set; }

    public decimal LoanAmount { get; set; }

    public DateOnly IssueDate { get; set; }

    public string? Reference { get; set; }

    public string? Reason { get; set; }

    public DurationDto? Duration { get; set; }

    public InterestRateDto InterestRate { get; set; } = new InterestRateDto();

    public DateTime CreatedOn { get; set; }
}