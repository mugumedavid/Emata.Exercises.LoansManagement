using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Shared;
using Emata.Exercise.LoansManagement.Loans.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Loans.UseCases;
using Emata.Exercise.LoansManagement.Shared;
using Emata.Exercise.LoansManagement.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Loans;

internal class LoansEndpoints : IEndpoints
{
    public string? Prefix => "loans";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet($"health", () => Results.Ok("Loans API is healthy"))
            .WithSummary("Get Health")
            .WithDescription("Checks the health of the Loans API");

        //create a new loan
        app.MapPost($"", async (
            [FromServices] ICommandHandler<AddLoanCommand, LoanItem> handler,
            [FromBody] AddLoanCommand addLoanRequest) =>
        {
            var loanItem = await handler.Handle(addLoanRequest);
            return Results.Created($"/loans/{loanItem.BorrowerId}", loanItem);
        })
        .WithSummary("Create a new loan")
        .WithDescription("Creates a new loan for a borrower.");

        //query for loans
        app.MapGet($"", async (
            [FromServices] IQueryHandler<GetLoansQuery, List<LoanItemDetails>> handler,
            [AsParameters] GetLoansQuery request) =>
        {
            var loans = await handler.Handle(request);
            return Results.Ok(loans);
        })
        .WithName("Get All Loans")
        .WithSummary("Get all loans")
        .WithDescription("Retrieves a list of all loans in the system.");

        //query for a specific loan
        app.MapGet("{id:guid}", async (Guid id, LoansDbContext dbContext, ILoanCalculatorService loanCalculator, CancellationToken cancellationToken) =>
        {
            var loan = await dbContext.Loans
                .FirstOrDefaultAsync(l => l.Id == id);
            var loanSummary = loan != null ? await loanCalculator.GetBalanceSummaryAsync(loan.ToDTO(), cancellationToken) : null;
            return loan is not null ? Results.Ok(loanSummary) : Results.NotFound();
        })
        .WithName("Get Loan By ID")
        .WithSummary("Get loan by ID")
        .WithDescription("Retrieves a specific loan by its ID.");
    }
}
