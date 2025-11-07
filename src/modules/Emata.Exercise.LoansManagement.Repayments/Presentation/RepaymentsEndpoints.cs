using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Shared;
using Emata.Exercise.LoansManagement.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Emata.Exercise.LoansManagement.Repayments.Presentation
{
    public class RepaymentsEndpoints : IEndpoints
    {
        public string? Prefix => "repayments";

        public void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet($"health", () => Results.Ok("Repayments API is healthy"))
                .WithSummary("Get Health")
                .WithDescription("Checks the health of the Repayments API");

            //add repayment
            app.MapPost($"", async (
                [FromServices] ICommandHandler<AddPaymentCommand, PaymentSummaryDTO> handler,
                [FromBody] AddPaymentCommand command,
                CancellationToken cancellationToken) =>
            {
                var repayment = await handler.Handle(command, cancellationToken);
                return Results.Created($"/{Prefix}/{repayment.Id}", repayment);
            })
                .WithSummary("Add Repayment")
                .WithDescription("Adds a new payment to a loan");

            //get repayments for a given loan
            app.MapGet($"loan/{{loanId:guid}}", async (
                Guid loanId,
                [FromServices] IQueryHandler<GetRepaymentsQuery, List<PaymentSummaryDTO>> handler,
                CancellationToken cancellationToken) =>
            {
                var request = new GetRepaymentsQuery { LoanId = loanId };
                var payments = await handler.Handle(request, cancellationToken);
                return Results.Ok(payments);
            })
                .WithName("Get Repayments By Loan ID")
                .WithSummary("Get repayments by loan ID")
                .WithDescription("Retrieves all the payments made to a given loan");

            //query for a specific payment
            app.MapGet("{id:guid}", async (
                Guid id, 
                PaymentsDbContext dbContext,
                [FromServices] IQueryHandler<GetSingleRepaymentQuery, PaymentSummaryDTO> handler,
                CancellationToken cancellationToken) =>
            {
                var request = new GetSingleRepaymentQuery { PaymentId = id };
                var paymentSummary = await handler.Handle(request, cancellationToken);

                return paymentSummary is not null ? Results.Ok(paymentSummary) : Results.NotFound();
            })
                .WithName("Get Repayment By ID")
                .WithSummary("Get repayment by ID")
                .WithDescription("Retrieves a specific payment by its ID.");
        }
    }
}
