using System.Collections.Concurrent;
using System.Threading.Channels;
using FluentValidation;
using ReceiptProcessor.Filters;
using ReceiptProcessor.Models;
using ReceiptProcessor.Repositories;
using ReceiptProcessor.Services;
using ReceiptProcessor.Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IReceiptRepository, ReceiptRepository>();
builder.Services.AddSingleton<IReceiptServices, ReceiptServices>();
builder.Services.AddSingleton<ConcurrentDictionary<Guid, ReceiptProcessingStatus>>();
builder.Services.AddHostedService<ReceiptScoringService>();
builder.Services.AddSingleton(_ =>
{
    var channel = Channel.CreateBounded<ReceiptProcessorTask>(new BoundedChannelOptions(25)
    {
        //setting this for back pressue.
        FullMode = BoundedChannelFullMode.Wait
    });
    return channel;
});

builder.Services.AddValidatorsFromAssemblyContaining<ReceiptValidator>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/receipts/process", async (Receipt receipt, IReceiptServices service,
    ConcurrentDictionary<Guid, ReceiptProcessingStatus> status, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Saving a receipt.");
        var receiptId = await service.SaveReceipt(receipt);
        logger.LogInformation($"Receipt {receiptId} saved.");

        return Results.Accepted("/receipts/" + receiptId + "/status", new { id = receiptId });
    }
    catch (Exception)
    {
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }

})
.WithName("PostReceipt")
.WithOpenApi()
.AddEndpointFilter<ValidationFilter<Receipt>>()  //this will perform basic validation on the JSON value
.ProducesValidationProblem();


app.MapGet("/receipts/{id}/status", async (Guid id, IReceiptServices service, ILogger<Program> logger) =>
{
    try
    {
        if (Guid.Empty == id || !Guid.TryParse(id.ToString(), out _))
        {
            return Results.BadRequest("Invalid receipt id.");
        }

        logger.LogInformation($"Checking the status of receipt {id}.");
        var status = await service.GetReceiptStatus(id);

        return Results.Ok(new { status = status.ToString() });
    }
    catch (KeyNotFoundException)
    {
        //log information because this is a handled exception and expected based on this application
        logger.LogInformation($"Receipt {id} and it's status was not found in the data.");

        return Results.NotFound($"Unable to find status for receipt {id}.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error while retrieving status for receipt {id}");
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetReceiptStatus")
.WithOpenApi();


app.MapGet("/receipts/{id}/points", async (Guid id, IReceiptServices service, ILogger<Program> logger) =>
{
    try
    {
        if (Guid.Empty == id || !Guid.TryParse(id.ToString(), out _))
        {
            return Results.BadRequest("Invalid receipt id.");
        }

        logger.LogInformation($"Getting the points for receipt {id}.");
        var receipt = await service.GetReceipt(id);
        return Results.Ok(new { points = receipt.points });
    }
    catch (KeyNotFoundException)
    {
        //log information because this is a handled exception and expected based on this application
        logger.LogInformation($"Receipt {id} was not found in the data.");

        return Results.NotFound($"Unable to find points for receipt {id}.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error while retrieving points for receipt {id}");
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetReceiptPoints")
.WithOpenApi();


app.Run();
