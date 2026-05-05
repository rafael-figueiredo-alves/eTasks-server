using eTasks_server.Models.Exceptions;
using System.Net;
using System.Security.Claims;

namespace eTasks_server.Extensions
{
    /// <summary>
    /// Extensões para a classe ClaimsPrincipal, que representa o usuário autenticado e suas reivindicações (claims) em um contexto de segurança.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        extension(ClaimsPrincipal user)
        {
            /// <summary>
            /// Obtém o identificador único do usuário a partir das claims. Lança uma exceção se o identificador não estiver presente ou for inválido.
            /// </summary>
            /// <param name="user">Usuário autenticado</param>
            /// <returns>Identificador único do usuário</returns>
            /// <exception cref="ApiException">Lançada quando o token JWT é inválido ou não contém a identificação do usuário</exception>
            public Guid GetRequiredUserUid()
            {
                //Captura o UserId do usuário a partir das claims, utilizando o tipo de claim NameIdentifier, que é comumente usado para armazenar o identificador do usuário.
                var rawUid = user.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (Guid.TryParse(rawUid, out var userUid))
                {
                    return userUid;
                }

                throw new ApiException(HttpStatusCode.Unauthorized, "Token JWT inválido ou sem identificação do usuário.");
            }
        }
    }
}
