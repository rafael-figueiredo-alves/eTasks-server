using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.DTOs.Shopping.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Shopping
{
    internal static class ShoppingListEtagHelper
    {
        public static string BuildListEtag(IEnumerable<ShoppingListListItemResponse> lists, ListShoppingListsRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.IsFinalized?.ToString() ?? string.Empty).Append('|')
                .Append(request.Type?.ToString() ?? string.Empty).Append('|')
                .Append(request.Place ?? string.Empty).Append('|')
                .Append(request.SearchTerm ?? string.Empty);

            foreach (var list in lists.OrderBy(x => x.Name).ThenBy(x => x.Id))
            {
                builder.Append('|')
                    .Append(list.Id)
                    .Append('|').Append(list.Name)
                    .Append('|').Append(list.TotalItems)
                    .Append('|').Append(list.TotalAmount)
                    .Append('|').Append(list.IsFinalized)
                    .Append('|').Append(list.UpdatedAt?.Ticks ?? list.CreatedAt.Ticks);
            }

            return BuildHash(builder.ToString());
        }

        public static string BuildDetailsEtag(ShoppingListDetailsResponse list)
        {
            var builder = new StringBuilder();
            builder.Append(list.Id).Append('|')
                .Append(list.UserUid).Append('|')
                .Append(list.Name).Append('|')
                .Append(list.Place ?? string.Empty).Append('|')
                .Append(list.Type).Append('|')
                .Append(list.TotalItems).Append('|')
                .Append(list.TotalAmount).Append('|')
                .Append(list.IsFinalized).Append('|')
                .Append(list.CreatedAt.Ticks).Append('|')
                .Append(list.UpdatedAt?.Ticks ?? 0);

            foreach (var item in list.Items.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            {
                builder.Append('|')
                    .Append(item.Id)
                    .Append('|').Append(item.Description)
                    .Append('|').Append(item.Unit)
                    .Append('|').Append(item.Quantity)
                    .Append('|').Append(item.UnitPrice)
                    .Append('|').Append(item.TotalAmount)
                    .Append('|').Append(item.IsCompleted);
            }

            return BuildHash(builder.ToString());
        }

        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
