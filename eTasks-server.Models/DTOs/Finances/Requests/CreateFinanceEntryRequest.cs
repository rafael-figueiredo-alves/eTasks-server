using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Dados para criar um lancamento financeiro.
    /// </summary>
    public class CreateFinanceEntryRequest
    {
        /// <summary>
        /// Título do lançamento financeiro.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do lançamento financeiro (opcional).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoria do lançamento financeiro (opcional), como "Alimentação", "Transporte", "Salário", etc.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Contraparte envolvida no lançamento financeiro (opcional), como o nome de um fornecedor, cliente ou instituição financeira.
        /// </summary>
        public string? Counterparty { get; set; }
       
        /// <summary>
        /// Tipo do lançamento financeiro, como Débito ou Crédito.
        /// </summary>
        public FinanceEntryType EntryType { get; set; } = FinanceEntryType.Debit;

        /// <summary>
        /// Método de pagamento utilizado para o lançamento financeiro, como Dinheiro, Cartão de Crédito, Transferência Bancária, etc.
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; } = FinancePaymentMethod.Other;

        /// <summary>
        /// Valor monetário do lançamento financeiro, representando a quantia de dinheiro envolvida na transação. Deve ser um valor positivo, onde o tipo do lançamento (débito ou crédito) determinará se o valor será subtraído ou adicionado ao saldo financeiro do usuário.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Data em que o lançamento financeiro ocorre ou ocorreu. Para lançamentos futuros, essa data representa quando a transação está programada para acontecer. Para lançamentos passados, essa data indica quando a transação realmente ocorreu. Essa informação é crucial para o controle financeiro do usuário, permitindo organizar e visualizar os lançamentos de acordo com suas datas de ocorrência.
        /// </summary>
        public DateTime OccursOn { get; set; }

        /// <summary>
        /// Confirma se o lançamento financeiro já foi pago ou não. Para lançamentos futuros, essa propriedade pode ser usada para indicar se o pagamento já foi efetuado ou se ainda está pendente. Para lançamentos passados, essa propriedade pode indicar se o pagamento foi realizado conforme o planejado ou se houve algum atraso ou problema no processo de pagamento. Essa informação é importante para o controle financeiro do usuário, permitindo acompanhar o status dos pagamentos e tomar as ações necessárias para garantir que as transações sejam concluídas com sucesso.
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Data em que o pagamento do lançamento financeiro foi efetuado. Para lançamentos futuros, essa propriedade pode ser preenchida quando o pagamento for realizado, indicando a data exata em que a transação foi paga. Para lançamentos passados, essa propriedade pode ser preenchida com a data em que o pagamento ocorreu, permitindo um registro preciso das transações financeiras do usuário. Essa informação é essencial para o controle financeiro, ajudando o usuário a acompanhar seus pagamentos e manter um histórico detalhado de suas transações financeiras.
        /// </summary>
        public DateTime? PaidAt { get; set; }
        
        /// <summary>
        /// Indica se o lançamento financeiro é recorrente. Para lançamentos recorrentes, essa propriedade pode ser usada para identificar se a transação se repete em intervalos regulares, como mensalmente, semanalmente ou anualmente. Essa informação é importante para o planejamento financeiro do usuário, permitindo acompanhar e gerenciar lançamentos que ocorrem de forma periódica.
        /// </summary>
        public bool IsRecurring { get; set; }
        
        /// <summary>
        /// Detalhes da recorrência do lançamento financeiro. Para lançamentos recorrentes, essa propriedade pode conter informações sobre a frequência, intervalo e duração da recorrência, ajudando o usuário a entender e gerenciar melhor suas transações financeiras periódicas.
        /// </summary>
        public FinanceRecurrenceRequest? Recurrence { get; set; }
    }
}
