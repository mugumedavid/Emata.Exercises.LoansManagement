using System.Reflection;
using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Emata.Exercise.LoansManagement.Repayments.Domain;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Repayments.UseCases;
using Emata.Exercise.LoansManagement.Shared;
using Emata.Exercise.LoansManagement.Shared.Endpoints;
using Emata.Exercise.LoansManagement.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emata.Exercise.LoansManagement.Repayments;

public static class RepaymentsExtensions
{

    internal const string ModuleName = "Payments";

    public static IServiceCollection AddRepaymentsModule(this IServiceCollection services,
        IConfiguration configuration,
        List<Assembly> mediatorAssemblies)
    {
        //register module assembly for mediator handlers
        mediatorAssemblies.Add(typeof(AddRepaymentCommandHandler).Assembly);

        //database context registration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<PaymentsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, dbOptions =>
            {
                dbOptions.MigrationsHistoryTable("__EFMigrationsHistory", ModuleName);
            });
        });

        // application services
        services.AddScoped<ICommandHandler<AddPaymentCommand, PaymentSummaryDTO>, AddRepaymentCommandHandler>();
        services.AddScoped<IQueryHandler<GetRepaymentsQuery, List<PaymentSummaryDTO>>, GetRepaymentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSingleRepaymentQuery, PaymentSummaryDTO>, GetSingleRepaymentQueryHandler>();
        services.AddScoped<ILoanCalculator, LoanCalculator>();

        //register endpoints...
        services.AddEndpoints(typeof(RepaymentsExtensions).Assembly);

        return services;
    }

    public static Task MigrateRepaymentsDatabaseAsync(this IApplicationBuilder app, CancellationToken cancellationToken = default)
        => app.MigrateDatabaseAsync<PaymentsDbContext>(cancellationToken);
}