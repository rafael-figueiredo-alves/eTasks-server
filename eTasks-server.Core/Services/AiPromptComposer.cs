using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.Enums.Ai;

namespace eTasks_server.Core.Services
{
    public class AiPromptComposer : IAiPromptComposer
    {
        public string BuildSystemPrompt(AiAssistRequest request)
        {
            var resourceGuidance = request.Resource switch
            {
                AiResourceType.Tasks => "Voce esta ajudando um usuario de produtividade pessoal a transformar tarefas em execucao real. Prefira acao clara, prioridade explicita, proximo passo concreto e decomposicao apenas quando ela reduzir friccao.",
                AiResourceType.Goals => "Voce esta ajudando um usuario a sair de metas abstratas para metas executaveis. Estruture metas com criterio de sucesso, marcos intermediarios, riscos e proximos passos realistas.",
                AiResourceType.Notes => "Voce esta ajudando um usuario a transformar anotacoes em algo mais util. Seu foco e resumir, limpar, reorganizar e extrair acao sem inventar informacao.",
                AiResourceType.Readings => "Voce esta ajudando um usuario a extrair valor de leituras. Priorize resumo, entendimento, reflexao, aprendizados e proximos passos com base apenas no material informado.",
                AiResourceType.Shopping => "Voce esta ajudando um usuario a planejar compras com praticidade. Agrupe itens, identifique possiveis duplicidades, faltas provaveis e oportunidades de organizacao ou economia sem fingir saber precos reais.",
                AiResourceType.Finances => "Voce esta ajudando um usuario a entender melhor seu comportamento financeiro. Explique padroes, categorias e concentracoes de gasto com prudencia. Ofereca educacao financeira basica, nao consultoria financeira definitiva.",
                AiResourceType.UserProfile => "Voce esta ajudando um usuario a interpretar configuracoes, historico de uso e sinais do proprio sistema para melhorar organizacao pessoal.",
                _ => "Voce esta ajudando um usuario dentro de um sistema de produtividade pessoal. O objetivo e transformar contexto salvo no app em orientacao pratica."
            };

            var intentGuidance = request.Intent switch
            {
                AiInteractionIntent.Summarize => "Responda com sintese util, direta e orientada ao que importa agora.",
                AiInteractionIntent.Rewrite => "Reescreva para aumentar clareza, utilidade e objetividade, sem alterar o sentido do contexto.",
                AiInteractionIntent.SuggestNextSteps => "Sugira proximos passos concretos, curtos e executaveis no mundo real.",
                AiInteractionIntent.Analyze => "Aponte padroes, riscos, gargalos, desperdicios ou oportunidades relevantes no contexto recebido.",
                AiInteractionIntent.Plan => "Monte um plano pratico em ordem logica, com foco em execucao progressiva e baixa friccao.",
                _ => "Seja util, claro, pratico e objetivo."
            };

            return $"{resourceGuidance} {intentGuidance} Nao invente fatos ausentes. Se o contexto for insuficiente, diga exatamente o que falta. Evite texto motivacional genérico. Prefira apoiar decisao, clareza, planejamento e organizacao. Em financas, deixe claro que a resposta e apoio educacional e organizacional.";
        }

        public string BuildUserPrompt(AiAssistRequest request)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.ResourceTitle))
            {
                parts.Add($"Titulo do recurso: {request.ResourceTitle.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.ResourceContent))
            {
                parts.Add($"Conteudo do recurso:\n{request.ResourceContent.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
            {
                parts.Add($"Contexto adicional:\n{request.AdditionalContext.Trim()}");
            }

            parts.Add($"Solicitacao do usuario:\n{request.UserPrompt.Trim()}");

            return string.Join("\n\n", parts);
        }
    }
}
