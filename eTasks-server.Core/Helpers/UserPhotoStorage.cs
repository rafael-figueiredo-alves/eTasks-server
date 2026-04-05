using System.Text.RegularExpressions;

namespace eTasks_server.Core.Helpers
{
    public static class UserPhotoStorage
    {
        private const string RelativeDirectory = "uploads/profiles";

        public static async Task<string?> ReadAsBase64Async(string? relativePath)
        {
            var absolutePath = ResolveAbsolutePath(relativePath);
            if (absolutePath is null || !File.Exists(absolutePath))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(absolutePath);
            return Convert.ToBase64String(bytes);
        }

        public static async Task<string> SaveAsync(string base64Payload, string? currentRelativePath, CancellationToken cancellationToken = default)
        {
            var (bytes, extension) = ParseBase64(base64Payload);
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "profiles");
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var absolutePath = Path.Combine(directory, fileName);

            await File.WriteAllBytesAsync(absolutePath, bytes, cancellationToken);
            Delete(currentRelativePath);

            return $"{RelativeDirectory}/{fileName}";
        }

        public static void Delete(string? relativePath)
        {
            var absolutePath = ResolveAbsolutePath(relativePath);
            if (absolutePath is null || !File.Exists(absolutePath))
            {
                return;
            }

            File.Delete(absolutePath);
        }

        private static string? ResolveAbsolutePath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            if (!normalized.StartsWith($"{RelativeDirectory}/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var uploadsRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "uploads", "profiles"));
            var absolutePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), normalized.Replace('/', Path.DirectorySeparatorChar)));

            return absolutePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
                ? absolutePath
                : null;
        }

        private static (byte[] Bytes, string Extension) ParseBase64(string payload)
        {
            var trimmedPayload = payload.Trim();
            var extension = ".jpg";
            var base64Data = trimmedPayload;

            var match = Regex.Match(trimmedPayload, @"^data:image/(?<type>[a-zA-Z0-9.+-]+);base64,(?<data>.+)$");
            if (match.Success)
            {
                extension = match.Groups["type"].Value.ToLowerInvariant() switch
                {
                    "png" => ".png",
                    "webp" => ".webp",
                    "gif" => ".gif",
                    "jpeg" => ".jpg",
                    "jpg" => ".jpg",
                    _ => ".jpg"
                };

                base64Data = match.Groups["data"].Value;
            }

            return (Convert.FromBase64String(base64Data), extension);
        }
    }
}
