using System.Text.RegularExpressions;

namespace eTasks_server.Core.Helpers
{
    /// <summary>
    /// Classe Helper para salvar Fotos dos usuários
    /// </summary>
    public static class UserPhotoStorage
    {
        /// <summary>
        /// Caminho das imagens
        /// </summary>
        private const string RelativeDirectory = "uploads/profiles";

        /// <summary>
        /// Ler dados como Base64
        /// </summary>
        /// <param name="relativePath">Caminho dos arquivos</param>
        /// <returns></returns>
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

        /// <summary>
        /// Salva imagens base64 em arquivos para dentro da pasta definida
        /// </summary>
        /// <param name="base64Payload">Foto em base 64</param>
        /// <param name="currentRelativePath">Caminho da pasta a salvar</param>
        /// <param name="cancellationToken">Token para cancelar</param>
        /// <returns></returns>
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

        /// <summary>
        /// Função para apagar arquivo
        /// </summary>
        /// <param name="relativePath">Caminho relativo</param>
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

        /// <summary>
        /// Resolver caminho absoluto
        /// </summary>
        /// <param name="relativePath">Caminho relativo</param>
        /// <returns></returns>
        private static string? ResolveAbsolutePath(string? relativePath)
        {
            return ResolveCandidatePaths(relativePath).FirstOrDefault(File.Exists);
        }

        /// <summary>
        /// Função que obtem o candidato mais provável para caminho absoluto
        /// </summary>
        /// <param name="relativePath">Caminho relativo</param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtem pasta rais do armazenamento
        /// </summary>
        /// <returns></returns>
        private static string GetStorageRoot()
        {
            var contentRoot = Directory.GetCurrentDirectory();
            var protectedRoot = Directory.GetParent(contentRoot)?.FullName ?? contentRoot;

            return Path.GetFullPath(Path.Combine(protectedRoot, "uploads", "profiles"));
        }

        /// <summary>
        /// Obtem pasta raiz legada
        /// </summary>
        /// <returns></returns>
        private static string GetLegacyStorageRoot()
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "uploads", "profiles"));
        }

        /// <summary>
        /// Verifica se está abaixo do caminho raiz
        /// </summary>
        /// <param name="path">caminho</param>
        /// <param name="root">raiz</param>
        /// <returns></returns>
        private static bool IsUnderRoot(string path, string root)
        {
            var fullPath = Path.GetFullPath(path);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parsear base 64
        /// </summary>
        /// <param name="payload">Conteundo base 64</param>
        /// <returns></returns>
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
