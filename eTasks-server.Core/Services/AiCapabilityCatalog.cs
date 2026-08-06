using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.AI.Responses;
using eTasks_server.Models.Enums.Ai;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe que implementa o catálogo de capacidades da IA, fornecendo informações sobre os recursos disponíveis, usos recomendados, intenções suportadas e diretrizes de segurança para cada recurso.
    /// </summary>
    public class AiCapabilityCatalog : IAiCapabilityCatalog
    {
        /// <summary>
        /// Obtém as capacidades da IA, incluindo o modo do provedor, diretrizes transversais e uma lista de recursos com suas respectivas capacidades.
        /// </summary>
        /// <returns></returns>
        public AiCapabilitiesResponse GetCapabilities()
        {
            return new AiCapabilitiesResponse
            {
                ProviderMode = "OpenRouter",
                CrossCuttingGuidance =
                [
                    "A IA deve ajudar o usuário a decidir e agir melhor, não apenas conversar.",
                    "O uso ideal é sempre contextual ao recurso atual da tela ou do fluxo.",
                    "Respostas devem ser curtas, acionáveis e alinhadas ao contexto salvo no eTasks.",
                    "A IA não deve inventar dados ausentes nem agir como fonte definitiva para temas sensíveis.",
                    "Em finanças, a IA deve priorizar educação e organização, não aconselhamento financeiro definitivo."
                ],
                Resources =
                [
                    BuildTasksCapability(),
                    BuildGoalsCapability(),
                    BuildNotesCapability(),
                    BuildReadingsCapability(),
                    BuildShoppingCapability(),
                    BuildFinancesCapability(),
                    BuildUserProfileCapability(),
                    BuildGeneralCapability()
                ]
            };
        }

        #region Métodos privados para construir capacidades específicas de recursos
        /// <summary>
        /// Monta a capacidade da IA para o recurso de Tarefas, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildTasksCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Tasks,
                Label = "Tarefas",
                RecommendedUses =
                [
                    "quebrar uma tarefa grande em subtarefas",
                    "reescrever tarefas vagas em ações objetivas",
                    "sugerir prioridade e ordem de execução",
                    "propor o próximo passo quando houver bloqueio"
                ],
                SupportedIntents = ["Rewrite", "SuggestNextSteps", "Analyze", "Plan"],
                Guardrails =
                [
                    "não mudar datas ou prioridades sem explicar o critério",
                    "não marcar tarefa como concluída automaticamente",
                    "não criar volume excessivo de subtarefas sem ganho real"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "tasks",
                    titlePattern: "{summary}",
                    contentPattern: "Resumo: {summary}\nNotas: {notes}\nPrioridade: {priority}\nData: {taskDate}\nConcluida: {isCompleted}",
                    additionalContextPattern: "Filtros atuais, recorrência, contexto da tela e objetivo do usuário.",
                    examplePrompts:
                    [
                        "Quebre essa tarefa em passos menores.",
                        "Sugira o próximo passo mais objetivo.",
                        "Reescreva essa tarefa para ficar menos vaga."
                    ],
                    fields:
                    [
                        Field("summary", "ResourceTitle", "Título ou resumo principal da tarefa.", true),
                        Field("notes", "ResourceContent", "Notas e detalhes da tarefa.", false),
                        Field("priority", "ResourceContent", "Prioridade atual da tarefa.", false),
                        Field("taskDate", "ResourceContent", "Data associada a execução.", false),
                        Field("isCompleted", "ResourceContent", "Status atual de conclusão.", false),
                        Field("screenContext", "AdditionalContext", "Contexto da lista, filtro ou fluxo atual.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Metas, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildGoalsCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Goals,
                Label = "Metas",
                RecommendedUses =
                [
                    "transformar meta abstrata em plano concreto",
                    "definir marcos intermediários",
                    "apontar riscos de meta irrealista",
                    "sugerir indicadores de progresso"
                ],
                SupportedIntents = ["Summarize", "SuggestNextSteps", "Analyze", "Plan"],
                Guardrails =
                [
                    "não prometer resultado garantido",
                    "não superestimar capacidade sem considerar contexto",
                    "explicitar quando faltar prazo, restrição ou critério de sucesso"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "goals",
                    titlePattern: "{summary}",
                    contentPattern: "Meta: {summary}\nDescricao: {description}\nTipo: {type}\nPrioridade: {priority}\nStatus: {status}\nRewardPoints: {rewardPoints}",
                    additionalContextPattern: "Prazo desejado, restrições, motivo da meta e situação atual.",
                    examplePrompts:
                    [
                        "Transforme essa meta em um plano de execução.",
                        "Quais riscos essa meta tem no estado atual?",
                        "Sugira marcos intermediários para acompanhar progresso."
                    ],
                    fields:
                    [
                        Field("summary", "ResourceTitle", "Resumo principal da meta.", true),
                        Field("description", "ResourceContent", "Descrição detalhada da meta.", false),
                        Field("type", "ResourceContent", "Tipo de meta.", false),
                        Field("priority", "ResourceContent", "Prioridade atual.", false),
                        Field("status", "ResourceContent", "Status atual da meta.", false),
                        Field("userConstraints", "AdditionalContext", "Prazo, restrições e contexto do usuário.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Anotações, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildNotesCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Notes,
                Label = "Anotações",
                RecommendedUses =
                [
                    "resumir anotações longas",
                    "reestruturar texto confuso",
                    "extrair checklist ou próximas ações",
                    "converter rascunho em texto mais claro"
                ],
                SupportedIntents = ["Summarize", "Rewrite", "SuggestNextSteps"],
                Guardrails =
                [
                    "preservar sentido original",
                    "sinalizar quando houver ambiguidade no texto",
                    "não adicionar fatos que não estejam no contexto"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "notes",
                    titlePattern: "{subject}",
                    contentPattern: "Assunto: {subject}\nConteudo:\n{content}",
                    additionalContextPattern: "Objetivo da reescrita, publico-alvo ou formato desejado.",
                    examplePrompts:
                    [
                        "Resuma isso em poucos pontos.",
                        "Transforme isso em checklist.",
                        "Reescreva em um texto mais claro e organizado."
                    ],
                    fields:
                    [
                        Field("subject", "ResourceTitle", "Assunto da anotação.", true),
                        Field("content", "ResourceContent", "Conteúdo integral da anotação.", true),
                        Field("desiredFormat", "AdditionalContext", "Formato desejado da saída.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Leituras, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildReadingsCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Readings,
                Label = "Leituras",
                RecommendedUses =
                [
                    "gerar resumo do que foi lido",
                    "sugerir reflexão ou revisão",
                    "extrair aprendizados principais",
                    "propor próximos passos de leitura"
                ],
                SupportedIntents = ["Summarize", "Rewrite", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "não fingir que leu material não enviado",
                    "explicitar quando o contexto for parcial",
                    "não atribuir opiniões ao usuário sem base"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "readings",
                    titlePattern: "{title}",
                    contentPattern: "Titulo: {title}\nAutores: {authors}\nAssunto: {subject}\nResumo salvo: {summary}\nOpiniao: {opinion}\nGenero: {genre}\nFormato: {format}\nStatus: {status}\nPagina atual: {currentPage}/{totalPages}",
                    additionalContextPattern: "Trecho lido recentemente, objetivo de estudo ou motivo da leitura.",
                    examplePrompts:
                    [
                        "Resuma os principais aprendizados desta leitura.",
                        "Sugira perguntas para refletir sobre o que li.",
                        "Qual pode ser o próximo passo de leitura?"
                    ],
                    fields:
                    [
                        Field("title", "ResourceTitle", "Título da leitura.", true),
                        Field("summary", "ResourceContent", "Resumo salvo da leitura.", false),
                        Field("opinion", "ResourceContent", "Opinião registrada pelo usuário.", false),
                        Field("progress", "ResourceContent", "Página atual e total de páginas.", false),
                        Field("recentExcerpt", "AdditionalContext", "Trecho recente ou contexto adicional enviado pelo cliente.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Compras, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildShoppingCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Shopping,
                Label = "Compras",
                RecommendedUses =
                [
                    "agrupar itens por categoria",
                    "detectar duplicidades ou faltas prováveis",
                    "sugerir lista mais economica ou prática",
                    "ajudar no planejamento antes da compra"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps", "Plan"],
                Guardrails =
                [
                    "não assumir preço real sem dado informado",
                    "não excluir item importante sem justificar",
                    "tratar sugestões como apoio, não verdade absoluta"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "shopping",
                    titlePattern: "{name}",
                    contentPattern: "Lista: {name}\nLocal: {place}\nTipo: {type}\nFinalizada: {isFinalized}\nItens:\n{items}",
                    additionalContextPattern: "Quantidade de pessoas, tipo de evento, limite de gasto ou objetivo da compra.",
                    examplePrompts:
                    [
                        "Agrupe essa lista por categorias.",
                        "Veja se há itens duplicados ou faltando.",
                        "Sugira uma forma melhor de organizar essa compra."
                    ],
                    fields:
                    [
                        Field("name", "ResourceTitle", "Nome da lista de compras.", true),
                        Field("items", "ResourceContent", "Itens em formato textual ou estruturado serializado.", true),
                        Field("place", "ResourceContent", "Local planejado da compra.", false),
                        Field("budget", "AdditionalContext", "Limite de gasto ou objetivo financeiro da compra.", false),
                        Field("shoppingContext", "AdditionalContext", "Contexto da compra, como mercado do mês ou evento.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Finanças, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildFinancesCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Finances,
                Label = "Finanças",
                RecommendedUses =
                [
                    "explicar categorias e padrões de gastos",
                    "resumir o mês financeiro do usuário",
                    "apontar concentrações de despesa",
                    "sugerir organização e perguntas para revisão financeira"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "não oferecer recomendação de investimento personalizada",
                    "não tratar educação financeira como consultoria profissional",
                    "destacar limites quando faltarem histórico, renda ou contexto"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "finances",
                    titlePattern: "{monthLabel}",
                    contentPattern: "Periodo: {monthLabel}\nResumo mensal: creditos={totalCredits}, debitos={totalDebits}, saldo={balance}\nLancamentos:\n{entries}\nCategorias:\n{categories}",
                    additionalContextPattern: "Objetivo do usuário, renda aproximada, preocupações do mês e perguntas específicas para revisão.",
                    examplePrompts:
                    [
                        "Resuma meu mês financeiro de forma clara.",
                        "Quais padrões de gasto aparecem aqui?",
                        "Que perguntas eu deveria fazer na minha revisão financeira?"
                    ],
                    fields:
                    [
                        Field("monthLabel", "ResourceTitle", "Título do período, como Abril 2026.", true),
                        Field("monthlySummary", "ResourceContent", "Totais de crédito, débito e saldo.", true),
                        Field("entries", "ResourceContent", "Lançamentos relevantes do período.", true),
                        Field("categories", "ResourceContent", "Agrupamentos por categoria, se o cliente tiver.", false),
                        Field("financialContext", "AdditionalContext", "Contexto educacional e limites da análise.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso de Perfil do Usuário, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildUserProfileCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.UserProfile,
                Label = "Perfil e uso",
                RecommendedUses =
                [
                    "explicar configurações do usuário",
                    "resumir histórico recente de uso",
                    "sugerir ajustes de organização pessoal",
                    "relacionar padrões entre recursos"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "não inferir perfil psicológico ou clínico",
                    "não usar linguagem invasiva",
                    "respeitar o escopo estrito dos dados do sistema"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "userprofile",
                    titlePattern: "{userName}",
                    contentPattern: "Usuario: {userName}\nConfiguracoes: {settings}\nPontos: {bonusSummary}\nResumo de uso: {usageSummary}",
                    additionalContextPattern: "Pergunta do usuário sobre organização, preferência ou histórico.",
                    examplePrompts:
                    [
                        "Explique minhas configurações atuais.",
                        "Resuma meu uso recente no sistema.",
                        "Sugira ajustes para me organizar melhor."
                    ],
                    fields:
                    [
                        Field("userName", "ResourceTitle", "Nome ou identificador amigável do usuário.", true),
                        Field("settings", "ResourceContent", "Configurações atuais do usuário.", true),
                        Field("bonusSummary", "ResourceContent", "Resumo de pontos e conquistas.", false),
                        Field("usageSummary", "ResourceContent", "Resumo agregado de uso vindo do cliente.", false)
                    ])
            };
        }

        /// <summary>
        /// Monta a capacidade da IA para o recurso Geral, incluindo usos recomendados, intenções suportadas, diretrizes de segurança e um modelo de payload para solicitações.
        /// </summary>
        /// <returns></returns>
        private static AiResourceCapabilityResponse BuildGeneralCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.General,
                Label = "Assistência geral",
                RecommendedUses =
                [
                    "orientar o usuário no uso prático do sistema",
                    "ajudar a escolher o melhor fluxo por recurso",
                    "traduzir dados do sistema em próximas acoes"
                ],
                SupportedIntents = ["GeneralHelp", "Summarize", "SuggestNextSteps"],
                Guardrails =
                [
                    "encaminhar para recurso específico quando houver contexto suficiente",
                    "evitar respostas genéricas demais",
                    "manter foco no valor do eTasks"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "ai",
                    titlePattern: "{screenOrFlow}",
                    contentPattern: "Tela ou fluxo: {screenOrFlow}\nContexto atual: {currentContext}",
                    additionalContextPattern: "Objetivo do usuário e recurso que ele está tentando usar.",
                    examplePrompts:
                    [
                        "Como devo usar melhor esse recurso?",
                        "Qual é o melhor próximo passo aqui?",
                        "Em qual recurso do app essa necessidade se encaixa melhor?"
                    ],
                    fields:
                    [
                        Field("screenOrFlow", "ResourceTitle", "Tela atual ou fluxo atual do cliente.", true),
                        Field("currentContext", "ResourceContent", "Resumo do estado atual do app ou do que o usuário está vendo.", true),
                        Field("userGoal", "AdditionalContext", "Intenção do usuário naquele momento.", false)
                    ])
            };
        }

        /// <summary>
        /// Constrói um modelo de payload para solicitações de assistência da IA, incluindo o padrão de rota, método HTTP, padrões sugeridos para título, conteúdo e contexto adicional, exemplos de prompts e campos esperados.
        /// </summary>
        /// <param name="resource">recurso para o qual construir o modelo de payload</param>
        /// <param name="titlePattern">padrão sugerido para o título do recurso</param>
        /// <param name="contentPattern">padrão sugerido para o conteúdo do recurso</param>
        /// <param name="additionalContextPattern">padrão sugerido para o contexto adicional</param>
        /// <param name="examplePrompts">lista de exemplos de prompts</param>
        /// <param name="fields">lista de campos esperados no payload</param>
        /// <returns></returns>
        private static AiPayloadTemplateResponse BuildTemplate(
            string resource,
            string titlePattern,
            string contentPattern,
            string additionalContextPattern,
            List<string> examplePrompts,
            List<AiPayloadFieldResponse> fields)
        {
            return new AiPayloadTemplateResponse
            {
                RoutePattern = resource == "ai" ? "/api/v2/ai/assist" : $"/api/v2/ai/{resource}/assist",
                Method = "POST",
                SuggestedResourceTitlePattern = titlePattern,
                SuggestedResourceContentPattern = contentPattern,
                SuggestedAdditionalContextPattern = additionalContextPattern,
                ExamplePrompts = examplePrompts,
                Fields = fields
            };
        }

        /// <summary>
        /// Constrói um campo de payload esperado para solicitações de assistência da IA, incluindo nome, propriedade de destino, descrição e se é obrigatório ou não.
        /// </summary>
        /// <param name="name">nome do campo</param>
        /// <param name="targetProperty">propriedade de destino do campo</param>
        /// <param name="description">descrição do campo</param>
        /// <param name="required">indica se o campo é obrigatório</param>
        /// <returns></returns>
        private static AiPayloadFieldResponse Field(string name, string targetProperty, string description, bool required)
        {
            return new AiPayloadFieldResponse
            {
                Name = name,
                TargetProperty = targetProperty,
                Description = description,
                Required = required
            };
        }
        #endregion
    }
}
