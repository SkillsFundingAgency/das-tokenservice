﻿namespace SFA.DAS.TokenService.Domain;

public class OAuthAccessToken
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Scope { get; set; }
    public string? TokenType { get; set; }
}