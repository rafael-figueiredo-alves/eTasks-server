namespace eTasks_server.Client.Services
{
    /// <summary>
    /// Serviço de estado para gerenciar as informações do usuário logado no cliente Blazor.
    /// Mantém o nome e a foto (base64) em memória para atualização instantânea da UI.
    /// </summary>
    public class UserState
    {
        public string? Name { get; private set; }
        public string? PhotoBase64 { get; private set; }
        public bool IsDarkTheme { get; private set; }

        public event Action? OnChange;

        public void SetUser(string? name, string? photoBase64)
        {
            Name = name;
            PhotoBase64 = photoBase64;
            NotifyStateChanged();
        }

        public void UpdatePhoto(string? photoBase64)
        {
            PhotoBase64 = photoBase64;
            NotifyStateChanged();
        }

        public void UpdateName(string? name)
        {
            Name = name;
            NotifyStateChanged();
        }

        public void UpdateTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
