using TicketSystem.Domain.Repositories;
using TicketSystem.Infrastructure.Parks;
using TicketSystem.Infrastructure.Shopping;
using TicketSystem.Services.Parks;
using TicketSystem.Services.Shopping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<InMemoryParkRepository>();
builder.Services.AddSingleton<IParkRepository>(serviceProvider => serviceProvider.GetRequiredService<InMemoryParkRepository>());
builder.Services.AddSingleton<ICartRepository, InMemoryCartRepository>();
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();
builder.Services.AddSingleton<IEmailService>(_ =>
{
    var provider = builder.Configuration["Email:Provider"] ?? "Fake";

    return provider.ToLowerInvariant() switch
    {
        "fake" => new FakeEmailService(),
        "sendgrid" => new SendGridEmailService(
            builder.Configuration["Email:ApiKey"]
                ?? throw new InvalidOperationException("Email:ApiKey is required for the SendGrid email provider.")),
        _ => throw new InvalidOperationException($"Unknown email provider '{provider}'.")
    };
});
builder.Services.AddSingleton(_ => new TicketEmailService(
    builder.Configuration["Email:SenderAddress"]
        ?? throw new InvalidOperationException("Email:SenderAddress is required.")));
builder.Services.AddScoped<ParkService>();
builder.Services.AddScoped<ShoppingService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
