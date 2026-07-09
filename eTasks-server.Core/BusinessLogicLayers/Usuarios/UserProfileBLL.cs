using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Helpers;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Settings;
using eTasks_server.Models.Enums.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace eTasks_server.Core.BusinessLogicLayers.Usuarios
{
    /// <summary>
    /// Regras de negócio do perfil de usuário
    /// </summary>
    /// <param name="context"></param>
    /// <param name="configuration"></param>
    /// <param name="emailService"></param>
    /// <param name="settingsProvider"></param>
    /// <param name="logger"></param>
    public class UserProfileBLL(
       AppDbContext context,
        IConfiguration configuration,
        IEmailService emailService,
        IServerSettingsProvider settingsProvider,
        ILogger<IUserProfileBLL> logger) : BaseBLL<IUserProfileBLL>(context, logger), IUserProfileBLL
    {
        #region Funções principais
        /// <summary>
        /// Obtem perfil
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        public async Task<UserProfileResponse> GetProfileAsync(Guid userUid)
        {
            // Pega usuário ativo
            var user = await GetActiveUserAsync(userUid);

            // Obtem configurações
            await EnsureSettingsAsync(user);

            // Verifica último acesso 
            await TouchLastAccessAsync(user);

            // Constroi resposta do perfil de usuário
            return await BuildProfileResponseAsync(user);
        }

        /// <summary>
        /// Atualiza o perfil do usuário
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem usuário conectado
            var user = await GetActiveUserAsync(userUid);

            // Obtem configurações
            await EnsureSettingsAsync(user);

            // Normaliza o e-mail
            var normalizedEmail = request.Email.Trim();

            // Valida se o email informado já não se encontra em uso
            var emailInUse = await _context.Users.AnyAsync(
                x => x.Uid != userUid && !x.IsDeleted && x.Email == normalizedEmail,
                cancellationToken);

            // Valida se e-mail em uso
            if (emailInUse)
            {
                throw new ValidationException("Email", "O e-mail informado já está cadastrado.");
            }

            user.Name = request.Name.Trim();
            user.Email = normalizedEmail;

            // Trata foto de perfil
            if (request.RemovePhoto)
            {
                UserPhotoStorage.Delete(user.PhotoPath);
                user.PhotoPath = null;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                user.PhotoPath = await UserPhotoStorage.SaveAsync(request.PhotoBase64, user.PhotoPath, cancellationToken);
            }

            // Atualiza último acesso 
            await TouchLastAccessAsync(user, saveChanges: false);

            // Salva dados no banco de dados
            await _context.SaveChangesAsync(cancellationToken);

            // Monta resposta
            return await BuildProfileResponseAsync(user);
        }

        /// <summary>
        /// Atualiza configurações do usuário
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserSettingsDTO> PatchSettingsAsync(Guid userUid, PatchUserSettingsRequest request)
        {
            // Obtem usuário e configurações do usuário
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);

            // Valida configuração do tema do usuário
            if (!string.IsNullOrWhiteSpace(request.Theme))
            {
                ValidateTheme(request.Theme);
                settings.Theme = request.Theme.Trim().ToLowerInvariant();
            }

            // Valida configuração do idioma do usuário
            if (!string.IsNullOrWhiteSpace(request.Language))
            {
                ValidateLanguage(request.Language);
                settings.Language = request.Language.Trim().ToLowerInvariant();
            }

            // Valida tela inicial
            if (request.InitialScreen.HasValue)
            {
                ValidateInitialScreen(request.InitialScreen.Value);
                settings.InitialScreen = request.InitialScreen.Value;
            }

            // Valida se habilitado bonificação
            if (request.EnableBonusSystem.HasValue)
            {
                settings.EnableBonusSystem = request.EnableBonusSystem.Value;
            }

            // Atualiza data de edições
            settings.UpdatedAt = SaoPauloDateTime.Now();

            // Salva último acesso
            await TouchLastAccessAsync(user, saveChanges: false);

            // Salva alterações no banco
            await _context.SaveChangesAsync();

            // retorna as configurações
            return new UserSettingsDTO
            {
                Theme = settings.Theme,
                Language = settings.Language,
                InitialScreen = settings.InitialScreen,
                EnableBonusSystem = settings.EnableBonusSystem
            };
        }

        /// <summary>
        /// Obtem configurações
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        public async Task<UserSettingsSyncResponse> GetSettingsAsync(Guid userUid)
        {
            // Obtem usuário e configurações
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);

            // Grava último acesso
            await TouchLastAccessAsync(user);

            // Mapeia configurações
            return MapSettings(settings);
        }

        /// <summary>
        /// Retorno pontos
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        public async Task<UserBonusSyncResponse> GetBonusAsync(Guid userUid)
        {
            // Obtem usuário e configurações
            var user = await GetActiveUserAsync(userUid);
            await EnsureSettingsAsync(user);

            // Grava último acesso
            await TouchLastAccessAsync(user);

            // Constroi resposta
            return BuildBonusSync(user);
        }

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserDataSyncResponse> SyncUserDataAsync(Guid userUid, SyncUserDataRequest request)
        {
            // Obtem usuário ativo e suas configurações
            var user = await GetActiveUserAsync(userUid);
            var settings = await EnsureSettingsAsync(user);

            // Grava último acesso
            await TouchLastAccessAsync(user);

            // Obtem se configurações foram alteradas
            var settingsChanged = !request.Since.HasValue || settings.UpdatedAt > request.Since.Value;

            // Obtem data das últimas atualizações de pontos e notifica se houve mudanças
            var lastBonusUpdateAt = GetLastBonusUpdatedAt(user);
            var bonusChanged = !request.Since.HasValue || lastBonusUpdateAt > request.Since.Value;

            // retorna dados sobre alterações
            return new UserDataSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Settings = settingsChanged ? MapSettings(settings) : null,
                Bonus = bonusChanged ? BuildBonusSync(user) : null
            };
        }

        /// <summary>
        /// Exporta perfil como CSV
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        public async Task<string> ExportProfileCsvAsync(Guid userUid)
        {
            // Obtem perfil
            var profile = await GetProfileAsync(userUid);

            // Constroi arquivo CSV
            var builder = new StringBuilder();
            builder.AppendLine("Uid,Name,Email,CreatedAt,LastAccessAt,Theme,Language,InitialScreen,EnableBonusSystem,TotalBonusPoints,Achievements");
            builder.Append(Utils.Escape(profile.Uid.ToString())).Append(',');
            builder.Append(Utils.Escape(profile.Name)).Append(',');
            builder.Append(Utils.Escape(profile.Email)).Append(',');
            builder.Append(Utils.Escape(profile.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))).Append(',');
            builder.Append(Utils.Escape(profile.LastAccessAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty)).Append(',');
            builder.Append(Utils.Escape(profile.Settings.Theme)).Append(',');
            builder.Append(Utils.Escape(profile.Settings.Language)).Append(',');
            builder.Append(Utils.Escape(profile.Settings.InitialScreen.ToString())).Append(',');
            builder.Append(Utils.Escape(profile.Settings.EnableBonusSystem.ToString())).Append(',');
            builder.Append(Utils.Escape(profile.Bonus.TotalPoints.ToString())).Append(',');
            builder.Append(Utils.Escape(string.Join(" | ", profile.Bonus.Achievements.Select(x => $"{x.Name}:{x.PointsRequired}"))));
            builder.AppendLine();

            // Retorna arquivo CSV
            return builder.ToString();
        }

        /// <summary>
        /// Marca registro / usuário para exclusão
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task SoftDeleteAsync(Guid userUid)
        {
            // Obtem usuário ativo
            var user = await GetActiveUserAsync(userUid);

            // Verifica se é Admin e avisa que não é permitido excluir
            if (user.IsAdmin)
            {
                throw new ApiException(HttpStatusCode.Forbidden, "Não é permitido excluir uma conta administrativa por esta rota.");
            }

            // Marca conta como excluida
            user.IsDeleted = true;
            user.DeletedAt = SaoPauloDateTime.Now();
            user.IsBlocked = true;
            user.LastAccessAt = user.DeletedAt;

            // Configura o tempo para reativação de conta
            var settings = await settingsProvider.GetCurrentAsync();
            var validityDays = settings.AccountReactivationCodeValidityDays is < 7 or > 90
                ? 30
                : settings.AccountReactivationCodeValidityDays;

            // Obtem código para recuperar conta anteriores
            var previousCodes = await _context.AccountReactivationCodes
                .Where(x => x.UserUid == userUid && !x.IsUsed)
                .ToListAsync();

            // Desabilita códigos antigos
            foreach (var previousCode in previousCodes)
            {
                previousCode.IsUsed = true;
                previousCode.UsedAt = DateTime.UtcNow;
            }

            // Gera código para reativar conta
            var reactivationCode = new AccountReactivationCode
            {
                UserUid = userUid,
                Code = Utils.GenerateReactivationCode(),
                ExpiresAt = DateTime.UtcNow.AddDays(validityDays)
            };

            // Salva código
            await _context.AccountReactivationCodes.AddAsync(reactivationCode);

            // Desativa tokens do usuário
            var activeTokens = await _context.RefreshTokens
                .Where(x => x.UserUid == userUid && !x.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            // Salva informações
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuário {Uid} removido logicamente.", userUid);

            // Envia e-mail de reativação de conta
            await emailService.SendAccountReactivationEmailAsync(
                user.Email,
                BuildAccountReactivationLink(reactivationCode.Code),
                reactivationCode.ExpiresAt);
        }
        #endregion

        #region Funções privadas, exclusivas da classe
        /// <summary>
        /// Constroi link para reativação da conta
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string BuildAccountReactivationLink(string code)
        {
            var baseUrl = configuration[Constants.ApiBaseUrl] ?? SettingsEnum.DefaultBaseURL;
            var apiPath = configuration[Constants.ApiV2Path] ?? "/api/v2";
            return $"{(baseUrl + apiPath).TrimEnd('/')}/auth/recover-account?code={WebUtility.UrlEncode(code)}";
        }

        /// <summary>
        /// Obtem usuário ativo
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        private Task<User> GetActiveUserAsync(Guid userUid)
        {
            return GetAndValidateActiveUserAsync(userUid, query => query
                .Include(x => x.Settings)
                .Include(x => x.BonusPoints)
                .Include(x => x.Achievements)
                    .ThenInclude(x => x.BonusAchievement));
        }

        /// <summary>
        /// Garante obtenção das configurações do usuário
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Mapeia as configurações do usuário
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retorna data da última atribuição de bonus
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static DateTime GetLastBonusUpdatedAt(User user)
        {
            var lastPoint = user.BonusPoints.Count == 0 ? DateTime.MinValue : user.BonusPoints.Max(x => x.CreatedAt);
            var lastAchievement = user.Achievements.Count == 0 ? DateTime.MinValue : user.Achievements.Max(x => x.AchievedAt);
            return lastPoint > lastAchievement ? lastPoint : lastAchievement;
        }

        /// <summary>
        /// Constroi o retorno da Bonificação do usuário
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Constroi resposta com dados do perfil a retornas
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<UserProfileResponse> BuildProfileResponseAsync(User user)
        {
            // Pega valor base64 da foto de perfil
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
                    Theme = user.Settings?.Theme ?? SettingsEnum.DefaultTheme,
                    Language = user.Settings?.Language ?? SettingsEnum.DefaultLanguage,
                    InitialScreen = user.Settings?.InitialScreen ?? SettingsEnum.DefaultStartScreen,
                    EnableBonusSystem = user.Settings?.EnableBonusSystem ?? SettingsEnum.DefaultEnableBonus
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

        /// <summary>
        /// Salva data/hora do último acesso
        /// </summary>
        /// <param name="user"></param>
        /// <param name="saveChanges"></param>
        /// <returns></returns>
        private async Task TouchLastAccessAsync(User user, bool saveChanges = true)
        {
            user.LastAccessAt = SaoPauloDateTime.Now();
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Valida Tema escolhido
        /// </summary>
        /// <param name="theme"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateTheme(string theme)
        {
            if (!SettingsEnum.AllowedThemes.Contains(theme.Trim()))
            {
                throw new ValidationException("Theme", $"Tema inválido. Valores aceitos: {Utils.FormatWithOr(SettingsEnum.AllowedThemes)}.");
            }
        }

        /// <summary>
        /// Valida idioma selecionado
        /// </summary>
        /// <param name="language"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateLanguage(string language)
        {
            if (!SettingsEnum.AllowedLanguages.Contains(language.Trim()))
            {
                throw new ValidationException("Language", $"Idioma inválido. Valores aceitos: {Utils.FormatWithOr(SettingsEnum.AllowedLanguages)}.");
            }
        }

        /// <summary>
        /// Valida tela inicial
        /// </summary>
        /// <param name="initialScreen"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateInitialScreen(AppStartScreen initialScreen)
        {
            if (!Enum.IsDefined(initialScreen))
            {
                throw new ValidationException("InitialScreen", "Tela inicial inválida.");
            }
        }
        #endregion
    }
}
