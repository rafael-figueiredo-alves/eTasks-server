namespace eTasks_server.Models.Enums.Readings
{
    /// <summary>
    /// Define o tipo de publicacao registrada na leitura.
    /// </summary>
    public enum ReadingFormat
    {
        /// <summary>
        /// Livro.
        /// </summary>
        Book = 0,
        /// <summary>
        /// Manga.
        /// </summary>
        Manga = 1,
        /// <summary>
        /// Gibi ou historia em quadrinhos.
        /// </summary>
        ComicBook = 2,
        /// <summary>
        /// Revista.
        /// </summary>
        Magazine = 3,
        /// <summary>
        /// Artigo ou texto.
        /// </summary>
        Article = 4,
        /// <summary>
        /// Outro tipo de publicacao.
        /// </summary>
        Other = 5
    }
}
