using Emata.Exercise.LoansManagement.Borrowers;
using Emata.Exercise.LoansManagement.Contracts.Exceptions;
using Emata.Exercise.LoansManagement.Loans;
using Emata.Exercise.LoansManagement.Repayments;
using Emata.Exercise.LoansManagement.Shared.Endpoints;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Enable DI validation (important for testing)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;      // catches scoped->singleton injection issues
    options.ValidateOnBuild = true;     // validates registrations when host builds
});

//We are using a modular architecture where each module is responsible for its own services registration
//Read more about modular monoliths here: 
// - https://www.milanjovanovic.tech/blog/what-is-a-modular-monolith
// - https://abp.io/architecture/modular-monolith
// - https://dev.to/xoubaman/modular-monolith-3fg1
builder.Services
    .AddBorrowersModule(builder.Configuration, [])
    .AddLoansModule(builder.Configuration, [])
    .AddRepaymentsModule(builder.Configuration, []);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        // Default
        var status = StatusCodes.Status500InternalServerError;
        var message = "An unexpected error occurred.";

        // Map your custom exceptions
        if (ex is LoansManagementNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            message = ex.Message;
        }
        else if (ex is LoansManagementValueException)
        {
            status = StatusCodes.Status400BadRequest;
            message = ex.Message;
        }

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(new
        {
            error = status == 404 ? "Record not found"
                  : status == 400 ? "Invalid data"
                  : "Server error",
            details = message
        });
    });
});

//migrate module databases
await app.MigrateBorrowersDatabaseAsync();
await app.MigrateLoansDatabaseAsync();
await app.MigrateRepaymentsDatabaseAsync();

app.Run();
