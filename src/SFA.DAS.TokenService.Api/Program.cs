using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.TokenService.Api.StartupExtensions;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;
using SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;
using SFA.DAS.TokenService.Domain.Data;
using SFA.DAS.TokenService.Domain.Services;
using SFA.DAS.TokenService.Infrastructure.Data;
using SFA.DAS.TokenService.Infrastructure.ExecutionPolicies;
using SFA.DAS.TokenService.Infrastructure.Http;
using SFA.DAS.TokenService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
