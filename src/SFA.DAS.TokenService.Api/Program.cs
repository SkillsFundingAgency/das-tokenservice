using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.TokenService.Api.Extensions;
using SFA.DAS.TokenService.Api.StartupExtensions;
using SFA.DAS.TokenService.Application.PrivilegedAccess.GetPrivilegedAccessToken;

var builder = WebApplication.CreateBuilder(args);

var rootConfiguration = builder.Configuration.BuildDasConfiguration();

builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<PrivilegedAccessQuery>());
builder.Services.AddConfigurationOptions(rootConfiguration);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddApplicationInsights();
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
    
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(
        "Microsoft.AspNetCore.Mvc.Infrastructure", LogLevel.Warning);
});

builder.Services.AddActiveDirectoryAuthentication(rootConfiguration);
builder.Services.AddApplicationServices();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseAuthentication();
}

app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.UseHttpsRedirection();

await app.RunAsync();