using CineVerse.Domain;
using Microsoft.EntityFrameworkCore;

namespace CineVerse.Infrastructure;

internal static class AccountModel
{
    public static void Configure(ModelBuilder model)
    {
        var profiles = model.Entity<UserProfile>().ToTable("UserProfiles");
        profiles.HasKey(item => item.UserId);
        profiles.HasOne<ApplicationUser>().WithOne().HasForeignKey<UserProfile>(item => item.UserId);

        var sessions = model.Entity<UserSession>().ToTable("UserSessions",
            table => table.HasCheckConstraint("CK_UserSession_Expiry", "ExpiresAtUtc > CreatedAtUtc"));
        sessions.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId);
        sessions.HasIndex(item => item.ExpiresAtUtc);

        var plans = model.Entity<SubscriptionPlan>().ToTable("SubscriptionPlans", table =>
        {
            table.HasCheckConstraint("CK_SubscriptionPlan_Price", "PriceCents >= 0");
            table.HasCheckConstraint("CK_SubscriptionPlan_Duration", "DurationDays >= 0");
        });
        plans.HasIndex(item => item.Name).IsUnique();
        plans.HasData(
            new SubscriptionPlan(1, "Gratuito", "Plano de entrada", "Acesso ao catálogo", 0, 0, true),
            new SubscriptionPlan(2, "Premium Mensal", "Subscrição de 30 dias", "Acesso a conteúdos premium", 799, 30, true),
            new SubscriptionPlan(3, "Premium Anual", "Subscrição de 365 dias", "Acesso a conteúdos premium", 7999, 365, true));

        var subscriptions = model.Entity<Subscription>().ToTable("Subscriptions",
            table => table.HasCheckConstraint("CK_Subscription_Dates", "EndsAtUtc IS NULL OR EndsAtUtc > StartsAtUtc"));
        subscriptions.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId);
        subscriptions.HasOne<SubscriptionPlan>().WithMany().HasForeignKey(item => item.PlanId).OnDelete(DeleteBehavior.Restrict);

        var audits = model.Entity<AdminAuditLog>().ToTable("AdminAuditLogs");
        audits.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.AdminId).OnDelete(DeleteBehavior.Restrict);
        audits.HasIndex(item => item.CreatedAtUtc);

        model.Entity<AppSetting>().ToTable("AppSettings").HasKey(item => item.Key);
    }
}
