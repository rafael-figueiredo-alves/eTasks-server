using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    /// <summary>
    /// Entidade enviada para atualizar o perfil do usuário. Todos os campos são obrigatórios, garantindo que o usuário forneça as informações necessárias para atualizar seu perfil de forma completa e consistente. O campo "PhotoBase64" é opcional, permitindo que o usuário escolha se deseja atualizar sua foto de perfil ou mantê-la inalterada. O campo "RemovePhoto" é um booleano que indica se a foto de perfil deve ser removida, oferecendo ao usuário a flexibilidade de gerenciar sua imagem de perfil conforme suas preferências.
    /// </summary>
    public class UpdateUserProfileRequest
    {
        /// <summary>
        /// Nome de exibição do usuário. Deve conter entre 3 e 30 caracteres, garantindo que o nome seja suficientemente descritivo sem ser excessivamente longo. Este campo é obrigatório para assegurar que o perfil do usuário tenha um nome de exibição válido e consistente. O nome de exibição é uma parte importante da identidade do usuário dentro do aplicativo, permitindo que outros usuários o reconheçam facilmente.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// E-mail do usuário. Deve ser um endereço de e-mail válido, garantindo que o usuário forneça um meio de contato confiável e funcional. Este campo é obrigatório para assegurar que o perfil do usuário tenha um endereço de e-mail válido, permitindo que o aplicativo envie notificações, atualizações e outras comunicações importantes para o usuário. O formato do e-mail é verificado para garantir que seja válido, evitando erros de comunicação e garantindo a integridade dos dados do perfil do usuário.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Arquivo de imagem em base64 para atualizar a foto de perfil do usuário. Este campo é opcional, permitindo que o usuário escolha se deseja atualizar sua foto de perfil ou mantê-la inalterada. Se fornecido, o valor deve ser uma string em formato Base64 que representa a imagem, garantindo que o aplicativo possa processar e exibir a nova foto de perfil corretamente. O formato Base64 é utilizado para garantir que a imagem seja transmitida de forma segura e eficiente, evitando problemas de compatibilidade e garantindo a integridade dos dados da imagem durante a transferência.
        /// </summary>
        [Base64String(ErrorMessage = "Formato de imagem aceito e apenas Base64.")]
        public string? PhotoBase64 { get; set; }

        /// <summary>
        /// Indicador se é para remover a foto de perfil do usuário. O valor é um booleano, onde "true" indica que a foto de perfil deve ser removida, permitindo que o usuário escolha se deseja excluir sua imagem de perfil atual. Este campo oferece ao usuário a flexibilidade de gerenciar sua imagem de perfil conforme suas preferências, permitindo que ele mantenha ou remova sua foto de perfil conforme desejar. Se "RemovePhoto" for definido como "true", a foto de perfil do usuário será removida, e o aplicativo deve garantir que a imagem seja excluída de forma segura e eficiente, mantendo a integridade dos dados do perfil do usuário.
        /// </summary>
        public bool RemovePhoto { get; set; }
    }
}
