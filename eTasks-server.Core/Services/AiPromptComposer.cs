using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.Enums.Ai;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe responsável por compor prompts para interações com a IA, considerando o tipo de recurso e a intenção do usuário.
    /// </summary>
    public class AiPromptComposer : IAiPromptComposer
    {
        /// <summary>
        /// Constrói o prompt do sistema com base no tipo de recurso e na intenção do usuário.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string BuildSystemPrompt(AiAssistRequest request)
        {
            var resourceGuidance = request.Resource switch
            {
                AiResourceType.Tasks => "Você está ajudando um usuário de produtividade pessoal a transformar tarefas em execução real. Prefira ação clara, prioridade explícita, próximo passo concreto e decomposição apenas quando ela reduzir fricção.",
                AiResourceType.Goals => "Você está ajudando um usuário a sair de metas abstratas para metas executáveis. Estruture metas com critério de sucesso, marcos intermediários, riscos e próximos passos realistas.",
                AiResourceType.Notes => "Você está ajudando um usuário a transformar anotações em algo mais útil. Seu foco é resumir, limpar, reorganizar e extrair ação sem inventar informação.",
                AiResourceType.Readings => "Você está ajudando um usuário a extrair valor de leituras. Priorize resumo, entendimento, reflexão, aprendizados e próximos passos com base apenas no material informado.",
                AiResourceType.Shopping => "Você está ajudando um usuário a planejar compras com praticidade. Agrupe itens, identifique possíveis duplicidades, faltas prováveis e oportunidades de organização ou economia sem fingir saber preços reais.",
                AiResourceType.Finances => "Você está ajudando um usuário a entender melhor seu comportamento financeiro. Explique padrões, categorias e concentrações de gasto com prudência. Ofereça educação financeira básica, não consultoria financeira definitiva.",
                AiResourceType.UserProfile => "Você está ajudando um usuário a interpretar configurações, histórico de uso e sinais do próprio sistema para melhorar organização pessoal.",
                _ => "Você está ajudando um usuário dentro de um sistema de produtividade pessoal. O objetivo é transformar contexto salvo no app em orientação prática."
            };

            var intentGuidance = request.Intent switch
            {
                AiInteractionIntent.Summarize => "Responda com síntese útil, direta e orientada ao que importa agora.",
                AiInteractionIntent.Rewrite => "Reescreva para aumentar clareza, utilidade e objetividade, sem alterar o sentido do contexto.",
                AiInteractionIntent.SuggestNextSteps => "Sugira próximos passos concretos, curtos e executáveis no mundo real.",
                AiInteractionIntent.Analyze => "Aponte padrões, riscos, gargalos, desperdícios ou oportunidades relevantes no contexto recebido.",
                AiInteractionIntent.Plan => "Monte um plano prático em ordem lógica, com foco em execução progressiva e baixa fricção.",
                _ => "Seja útil, claro, prático e objetivo."
            };

            return $"{resourceGuidance} {intentGuidance} Não invente fatos ausentes. Se o contexto for insuficiente, diga exatamente o que falta. Evite texto motivacional genérico. Prefira apoiar decisão, clareza, planejamento e organização. Em finanças, deixe claro que a resposta é apoio educacional e organizacional.";
        }

        /// <summary>
        /// Constrói o prompt do usuário combinando título, conteúdo, contexto adicional e a solicitação do usuário.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string BuildUserPrompt(AiAssistRequest request)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.ResourceTitle))
            {
                parts.Add($"Título do recurso: {request.ResourceTitle.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.ResourceContent))
            {
                parts.Add($"Conteúdo do recurso:\n{request.ResourceContent.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
            {
                parts.Add($"Contexto adicional:\n{request.AdditionalContext.Trim()}");
            }

            parts.Add($"Solicitação do usuário:\n{request.UserPrompt.Trim()}");

            return string.Join("\n\n", parts);
        }
    }
}
