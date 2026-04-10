namespace eTasks_server.Models.DTOs.Users.Admin.Responses
{
    /// <summary>
    /// Entidade de resposta que representa um registro de login do usuário, contendo informações sobre o status do login, endereço IP, agente do usuário e data de criação do registro. Esta entidade é utilizada para fornecer detalhes sobre as tentativas de login dos usuários, permitindo que os administradores monitorem e analisem os padrões de acesso dos usuários ao sistema.
    /// </summary>
    public class UserLoginLogDTO
    {
        /// <summary>
        /// Identificador único do registro de login do usuário, representado como um GUID (Globally Unique Identifier). Este identificador é utilizado para distinguir de forma única cada registro de login, permitindo que os administradores rastreiem e gerenciem os registros de login dos usuários de maneira eficiente.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Indicador do status do login do usuário, representado como uma string. Este campo pode conter valores como "Success" (sucesso) ou "Failure" (falha), indicando se a tentativa de login foi bem-sucedida ou não. O status do login é essencial para que os administradores possam monitorar e analisar as tentativas de acesso dos usuários ao sistema, identificando possíveis problemas de segurança ou padrões de comportamento suspeitos.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Endereço IP associado à tentativa de login do usuário, representado como uma string. Este campo pode conter o endereço IP do dispositivo utilizado para realizar a tentativa de login, permitindo que os administradores rastreiem a origem das tentativas de acesso e identifiquem possíveis atividades suspeitas ou não autorizadas. O endereço IP é uma informação crucial para a segurança do sistema, ajudando a proteger as contas dos usuários contra acessos não autorizados.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Agente do usuário associado à tentativa de login do usuário, representado como uma string. Este campo pode conter informações sobre o navegador, sistema operacional ou dispositivo utilizado para realizar a tentativa de login, permitindo que os administradores obtenham insights sobre os padrões de acesso dos usuários e identifiquem possíveis atividades suspeitas ou não autorizadas. O agente do usuário é uma informação valiosa para a segurança do sistema, ajudando a proteger as contas dos usuários contra acessos não autorizados e a melhorar a experiência do usuário ao fornecer informações sobre os dispositivos utilizados para acessar o sistema.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Data e hora em que o registro de login do usuário foi criado, representada como um objeto DateTime. Este campo indica o momento exato em que a tentativa de login ocorreu, permitindo que os administradores monitorem e analisem os padrões de acesso dos usuários ao sistema ao longo do tempo. A data de criação do registro de login é essencial para a segurança do sistema, ajudando a identificar possíveis atividades suspeitas ou não autorizadas e a fornecer insights sobre o comportamento dos usuários ao acessar o sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
