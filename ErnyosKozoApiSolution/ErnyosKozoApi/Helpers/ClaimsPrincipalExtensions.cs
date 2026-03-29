using System.Security.Claims;

namespace ErnyosKozoApi.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("isAdmin")?.Value;
            return bool.TryParse(claim, out var isAdmin) && isAdmin;
        }

        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(value, out var id))
                return id;

            return null;
        }
    }
}