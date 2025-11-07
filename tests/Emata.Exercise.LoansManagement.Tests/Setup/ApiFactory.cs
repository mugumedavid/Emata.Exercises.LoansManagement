using System.Data.Common;
using Emata.Exercise.LoansManagement.API;
using Emata.Exercise.LoansManagement.Borrowers;
using Emata.Exercise.LoansManagement.Borrowers.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Loans;
using Emata.Exercise.LoansManagement.Loans.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Repayments;
using Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data;
using Emata.Exercise.LoansManagement.Tests.Borrowers;
using Emata.Exercise.LoansManagement.Tests.Loans;
using Emata.Exercise.LoansManagement.Tests.Repayments;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Refit;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace Emata.Exercise.LoansManagement.Tests.Setup;

public class ApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    public IBorrowersRefitApi BorrowersApi { get; private set; } = default!;
    public ILoansRefitApi LoansApi { get; private set; } = default!;
    public IRepaymentsRefitApi RepaymentsApi { get; private set; } = default!;

    private PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("LoansManagementTests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    private string _connectionString = string.Empty;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    private static RefitSettings DefaultRefitSettings => new()
    {
        CollectionFormat = CollectionFormat.Multi,
        ContentSerializer = new SystemTextJsonContentSerializer(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            })
    };

    private static string[] DbSchemas =>
    [
        BorrowersExtensions.ModuleName,
        LoansExtensions.ModuleName,
        RepaymentsExtensions.ModuleName
    ];

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();
        _dbConnection = new NpgsqlConnection(_connectionString);

        //initialize Refit APIs
        HttpClient httpClient = CreateClient();
        BorrowersApi = RestService.For<IBorrowersRefitApi>(httpClient, DefaultRefitSettings);
        LoansApi = RestService.For<ILoansRefitApi>(httpClient, DefaultRefitSettings);
        RepaymentsApi = RestService.For<IRepaymentsRefitApi>(httpClient, DefaultRefitSettings);

        //initialize database schemas
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = DbSchemas,
            TablesToIgnore = [.. DbSchemas.Select(sch => new Table(sch, "__EFMigrationsHistory"))]
        });

    }

    public new async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _postgresContainer.DisposeAsync();

    }

    public Task ResetDatabaseAsync()
    {
        return _respawner.ResetAsync(_dbConnection);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            //replace module db contexts with test container connection string
            ReplaceDbContext<BorrowersDbContext>(services, _connectionString);
            ReplaceDbContext<LoansDbContext>(services, _connectionString);
            ReplaceDbContext<PaymentsDbContext>(services, _connectionString);
        });

        return base.CreateHost(builder);
    }
    
    private void ReplaceDbContext<TDbContext>(IServiceCollection services, string connectionString)
      where TDbContext : DbContext
   {
      ServiceDescriptor? descriptor =
         services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
      if (descriptor != null)
      {
         services.Remove(descriptor);
      }

      //add the test db context
      services.AddDbContext<TDbContext>(options => { options.UseNpgsql(connectionString); });
   }
}