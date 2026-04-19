using eTasks_server.Models.Exceptions;
using System.Net;
using System.Security.Claims;

namespace eTasks_server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetRequiredUserUid(this ClaimsPrincipal user)
        {
            var rawUid = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(rawUid, out var userUid))
            {
                return userUid;
            }

            throw new ApiException(HttpStatusCode.Unauthorized, "Token JWT invalido ou sem identificacao do usuario.");
        }
    }
}
