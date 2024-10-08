using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.TokenService.Api.StartupExtensions;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<PrivilegedAccessQuery>());
builder.Services.AddConfigurationOptions(builder.Configuration);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
});

builder.Services.AddActiveDirectoryAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

// builder.Services.AddApiAuthentication(builder.Configuration);
// builder.Services.AddApiAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHttpsRedirection();

await app.RunAsync();