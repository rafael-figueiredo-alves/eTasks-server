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
            var directory = GetStorageRoot();
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var absolutePath = Path.Combine(directory, fileName);

            await File.WriteAllBytesAsync(absolutePath, bytes, cancellationToken);
            Delete(currentRelativePath);

            return $"{RelativeDirectory}/{fileName}";
        }

        public static void Delete(string? relativePath)
        {
            foreach (var absolutePath in ResolveCandidatePaths(relativePath))
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
        }

        private static string? ResolveAbsolutePath(string? relativePath)
        {
            return ResolveCandidatePaths(relativePath).FirstOrDefault(File.Exists);
        }

        private static IEnumerable<string> ResolveCandidatePaths(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                yield break;
            }

            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            if (!normalized.StartsWith($"{RelativeDirectory}/", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            var fileName = Path.GetFileName(normalized);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            var storageRoot = GetStorageRoot();
            var storagePath = Path.GetFullPath(Path.Combine(storageRoot, fileName));
            if (IsUnderRoot(storagePath, storageRoot))
            {
                yield return storagePath;
            }

            var legacyRoot = GetLegacyStorageRoot();
            var legacyPath = Path.GetFullPath(Path.Combine(legacyRoot, fileName));
            if (IsUnderRoot(legacyPath, legacyRoot))
            {
                yield return legacyPath;
            }
        }

        private static string GetStorageRoot()
        {
            var contentRoot = Directory.GetCurrentDirectory();
            var protectedRoot = Directory.GetParent(contentRoot)?.FullName ?? contentRoot;

            return Path.GetFullPath(Path.Combine(protectedRoot, "uploads", "profiles"));
        }

        private static string GetLegacyStorageRoot()
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "uploads", "profiles"));
        }

        private static bool IsUnderRoot(string path, string root)
        {
            var fullPath = Path.GetFullPath(path);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
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
