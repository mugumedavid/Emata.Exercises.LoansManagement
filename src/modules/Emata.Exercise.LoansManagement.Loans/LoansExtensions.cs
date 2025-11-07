using System.Reflection;
using System.Windows.Input;
using Emata.Exercise.LoansManagement.Contracts.Loans;
using Emata.Exercise.LoansManagement.Contracts.Loans.DTOs;
using Emata.Exercise.LoansManagement.Loans.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Loans.Presentation;
using Emata.Exercise.LoansManagement.Loans.UseCases;
using Emata.Exercise.LoansManagement.Shared;
using Emata.Exercise.LoansManagement.Shared.Endpoints;
using Emata.Exercise.LoansManagement.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emata.Exercise.LoansManagement.Loans;

public static class LoansExtensions
{

    internal const string ModuleName = "Loans";

    public static IServiceCollection AddLoansModule(this IServiceCollection services,
        IConfiguration configuration,
        List<Assembly> mediatorAssemblies)
    {
        //register module assembly for mediator handlers
        mediatorAssemblies.Add(typeof(AddLoanCommandHandler).Assembly);

        //database context registration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<LoansDbContext>(options =>
        {
            options.UseNpgsql(connectionString, dbOptions =>
            {
                dbOptions.MigrationsHistoryTable("__EFMigrationsHistory", ModuleName);
            });
        });

    // application services
    services.AddScoped<ICommandHandler<AddLoanCommand, LoanItem>, AddLoanCommandHandler>();
    services.AddScoped<IQueryHandler<GetLoansQuery, List<LoanItemDetails>>, GetLoansQueryHandler>();
    services.AddScoped<IQueryHandler<GetLoanByIdQuery, LoanItem?>, GetLoanByIdQueryHandler>();
    services.AddScoped<ILoanService, LoanService>();

    //register endpoints...
    services.AddEndpoints(typeof(LoansExtensions).Assembly);

        return services;
    }

    public static Task MigrateLoansDatabaseAsync(this IApplicationBuilder app, CancellationToken cancellationToken = default) 
        => app.MigrateDatabaseAsync<LoansDbContext>(cancellationToken);
}