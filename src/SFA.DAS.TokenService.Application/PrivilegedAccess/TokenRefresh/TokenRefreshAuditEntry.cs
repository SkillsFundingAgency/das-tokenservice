using System;

namespace SFA.DAS.TokenService.Application.PrivilegedAccess.TokenRefresh
{
    public class TokenRefreshAuditEntry
    {
        public DateTime ExpirationTime { get; set; }
        public TimeSpan PlannedRefreshDelay { get; set; }
        public DateTime PlannedRefreshTime { get; set; }
        public DateTime? ActualRefreshStart { get; set; }
        public DateTime? ActualRefreshEnd { get; set; }
        public int RefreshAttemps { get; set; }
        public bool Overdue => ActualRefreshStart.HasValue && ActualRefreshStart.Value > ExpirationTime;
        public override string ToString()
        {
            var refreshTime = ActualRefreshStart?.ToString("HH:mm: ss.fff") ?? "n/a";

            return $"Expiration:{ExpirationTime:HH:mm:ss.fff} " +
                   $"Refresh:{PlannedRefreshDelay.TotalMilliseconds} ({PlannedRefreshTime:HH:mm:ss.fff}) " +
                   $"ActualRefresh:{refreshTime} " +
                   $"Refresh-attempts:{RefreshAttemps} " +
                   $"Overdue:{Overdue}";
        }
    }
}