﻿namespace SFA.DAS.TokenService.Infrastructure.Configuration;

public record OAuthTokenServiceConfiguration
{
    public string? Url { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}