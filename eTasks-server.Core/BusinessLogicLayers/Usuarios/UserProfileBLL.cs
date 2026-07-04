using System.Net;
using System.Text;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Helpers;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using eTasks_server.Models.Enums.Users;

namespace eTasks_server.Core.BusinessLogicLayers.Usuarios
{
    public class UserProfileBLL(
        AppDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        IServerSettingsProvider settingsProvider,
        ILogger<IUserProfileBLL> logger) : BaseBLL<IUserProfileBLL>(context, logger), IUserProfileBLL
    {
        private static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase) { "light", "dark" };
        private static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "pt-BR", "en-US" };

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

            if (request.InitialScreen.HasValue)
            {
                ValidateInitialScreen(request.InitialScreen.Value);
                settings.InitialScreen = request.InitialScreen.Value;
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
                InitialScreen = settings.InitialScreen,
                EnableBonusSystem = settings.EnableBonusSystem
            };
        }

        public async Task<UserSettingsSyncResponse> GetSettingsAsync(Guid userUid)
        {
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);
            await TouchLastAccessAsync(user);
            return MapSettings(settings);
        }

        public async Task<UserBonusSyncResponse> GetBonusAsync(Guid userUid)
        {
            var user = await GetActiveUserAsync(userUid);
            await EnsureSettingsAsync(user);
            await TouchLastAccessAsync(user);
            return BuildBonusSync(user);
        }

        public async Task<UserDataSyncResponse> SyncUserDataAsync(Guid userUid, SyncUserDataRequest request)
        {
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);
            await TouchLastAccessAsync(user);

            var settingsChanged = !request.Since.HasValue || settings.UpdatedAt > request.Since.Value;
            var lastBonusUpdateAt = GetLastBonusUpdatedAt(user);
            var bonusChanged = !request.Since.HasValue || lastBonusUpdateAt > request.Since.Value;

            return new UserDataSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Settings = settingsChanged ? MapSettings(settings) : null,
                Bonus = bonusChanged ? BuildBonusSync(user) : null
            };
        }

        public async Task<string> ExportProfileCsvAsync(Guid userUid)
        {
            var profile = await GetProfileAsync(userUid);
            var builder = new StringBuilder();
            builder.AppendLine("Uid,Name,Email,CreatedAt,LastAccessAt,Theme,Language,InitialScreen,EnableBonusSystem,TotalBonusPoints,Achievements");
            builder.Append(Escape(profile.Uid.ToString())).Append(',');
            builder.Append(Escape(profile.Name)).Append(',');
            builder.Append(Escape(profile.Email)).Append(',');
            builder.Append(Escape(profile.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))).Append(',');
            builder.Append(Escape(profile.LastAccessAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty)).Append(',');
            builder.Append(Escape(profile.Settings.Theme)).Append(',');
            builder.Append(Escape(profile.Settings.Language)).Append(',');
            builder.Append(Escape(profile.Settings.InitialScreen.ToString())).Append(',');
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

            var settings = await settingsProvider.GetCurrentAsync();
            var validityDays = settings.AccountReactivationCodeValidityDays is < 7 or > 90
                ? 30
                : settings.AccountReactivationCodeValidityDays;

            var previousCodes = await _context.AccountReactivationCodes
                .Where(x => x.UserUid == userUid && !x.IsUsed)
                .ToListAsync();

            foreach (var previousCode in previousCodes)
            {
                previousCode.IsUsed = true;
                previousCode.UsedAt = DateTime.UtcNow;
            }

            var reactivationCode = new AccountReactivationCode
            {
                UserUid = userUid,
                Code = GenerateReactivationCode(),
                ExpiresAt = DateTime.UtcNow.AddDays(validityDays)
            };

            await _context.AccountReactivationCodes.AddAsync(reactivationCode);

            var activeTokens = await _context.RefreshTokens
                .Where(x => x.UserUid == userUid && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuario {Uid} removido logicamente.", userUid);

            await emailService.SendAccountReactivationEmailAsync(
                user.Email,
                BuildAccountReactivationLink(reactivationCode.Code),
                reactivationCode.ExpiresAt);
        }

        private string BuildAccountReactivationLink(string code)
        {
            var baseUrl = configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
            var apiPath = configuration[Constants.ApiV2Path] ?? "/api/v2";
            return $"{(baseUrl + apiPath).TrimEnd('/')}/auth/recover-account?code={WebUtility.UrlEncode(code)}";
        }

        private static string GenerateReactivationCode()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        }

        private Task<User> GetActiveUserAsync(Guid userUid)
        {
            return GetAndValidateActiveUserAsync(userUid, query => query
                .Include(x => x.Settings)
                .Include(x => x.BonusPoints)
                .Include(x => x.Achievements)
                    .ThenInclude(x => x.BonusAchievement));
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

        private static UserSettingsSyncResponse MapSettings(UserSettings settings)
        {
            return new UserSettingsSyncResponse
            {
                Id = settings.Id,
                UserUid = settings.UserUid,
                Theme = settings.Theme,
                Language = settings.Language,
                InitialScreen = settings.InitialScreen,
                EnableBonusSystem = settings.EnableBonusSystem,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = settings.UpdatedAt
            };
        }

        private static DateTime GetLastBonusUpdatedAt(User user)
        {
            var lastPoint = user.BonusPoints.Count == 0 ? DateTime.MinValue : user.BonusPoints.Max(x => x.CreatedAt);
            var lastAchievement = user.Achievements.Count == 0 ? DateTime.MinValue : user.Achievements.Max(x => x.AchievedAt);
            return lastPoint > lastAchievement ? lastPoint : lastAchievement;
        }

        private static UserBonusSyncResponse BuildBonusSync(User user)
        {
            return new UserBonusSyncResponse
            {
                TotalPoints = user.BonusPoints.Sum(x => x.Points),
                LastUpdatedAt = GetLastBonusUpdatedAt(user),
                PointEntries = user.BonusPoints
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new UserBonusPointEntryDTO
                    {
                        Id = x.Id,
                        Points = x.Points,
                        Source = x.Source,
                        Description = x.Description,
                        SourceReferenceId = x.SourceReferenceId,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList(),
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
            };
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
                    InitialScreen = user.Settings?.InitialScreen ?? AppStartScreen.Home,
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

        private static void ValidateInitialScreen(AppStartScreen initialScreen)
        {
            if (!Enum.IsDefined(initialScreen))
            {
                throw new ValidationException("InitialScreen", "Tela inicial invalida.");
            }
        }

        private static string Escape(string value)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
