using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados para criar uma tarefa.
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Resumo ou título da tarefa. Este campo é obrigatório e deve conter uma descrição breve da tarefa a ser realizada.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Anotações ou detalhes adicionais sobre a tarefa. Este campo é opcional e pode ser usado para fornecer informações complementares que ajudem na execução da tarefa, como instruções específicas, links relacionados ou qualquer outra informação relevante.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Prioridade da tarefa. Este campo é opcional e pode ser usado para indicar a importância ou urgência da tarefa. Os valores possíveis são: Baixa, Média e Alta. Se não for especificado, a prioridade padrão será Média.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        /// <summary>
        /// Data da tarefa. Este campo é obrigatório e indica quando a tarefa deve ser realizada.
        /// </summary>
        public DateTime TaskDate { get; set; }
        
        /// <summary>
        /// Indica se a tarefa foi concluída. Este campo é opcional e pode ser usado para marcar a tarefa como concluída ou pendente.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Recorrência da tarefa. Este campo é opcional e pode ser usado para definir a repetição da tarefa em intervalos específicos.
        /// </summary>
        public TaskRecurrenceRequest? Recurrence { get; set; }
    }
}
