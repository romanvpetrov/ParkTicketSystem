using System.Diagnostics;
using System.Net;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry;
using TicketSystem.Domain.Repositories;
using TicketSystem.Infrastructure.Parks;
using TicketSystem.Infrastructure.Products;
using TicketSystem.Infrastructure.Shopping;
using TicketSystem.Services.Parks;
using TicketSystem.Services.Shopping;
using AspNetCoreIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("10.0.0.0"), 8));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("172.16.0.0"), 12));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("192.168.0.0"), 16));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("169.254.0.0"), 16));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("::ffff:10.0.0.0"), 104));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("::ffff:172.16.0.0"), 108));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("::ffff:192.168.0.0"), 112));
    options.KnownNetworks.Add(new AspNetCoreIPNetwork(IPAddress.Parse("::ffff:169.254.0.0"), 112));
});

if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    var samplingRatio = Math.Clamp(
        builder.Configuration.GetValue<float?>("AzureMonitor:SamplingRatio") ?? 1.0F,
        0.0F,
        1.0F);

    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.SamplingRatio = samplingRatio;
        options.TracesPerSecond = null;
    });
}

var dataDirectory = builder.Configuration["DataDirectory"];

if (string.IsNullOrWhiteSpace(dataDirectory))
{
    if (builder.Environment.IsDevelopment())
    {
        dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    }
    else
    {
        var homeDirectory = Environment.GetEnvironmentVariable("HOME");

        if (string.IsNullOrWhiteSpace(homeDirectory))
        {
            throw new InvalidOperationException(
                "DataDirectory must be configured when persistent HOME storage is unavailable.");
        }

        dataDirectory = Path.Combine(homeDirectory, "data", "park-ticket-system");
    }
}

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<IParkRepository>(serviceProvider => new JsonParkRepository(
    dataDirectory,
    serviceProvider.GetRequiredService<IProductRepository>()));
builder.Services.AddSingleton<ICartRepository>(_ => new JsonCartRepository(dataDirectory));
builder.Services.AddSingleton<ITicketRepository>(_ => new JsonTicketRepository(dataDirectory));
builder.Services.AddSingleton<IEmailService>(_ =>
{
    var provider = builder.Configuration["Email:Provider"] ?? "Fake";

    return provider.ToLowerInvariant() switch
    {
        "fake" => new FakeEmailService(),
        "sendgrid" => new SendGridEmailService(
            builder.Configuration["Email:ApiKey"] 
            ?? throw new InvalidOperationException("Email:ApiKey is required for the SendGrid email provider.")
        ),
        _ => throw new InvalidOperationException($"Unknown email provider '{provider}'.")
    };
});
builder.Services.AddSingleton(_ => new TicketEmailService(
    builder.Configuration["Email:SenderAddress"]
        ?? throw new InvalidOperationException("Email:SenderAddress is required.")));
builder.Services.AddScoped<ParkService>();
builder.Services.AddScoped<ShoppingService>();

var app = builder.Build();

app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    var clientAddress = context.Connection.RemoteIpAddress;
    if (clientAddress?.IsIPv4MappedToIPv6 == true)
    {
        clientAddress = clientAddress.MapToIPv4();
    }

    if (clientAddress is not null)
    {
        Activity.Current?.SetTag("client.address", clientAddress.ToString());
    }

    await next(context);
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
