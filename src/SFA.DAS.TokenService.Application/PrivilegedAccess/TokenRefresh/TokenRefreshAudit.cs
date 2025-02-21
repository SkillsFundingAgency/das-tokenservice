using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh;

public class TokenRefreshAudit(bool maintainList = false) : ITokenRefreshAudit
{
    private readonly List<TokenRefreshAuditEntry>? _auditEntries = maintainList ? [] : null;

    public TokenRefreshAuditEntry CreateAuditEntry(OAuthAccessToken token)
    {
        var result = new TokenRefreshAuditEntry
        {
            ExpirationTime = token.ExpiresAt
        };

        _auditEntries?.Add(result);
        return result;
    }

    public void RefreshStarted(TokenRefreshAuditEntry auditEntry)
    {
        if (auditEntry is not null)
        {
            auditEntry.ActualRefreshStart = DateTime.UtcNow;
        }
    }

    public void RefreshEnded(TokenRefreshAuditEntry auditEntry)
    {
        if (auditEntry is not null)
        {
            auditEntry.ActualRefreshEnd = DateTime.UtcNow;
        }
    }

    public IEnumerable<TokenRefreshAuditEntry> AuditItems => _auditEntries ?? [];
}