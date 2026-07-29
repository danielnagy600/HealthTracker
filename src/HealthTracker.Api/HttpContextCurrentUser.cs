using System.Security.Claims;
using HealthTracker.SharedKernel.Abstractions;

namespace HealthTracker.Api;

/// <summary>
/// Az <see cref="ICurrentUser"/> megvalósítása: a bejelentkezett felhasználót a
/// HTTP-kéréshez tartozó claim-ekből olvassa ki. Ez az egyetlen hely, ami tud az
/// ASP.NET HttpContextről – a funkciómodulok csak az absztrakciót látják.
/// </summary>
public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var value = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
