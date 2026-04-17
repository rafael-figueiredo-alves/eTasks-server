using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Client.Services
{
    /// <summary>
    /// Serviço para gerenciar a abertura e fechamento de um drawer que exibe os logs de login de um usuário selecionado. Ele mantém o estado do usuário selecionado, os logs correspondentes e se o drawer está aberto ou fechado. O serviço também expõe um evento para notificar os componentes interessados sobre as mudanças de estado.
    /// </summary>
    public class UserLogsDrawerService
    {
        /// <summary>
        /// Usuário selecionado para exibir os logs de login. Pode ser nulo se nenhum usuário estiver selecionado ou se o drawer estiver fechado.
        /// </summary>
        public AdminUserDTO? SelectedUser { get; private set; }

        /// <summary>
        /// Lista de logs de login do usuário selecionado. Será atualizada toda vez que um novo usuário for selecionado e o drawer for aberto. Se nenhum usuário estiver selecionado, esta lista estará vazia.
        /// </summary>
        public List<UserLoginLogDTO> SelectedUserLogs { get; private set; } = new();

        /// <summary>
        /// Indica se o drawer está aberto ou fechado. O drawer estará aberto quando um usuário for selecionado e os logs forem carregados, e estará fechado quando o usuário fechar o drawer ou selecionar outro usuário.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Evento que é disparado sempre que o estado do drawer muda, seja abrindo ou fechando. Os componentes interessados podem se inscrever nesse evento para atualizar sua interface de acordo com as mudanças de estado do drawer.
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Método para abrir o drawer com um usuário selecionado e seus logs de login correspondentes. Ele atualiza o estado do usuário selecionado, os logs e marca o drawer como aberto, além de disparar o evento de mudança para notificar os componentes interessados.
        /// </summary>
        /// <param name="user">Usuário selecionado</param>
        /// <param name="logs">Logs de login do usuário selecionado</param>
        public void Open(AdminUserDTO? user, List<UserLoginLogDTO> logs)
        {
            SelectedUser = user;
            SelectedUserLogs = logs;
            IsOpen = true;
            OnChange?.Invoke();
        }

        /// <summary>
        /// Método para fechar o drawer. Ele limpa o estado do usuário selecionado e dos logs, marca o drawer como fechado e dispara o evento de mudança para notificar os componentes interessados sobre a mudança de estado.
        /// </summary>
        public void Close()
        {
            SelectedUser = null;
            SelectedUserLogs.Clear();
            IsOpen = false;
            OnChange?.Invoke();
        }
    }
}
