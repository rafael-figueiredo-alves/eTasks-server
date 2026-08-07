using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace eTasks_server.Tests.Data
{
    /// <summary>
    /// Classe de testes unitários para o modelo do contexto de banco de dados (AppDbContext).
    /// </summary>
    public class AppDbContextModelTests
    {
        /// <summary>
        /// Teste para verificar se as chaves primárias do tipo Guid estão configuradas para usar o tipo de coluna "binary(16)" e a função padrão "UUID_TO_BIN(UUID(), 1)".
        /// </summary>
        [Fact]
        public void GuidPrimaryKeys_UseBinary16_AndUuidToBinDefault()
        {
            using var context = TestDbContextFactory.CreateRelationalModelContext();

            var userUid = context.Model.FindEntityType(typeof(User))!.FindProperty(nameof(User.Uid))!;
            var financeId = context.Model.FindEntityType(typeof(FinanceEntry))!.FindProperty(nameof(FinanceEntry.Id))!;

            Assert.Equal("binary(16)", userUid.GetColumnType());
            Assert.Equal("UUID_TO_BIN(UUID(), 1)", userUid.GetDefaultValueSql());
            Assert.Equal("binary(16)", financeId.GetColumnType());
            Assert.Equal("UUID_TO_BIN(UUID(), 1)", financeId.GetDefaultValueSql());
        }

        /// <summary>
        /// Teste para verificar se as entidades RefreshToken e PasswordResetCode possuem índices críticos configurados corretamente.
        /// </summary>
        [Fact]
        public void RefreshToken_AndPasswordResetCode_ContainCriticalIndexes()
        {
            using var context = TestDbContextFactory.CreateRelationalModelContext();

            var refreshTokenIndexes = context.Model.FindEntityType(typeof(RefreshToken))!.GetIndexes().ToList();
            var passwordResetIndexes = context.Model.FindEntityType(typeof(PasswordResetCode))!.GetIndexes().ToList();

            Assert.Contains(refreshTokenIndexes, x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual([nameof(RefreshToken.Token)]));
            Assert.Contains(refreshTokenIndexes, x => x.Properties.Select(p => p.Name).SequenceEqual([nameof(RefreshToken.IsRevoked), nameof(RefreshToken.ExpiresAt)]));
            Assert.Contains(passwordResetIndexes, x => x.Properties.Select(p => p.Name).SequenceEqual([nameof(PasswordResetCode.UserUid), nameof(PasswordResetCode.IsUsed), nameof(PasswordResetCode.ExpiresAt)]));
        }

        /// <summary>
        /// Teste para verificar se as entidades FinanceRecurrence e TaskItem possuem estratégias de indexação esperadas configuradas corretamente.
        /// </summary>
        [Fact]
        public void FinanceRecurrence_AndTaskItem_HaveExpectedIndexingStrategy()
        {
            using var context = TestDbContextFactory.CreateRelationalModelContext();

            var financeRecurrenceIndexes = context.Model.FindEntityType(typeof(FinanceRecurrence))!.GetIndexes().ToList();
            var taskIndexes = context.Model.FindEntityType(typeof(TaskItem))!.GetIndexes().ToList();

            Assert.Contains(financeRecurrenceIndexes, x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual([nameof(FinanceRecurrence.FinanceEntryId)]));
            Assert.Contains(taskIndexes, x => x.Properties.Select(p => p.Name).SequenceEqual([nameof(TaskItem.UserUid), nameof(TaskItem.IsDeleted), nameof(TaskItem.TaskDate)]));
        }
    }
}
