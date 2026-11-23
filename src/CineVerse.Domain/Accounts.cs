namespace CineVerse.Domain;

public sealed record UserProfile(string UserId, string DisplayName, string? PhotoUrl = null);
public sealed record UserSession(Guid Id, string UserId, DateTime CreatedAtUtc, DateTime ExpiresAtUtc,
    DateTime? RevokedAtUtc = null);
public sealed record SubscriptionPlan(int Id, string Name, string Description, string Benefits,
    int PriceCents, int DurationDays, bool IsActive);
public sealed record Subscription(Guid Id, string UserId, int PlanId, DateTime StartsAtUtc,
    DateTime? EndsAtUtc, DateTime? CancelledAtUtc = null);
public sealed record AdminAuditLog(long Id, string AdminId, string Action, string Target, DateTime CreatedAtUtc);
public sealed record AppSetting(string Key, string Value);
