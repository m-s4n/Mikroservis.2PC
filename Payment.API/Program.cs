var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();


app.MapGet("/ready", () =>
{
    Console.WriteLine("Payment service is ready");
    return true;
});

app.MapGet("/commit", () =>
{
    Console.WriteLine("Payment service is commited");
    return true;
});

app.MapGet("/rollback", () =>
{
    Console.WriteLine("Paymen service is rollback");
});


app.Run();

