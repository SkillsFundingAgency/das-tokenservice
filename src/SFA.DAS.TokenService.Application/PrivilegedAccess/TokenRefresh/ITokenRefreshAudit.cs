using System.Collections.Generic;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    /// <summary>
    ///     Provides a means for the auditing the token refresh attempts
    /// </summary>
    /// <remarks>
    ///     The motivation for this was to allow unit tests to verify the 
    ///     behaviour of the token refresher. 
    ///     In production the audit items are not preserved to avoid these
    ///     building up over long running processes.
    /// </remarks>
    public interface ITokenRefreshAudit
    {
        TokenRefreshAuditEntry CreateAuditEntry(OAuthAccessToken token);
        void RefreshStarted(TokenRefreshAuditEntry auditEntry);
        void RefreshEnded(TokenRefreshAuditEntry auditEntry);
        IEnumerable<TokenRefreshAuditEntry> AuditItems { get; }
    }
}