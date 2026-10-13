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
builder.Services.AddScoped<ParkService>();
builder.Services.AddScoped<ShoppingService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
