using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Pages
{
    public class AboutBase : ComponentBase
    {
        protected string VersionDetails = @"Esta é a versão inicial da aplicação eTasks Server.

                                            Recursos implementados: Gerenciamento básico de tarefas.
                                            Correções: Nenhuma pendente.
                                            Notas: Em desenvolvimento futuro, os detalhes serão lidos de um arquivo TXT.";
    }
}
