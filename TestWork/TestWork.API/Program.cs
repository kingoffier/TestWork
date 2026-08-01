using Microsoft.EntityFrameworkCore;
using TestWork.API.Extensions;
using TestWork.Application.Services;
using TestWork.Core.Interfaces.Subscription;
using TestWork.Infrastructure;
using TestWork.Infrastructure.Data;
using TestWork.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiDocumentation();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TestWorkContext>(options => options.UseSqlServer(connectionString));

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<PriceTrackingOptions>(builder.Configuration.GetSection("PriceTracking"));

builder.Services.AddHttpClient("Prinzip", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PriceTracker/1.0");
});

builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IApartmentPriceParser, ApartmentPriceParser>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<PriceCheckerBackgroundService>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    TestWorkContext context = scope.ServiceProvider.GetRequiredService<TestWorkContext>();
    context.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
