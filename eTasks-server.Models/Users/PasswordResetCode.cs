using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Users
{
    /// <summary>
    /// Entidade que representa um código de redefinição de senha para um usuário. Este código é gerado quando um usuário solicita a redefinição de senha e é usado para validar a solicitação. Ele contém informações como o ID do usuário, o código gerado, a data de expiração e se o código já foi usado ou não.
    /// </summary>
    public class PasswordResetCode
    {
        /// <summary>
        /// Identificador único do código de redefinição de senha, gerado usando o método CreateVersion7 para garantir a unicidade e a ordenação temporal. Este ID é usado para referenciar o código de redefinição de senha em operações de banco de dados e para garantir que cada código seja distinto.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador único do usuário associado a este código de redefinição de senha. Este campo é usado para vincular o código ao usuário específico que solicitou a redefinição de senha, permitindo que o sistema valide a solicitação e atualize a senha do usuário corretamente.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Código de redefinição de senha gerado para o usuário. Este código é uma string única que é enviada ao usuário (geralmente por e-mail) para que ele possa usá-lo para validar a solicitação de redefinição de senha. O código deve ser difícil de adivinhar para garantir a segurança do processo de redefinição de senha.
        /// </summary>
        [Length(6, 6, ErrorMessage = "O código de verificação deve ter exatamente 6 dígitos")]
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Data em que o código de redefinição de senha expira. Após essa data, o código não será mais válido e o usuário precisará solicitar um novo código para redefinir sua senha. A expiração do código é uma medida de segurança para garantir que os códigos de redefinição de senha não sejam usados indefinidamente, reduzindo o risco de uso indevido.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Identifica se o código de redefinição de senha já foi usado ou não. Este campo é importante para garantir que um código de redefinição de senha só possa ser usado uma vez, aumentando a segurança do processo de redefinição de senha. Se o código já tiver sido usado, ele não poderá ser reutilizado para redefinir a senha do usuário.
        /// </summary>
        public bool IsUsed { get; set; } = false;
        /// <summary>
        /// Data em que o código de redefinição de senha foi criado. Este campo é útil para fins de auditoria e para calcular a validade do código, especialmente quando combinado com a data de expiração. Ele permite que o sistema determine quanto tempo um código de redefinição de senha esteve ativo e se ele ainda é válido ou se já expirou.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuário associado a este código de redefinição de senha. Esta propriedade de navegação permite acessar as informações do usuário relacionado a este código, facilitando a validação e o processo de redefinição de senha. A relação entre PasswordResetCode e User é estabelecida através do campo UserUid, que é uma chave estrangeira que referencia o ID do usuário na tabela de usuários.
        /// </summary>
        // Navigation Property
        public User? User { get; set; }
    }
}
