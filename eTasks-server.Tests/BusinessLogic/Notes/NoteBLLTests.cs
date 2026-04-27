using eTasks_server.Core.BusinessLogicLayers.API_Resources.Notes;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Notes
{
    public class NoteBLLTests
    {
        [Fact]
        public async Task CreateAsync_ThenUpdateAsync_PersistsTrimmedContent()
        {
            using var context = TestDbContextFactory.Create(nameof(CreateAsync_ThenUpdateAsync_PersistsTrimmedContent));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            INoteBLL sut = new NoteBLL(context, NullLogger<INoteBLL>.Instance);

            var created = await sut.CreateAsync(user.Uid, new CreateNoteRequest
            {
                Subject = "  Nota 1  ",
                Content = "  Conteudo inicial  "
            });

            var updated = await sut.UpdateAsync(user.Uid, created.Id, new UpdateNoteRequest
            {
                Subject = "  Nota alterada ",
                Content = "  Conteudo alterado "
            });

            Assert.Equal("Nota alterada", updated.Subject);
            Assert.Equal("Conteudo alterado", updated.Content);
        }

        [Fact]
        public async Task DeleteAsync_CreatesTombstoneVisibleInSync()
        {
            using var context = TestDbContextFactory.Create(nameof(DeleteAsync_CreatesTombstoneVisibleInSync));
            var user = TestDbContextFactory.CreateActiveUser();
            var note = new Models.Entities.Notes.NoteItem
            {
                UserUid = user.Uid,
                Subject = "Nota",
                Content = "Texto"
            };

            context.Users.Add(user);
            context.Notes.Add(note);
            await context.SaveChangesAsync();

            INoteBLL sut = new NoteBLL(context, NullLogger<INoteBLL>.Instance);

            await sut.DeleteAsync(user.Uid, note.Id);
            var sync = await sut.SyncAsync(user.Uid, new SyncNotesRequest());

            Assert.Contains(sync.Deleted, x => x.Id == note.Id);
            Assert.True(context.Notes.Single().IsDeleted);
        }
    }
}
