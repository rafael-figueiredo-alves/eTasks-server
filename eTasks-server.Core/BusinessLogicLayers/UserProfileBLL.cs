using System.Net;
using System.Text;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Helpers;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public class UserProfileBLL : IUserProfileBLL
    {
        private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase) { "light", "dark" };
        private static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "pt", "en" };

        private readonly AppDbContext _context;
        private readonly ILogger<UserProfileBLL> _logger;

        public UserProfileBLL(AppDbContext context, ILogger<UserProfileBLL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfileResponse> GetProfileAsync(Guid userUid)
        {
            var user = await GetActiveUserAsync(userUid);
            await EnsureSettingsAsync(user);
            await TouchLastAccessAsync(user);
            return await BuildProfileResponseAsync(user);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        {
            var user = await GetActiveUserAsync(userUid);
            await EnsureSettingsAsync(user);

            var normalizedEmail = request.Email.Trim();
            var emailInUse = await _context.Users.AnyAsync(
                x => x.Uid != userUid && !x.IsDeleted && x.Email == normalizedEmail,
                cancellationToken);

            if (emailInUse)
            {
                throw new ValidationException("Email", "O e-mail informado ja esta cadastrado.");
            }

            user.Name = request.Name.Trim();
            user.Email = normalizedEmail;

            if (request.RemovePhoto)
            {
                UserPhotoStorage.Delete(user.PhotoPath);
                user.PhotoPath = null;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                user.PhotoPath = await UserPhotoStorage.SaveAsync(request.PhotoBase64, user.PhotoPath, cancellationToken);
            }

            await TouchLastAccessAsync(user, saveChanges: false);
            await _context.SaveChangesAsync(cancellationToken);
            return await BuildProfileResponseAsync(user);
        }

        public async Task<UserSettingsDTO> PatchSettingsAsync(Guid userUid, PatchUserSettingsRequest request)
        {
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);

            if (!string.IsNullOrWhiteSpace(request.Theme))
            {
                ValidateTheme(request.Theme);
                settings.Theme = request.Theme.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(request.Language))
            {
                ValidateLanguage(request.Language);
                settings.Language = request.Language.Trim().ToLowerInvariant();
            }

            if (request.UseCamera.HasValue)
            {
                settings.UseCamera = request.UseCamera.Value;
            }

            if (request.EnableBonusSystem.HasValue)
            {
                settings.EnableBonusSystem = request.EnableBonusSystem.Value;
            }

            settings.UpdatedAt = SaoPauloDateTime.Now();

            await TouchLastAccessAsync(user, saveChanges: false);
            await _context.SaveChangesAsync();

            return new UserSettingsDTO
            {
                Theme = settings.Theme,
                Language = settings.Language,
                UseCamera = settings.UseCamera,
                EnableBonusSystem = settings.EnableBonusSystem
            };
        }

        public async Task<string> ExportProfileCsvAsync(Guid userUid)
        {
            var profile = await GetProfileAsync(userUid);
            var builder = new StringBuilder();
            builder.AppendLine("Uid,Name,Email,CreatedAt,LastAccessAt,Theme,Language,UseCamera,EnableBonusSystem,TotalBonusPoints,Achievements");
            builder.Append(Escape(profile.Uid.ToString())).Append(',');
            builder.Append(Escape(profile.Name)).Append(',');
            builder.Append(Escape(profile.Email)).Append(',');
            builder.Append(Escape(profile.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))).Append(',');
            builder.Append(Escape(profile.LastAccessAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty)).Append(',');
            builder.Append(Escape(profile.Settings.Theme)).Append(',');
            builder.Append(Escape(profile.Settings.Language)).Append(',');
            builder.Append(Escape(profile.Settings.UseCamera.ToString())).Append(',');
            builder.Append(Escape(profile.Settings.EnableBonusSystem.ToString())).Append(',');
            builder.Append(Escape(profile.Bonus.TotalPoints.ToString())).Append(',');
            builder.Append(Escape(string.Join(" | ", profile.Bonus.Achievements.Select(x => $"{x.Name}:{x.PointsRequired}"))));
            builder.AppendLine();

            return builder.ToString();
        }

        public async Task SoftDeleteAsync(Guid userUid)
        {
            var user = await GetActiveUserAsync(userUid);
            if (user.IsAdmin)
            {
                throw new ApiException(HttpStatusCode.Forbidden, "Nao e permitido excluir uma conta administrativa por esta rota.");
            }

            user.IsDeleted = true;
            user.DeletedAt = SaoPauloDateTime.Now();
            user.IsBlocked = true;
            user.LastAccessAt = user.DeletedAt;

            var activeTokens = await _context.RefreshTokens
                .Where(x => x.UserUid == userUid && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuario {Uid} removido logicamente.", userUid);
        }

        private async Task<User> GetActiveUserAsync(Guid userUid)
        {
            var user = await _context.Users
                .Include(x => x.Settings)
                .Include(x => x.BonusPoints)
                .Include(x => x.Achievements)
                    .ThenInclude(x => x.BonusAchievement)
                .FirstOrDefaultAsync(x => x.Uid == userUid && !x.IsDeleted);

            if (user is null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "Usuario nao encontrado.");
            }

            return user;
        }

        private async Task<UserSettings> EnsureSettingsAsync(User user)
        {
            if (user.Settings is not null)
            {
                return user.Settings;
            }

            var settings = new UserSettings
            {
                UserUid = user.Uid
            };

            user.Settings = settings;
            await _context.UserSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        private async Task<UserProfileResponse> BuildProfileResponseAsync(User user)
        {
            var photoBase64 = await UserPhotoStorage.ReadAsBase64Async(user.PhotoPath);

            return new UserProfileResponse
            {
                Uid = user.Uid,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                LastAccessAt = user.LastAccessAt,
                PhotoBase64 = photoBase64,
                Settings = new UserSettingsDTO
                {
                    Theme = user.Settings?.Theme ?? "light",
                    Language = user.Settings?.Language ?? "pt",
                    UseCamera = user.Settings?.UseCamera ?? false,
                    EnableBonusSystem = user.Settings?.EnableBonusSystem ?? false
                },
                Bonus = new UserBonusSummaryDTO
                {
                    TotalPoints = user.BonusPoints.Sum(x => x.Points),
                    Achievements = user.Achievements
                        .Where(x => x.BonusAchievement is not null)
                        .OrderByDescending(x => x.AchievedAt)
                        .Select(x => new UserAchievementDTO
                        {
                            Code = x.BonusAchievement!.Code,
                            Name = x.BonusAchievement.Name,
                            PointsRequired = x.BonusAchievement.PointsRequired,
                            DisplayType = (int)x.BonusAchievement.DisplayType,
                            AchievedAt = x.AchievedAt
                        })
                        .ToList()
                }
            };
        }

        private async Task TouchLastAccessAsync(User user, bool saveChanges = true)
        {
            user.LastAccessAt = SaoPauloDateTime.Now();
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private static void ValidateTheme(string theme)
        {
            if (!AllowedThemes.Contains(theme.Trim()))
            {
                throw new ValidationException("Theme", "Tema invalido. Valores aceitos: light ou dark.");
            }
        }

        private static void ValidateLanguage(string language)
        {
            if (!AllowedLanguages.Contains(language.Trim()))
            {
                throw new ValidationException("Language", "Idioma invalido. Valores aceitos: pt ou en.");
            }
        }

        private static string Escape(string value)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
