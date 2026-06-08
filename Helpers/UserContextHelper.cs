using Microsoft.AspNetCore.Mvc;

namespace Gestion_SalleClasseEDT.Helpers
{
    public class UserContext
    {
        public int? UserId { get; set; }
        public string? Role { get; set; }
        public bool IsAuthenticated => UserId.HasValue && !string.IsNullOrEmpty(Role);
    }

    public static class UserContextHelper
    {
        public static UserContext FromRequest(ActionContext actionContext)
        {
            var headers = actionContext.HttpContext.Request.Headers;

            int? userId = null;
            if (headers.TryGetValue("X-User-Id", out var userIdVal))
            {
                if (int.TryParse(userIdVal.ToString(), out int parsed))
                    userId = parsed;
            }

            string? role = null;
            if (headers.TryGetValue("X-User-Role", out var roleVal))
                role = roleVal.ToString()?.ToLowerInvariant();

            return new UserContext { UserId = userId, Role = role };
        }
    }
}
