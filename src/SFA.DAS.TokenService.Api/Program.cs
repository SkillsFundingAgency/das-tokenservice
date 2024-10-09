using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.TokenService.Api.Extensions;
using SFA.DAS.TokenService.Api.StartupExtensions;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.BuildDasConfiguration();

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<PrivilegedAccessQuery>());
builder.Services.AddConfigurationOptions(builder.Configuration);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
});

builder.Services.AddActiveDirectoryAuthentication(rootConfiguration);
builder.Services.AddApplicationServices();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHttpsRedirection();

await app.RunAsync();