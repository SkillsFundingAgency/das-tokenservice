﻿using System;
using System.Collections.Generic;
using SFA.DAS.TokenService.Domain;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public class TokenRefreshAudit : ITokenRefreshAudit
    {
        private readonly List<TokenRefreshAuditEntry> _auditEntries;

        private readonly bool _maintainList;

        public TokenRefreshAudit(bool maintainList = false)
        {
            _maintainList = maintainList;
            if (maintainList)
            {
                _auditEntries = new List<TokenRefreshAuditEntry>();
            }
        }

        public TokenRefreshAuditEntry CreateAuditEntry(OAuthAccessToken token)
        {
            var result = new TokenRefreshAuditEntry
            {
                ExpirationTime = token.ExpiresAt
            };

            if (_maintainList)
            {
                _auditEntries.Add(result);
            }

            return result;
        }

        public void StartRefresh(TokenRefreshAuditEntry auditEntry)
        {
            auditEntry.ActualRefreshStart = DateTime.UtcNow;
        }

        public void EndRefresh(TokenRefreshAuditEntry auditEntry)
        {
            auditEntry.ActualRefreshEnd = DateTime.UtcNow;
        }

        public IEnumerable<TokenRefreshAuditEntry> AuditItems => _auditEntries;
    }
}