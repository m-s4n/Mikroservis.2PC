using Coordinator.Models.Contexts;
using Coordinator.Services;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDb")));

// Micro servislerin iletiþim bilgileri httpclient kaydý olarak tutulacak
builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new("http://localhost:5001"));
builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("http://localhost:5002"));
builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("http://localhost:5003"));

// IoC
builder.Services.AddTransient<ITransactionService, TransactionService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapGet("/create-order-transaction", async (ITransactionService transactionService) =>
{
    // Phase 1
    var transactionId = await transactionService.CreateTransactionAsync();
    await transactionService.PrepareServicesAsync(transactionId);
    bool transactionState = await transactionService.CheckReadyServicesAsync(transactionId);

    // Phase 2
    if (transactionState)
    {
        
        await transactionService.CommitAsync(transactionId);
        transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);
    }

    if (!transactionState)
    {
        await transactionService.RollbackAsync(transactionId);
    }

});



app.Run();


