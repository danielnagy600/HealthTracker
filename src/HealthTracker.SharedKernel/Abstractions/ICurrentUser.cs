namespace HealthTracker.SharedKernel.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated => UserId is not null;

    Guid RequireUserId() =>
        UserId ?? throw new InvalidOperationException("Nincs bejelentkezett felhasználó.");
}
