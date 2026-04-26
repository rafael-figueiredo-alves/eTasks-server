using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;

namespace eTasks_server.Core.Services
{
    public class AiCapabilityCatalog : IAiCapabilityCatalog
    {
        public AiCapabilitiesResponse GetCapabilities()
        {
            return new AiCapabilitiesResponse
            {
                ProviderMode = "OpenRouter",
                CrossCuttingGuidance =
                [
                    "A IA deve ajudar o usuario a decidir e agir melhor, nao apenas conversar.",
                    "O uso ideal e sempre contextual ao recurso atual da tela ou do fluxo.",
                    "Respostas devem ser curtas, acionaveis e alinhadas ao contexto salvo no eTasks.",
                    "A IA nao deve inventar dados ausentes nem agir como fonte definitiva para temas sensiveis.",
                    "Em financas, a IA deve priorizar educacao e organizacao, nao aconselhamento financeiro definitivo."
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

        private static AiResourceCapabilityResponse BuildTasksCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Tasks,
                Label = "Tarefas",
                RecommendedUses =
                [
                    "quebrar uma tarefa grande em subtarefas",
                    "reescrever tarefas vagas em acoes objetivas",
                    "sugerir prioridade e ordem de execucao",
                    "propor o proximo passo quando houver bloqueio"
                ],
                SupportedIntents = ["Rewrite", "SuggestNextSteps", "Analyze", "Plan"],
                Guardrails =
                [
                    "nao mudar datas ou prioridades sem explicar o criterio",
                    "nao marcar tarefa como concluida automaticamente",
                    "nao criar volume excessivo de subtarefas sem ganho real"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "tasks",
                    titlePattern: "{summary}",
                    contentPattern: "Resumo: {summary}\nNotas: {notes}\nPrioridade: {priority}\nData: {taskDate}\nConcluida: {isCompleted}",
                    additionalContextPattern: "Filtros atuais, recorrencia, contexto da tela e objetivo do usuario.",
                    examplePrompts:
                    [
                        "Quebre essa tarefa em passos menores.",
                        "Sugira o proximo passo mais objetivo.",
                        "Reescreva essa tarefa para ficar menos vaga."
                    ],
                    fields:
                    [
                        Field("summary", "ResourceTitle", "Titulo ou resumo principal da tarefa.", true),
                        Field("notes", "ResourceContent", "Notas e detalhes da tarefa.", false),
                        Field("priority", "ResourceContent", "Prioridade atual da tarefa.", false),
                        Field("taskDate", "ResourceContent", "Data associada a execucao.", false),
                        Field("isCompleted", "ResourceContent", "Status atual de conclusao.", false),
                        Field("screenContext", "AdditionalContext", "Contexto da lista, filtro ou fluxo atual.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildGoalsCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Goals,
                Label = "Metas",
                RecommendedUses =
                [
                    "transformar meta abstrata em plano concreto",
                    "definir marcos intermediarios",
                    "apontar riscos de meta irrealista",
                    "sugerir indicadores de progresso"
                ],
                SupportedIntents = ["Summarize", "SuggestNextSteps", "Analyze", "Plan"],
                Guardrails =
                [
                    "nao prometer resultado garantido",
                    "nao superestimar capacidade sem considerar contexto",
                    "explicitar quando faltar prazo, restricao ou criterio de sucesso"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "goals",
                    titlePattern: "{summary}",
                    contentPattern: "Meta: {summary}\nDescricao: {description}\nTipo: {type}\nPrioridade: {priority}\nStatus: {status}\nRewardPoints: {rewardPoints}",
                    additionalContextPattern: "Prazo desejado, restricoes, motivo da meta e situacao atual.",
                    examplePrompts:
                    [
                        "Transforme essa meta em um plano de execucao.",
                        "Quais riscos essa meta tem no estado atual?",
                        "Sugira marcos intermediarios para acompanhar progresso."
                    ],
                    fields:
                    [
                        Field("summary", "ResourceTitle", "Resumo principal da meta.", true),
                        Field("description", "ResourceContent", "Descricao detalhada da meta.", false),
                        Field("type", "ResourceContent", "Tipo de meta.", false),
                        Field("priority", "ResourceContent", "Prioridade atual.", false),
                        Field("status", "ResourceContent", "Status atual da meta.", false),
                        Field("userConstraints", "AdditionalContext", "Prazo, restricoes e contexto do usuario.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildNotesCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Notes,
                Label = "Anotacoes",
                RecommendedUses =
                [
                    "resumir anotacoes longas",
                    "reestruturar texto confuso",
                    "extrair checklist ou proximas acoes",
                    "converter rascunho em texto mais claro"
                ],
                SupportedIntents = ["Summarize", "Rewrite", "SuggestNextSteps"],
                Guardrails =
                [
                    "preservar sentido original",
                    "sinalizar quando houver ambiguidade no texto",
                    "nao adicionar fatos que nao estejam no contexto"
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
                        Field("subject", "ResourceTitle", "Assunto da anotacao.", true),
                        Field("content", "ResourceContent", "Conteudo integral da anotacao.", true),
                        Field("desiredFormat", "AdditionalContext", "Formato desejado da saida.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildReadingsCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Readings,
                Label = "Leituras",
                RecommendedUses =
                [
                    "gerar resumo do que foi lido",
                    "sugerir reflexao ou revisao",
                    "extrair aprendizados principais",
                    "propor proximos passos de leitura"
                ],
                SupportedIntents = ["Summarize", "Rewrite", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "nao fingir que leu material nao enviado",
                    "explicitar quando o contexto for parcial",
                    "nao atribuir opinioes ao usuario sem base"
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
                        "Qual pode ser o proximo passo de leitura?"
                    ],
                    fields:
                    [
                        Field("title", "ResourceTitle", "Titulo da leitura.", true),
                        Field("summary", "ResourceContent", "Resumo salvo da leitura.", false),
                        Field("opinion", "ResourceContent", "Opiniao registrada pelo usuario.", false),
                        Field("progress", "ResourceContent", "Pagina atual e total de paginas.", false),
                        Field("recentExcerpt", "AdditionalContext", "Trecho recente ou contexto adicional enviado pelo cliente.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildShoppingCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Shopping,
                Label = "Compras",
                RecommendedUses =
                [
                    "agrupar itens por categoria",
                    "detectar duplicidades ou faltas provaveis",
                    "sugerir lista mais economica ou pratica",
                    "ajudar no planejamento antes da compra"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps", "Plan"],
                Guardrails =
                [
                    "nao assumir preco real sem dado informado",
                    "nao excluir item importante sem justificar",
                    "tratar sugestoes como apoio, nao verdade absoluta"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "shopping",
                    titlePattern: "{name}",
                    contentPattern: "Lista: {name}\nLocal: {place}\nTipo: {type}\nFinalizada: {isFinalized}\nItens:\n{items}",
                    additionalContextPattern: "Quantidade de pessoas, tipo de evento, limite de gasto ou objetivo da compra.",
                    examplePrompts:
                    [
                        "Agrupe essa lista por categorias.",
                        "Veja se ha itens duplicados ou faltando.",
                        "Sugira uma forma melhor de organizar essa compra."
                    ],
                    fields:
                    [
                        Field("name", "ResourceTitle", "Nome da lista de compras.", true),
                        Field("items", "ResourceContent", "Itens em formato textual ou estruturado serializado.", true),
                        Field("place", "ResourceContent", "Local planejado da compra.", false),
                        Field("budget", "AdditionalContext", "Limite de gasto ou objetivo financeiro da compra.", false),
                        Field("shoppingContext", "AdditionalContext", "Contexto da compra, como mercado do mes ou evento.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildFinancesCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.Finances,
                Label = "Financas",
                RecommendedUses =
                [
                    "explicar categorias e padroes de gastos",
                    "resumir o mes financeiro do usuario",
                    "apontar concentracoes de despesa",
                    "sugerir organizacao e perguntas para revisao financeira"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "nao oferecer recomendacao de investimento personalizada",
                    "nao tratar educacao financeira como consultoria profissional",
                    "destacar limites quando faltarem historico, renda ou contexto"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "finances",
                    titlePattern: "{monthLabel}",
                    contentPattern: "Periodo: {monthLabel}\nResumo mensal: creditos={totalCredits}, debitos={totalDebits}, saldo={balance}\nLancamentos:\n{entries}\nCategorias:\n{categories}",
                    additionalContextPattern: "Objetivo do usuario, renda aproximada, preocupacoes do mes e perguntas especificas para revisao.",
                    examplePrompts:
                    [
                        "Resuma meu mes financeiro de forma clara.",
                        "Quais padroes de gasto aparecem aqui?",
                        "Que perguntas eu deveria fazer na minha revisao financeira?"
                    ],
                    fields:
                    [
                        Field("monthLabel", "ResourceTitle", "Titulo do periodo, como Abril 2026.", true),
                        Field("monthlySummary", "ResourceContent", "Totais de credito, debito e saldo.", true),
                        Field("entries", "ResourceContent", "Lancamentos relevantes do periodo.", true),
                        Field("categories", "ResourceContent", "Agrupamentos por categoria, se o cliente tiver.", false),
                        Field("financialContext", "AdditionalContext", "Contexto educacional e limites da analise.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildUserProfileCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.UserProfile,
                Label = "Perfil e uso",
                RecommendedUses =
                [
                    "explicar configuracoes do usuario",
                    "resumir historico recente de uso",
                    "sugerir ajustes de organizacao pessoal",
                    "relacionar padroes entre recursos"
                ],
                SupportedIntents = ["Summarize", "Analyze", "SuggestNextSteps"],
                Guardrails =
                [
                    "nao inferir perfil psicologico ou clinico",
                    "nao usar linguagem invasiva",
                    "respeitar o escopo estrito dos dados do sistema"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "userprofile",
                    titlePattern: "{userName}",
                    contentPattern: "Usuario: {userName}\nConfiguracoes: {settings}\nPontos: {bonusSummary}\nResumo de uso: {usageSummary}",
                    additionalContextPattern: "Pergunta do usuario sobre organizacao, preferencia ou historico.",
                    examplePrompts:
                    [
                        "Explique minhas configuracoes atuais.",
                        "Resuma meu uso recente no sistema.",
                        "Sugira ajustes para me organizar melhor."
                    ],
                    fields:
                    [
                        Field("userName", "ResourceTitle", "Nome ou identificador amigavel do usuario.", true),
                        Field("settings", "ResourceContent", "Configuracoes atuais do usuario.", true),
                        Field("bonusSummary", "ResourceContent", "Resumo de pontos e conquistas.", false),
                        Field("usageSummary", "ResourceContent", "Resumo agregado de uso vindo do cliente.", false)
                    ])
            };
        }

        private static AiResourceCapabilityResponse BuildGeneralCapability()
        {
            return new AiResourceCapabilityResponse
            {
                Resource = AiResourceType.General,
                Label = "Assistencia geral",
                RecommendedUses =
                [
                    "orientar o usuario no uso pratico do sistema",
                    "ajudar a escolher o melhor fluxo por recurso",
                    "traduzir dados do sistema em proximas acoes"
                ],
                SupportedIntents = ["GeneralHelp", "Summarize", "SuggestNextSteps"],
                Guardrails =
                [
                    "encaminhar para recurso especifico quando houver contexto suficiente",
                    "evitar respostas genericas demais",
                    "manter foco no valor do eTasks"
                ],
                PayloadTemplate = BuildTemplate(
                    resource: "ai",
                    titlePattern: "{screenOrFlow}",
                    contentPattern: "Tela ou fluxo: {screenOrFlow}\nContexto atual: {currentContext}",
                    additionalContextPattern: "Objetivo do usuario e recurso que ele esta tentando usar.",
                    examplePrompts:
                    [
                        "Como devo usar melhor esse recurso?",
                        "Qual e o melhor proximo passo aqui?",
                        "Em qual recurso do app essa necessidade se encaixa melhor?"
                    ],
                    fields:
                    [
                        Field("screenOrFlow", "ResourceTitle", "Tela atual ou fluxo atual do cliente.", true),
                        Field("currentContext", "ResourceContent", "Resumo do estado atual do app ou do que o usuario esta vendo.", true),
                        Field("userGoal", "AdditionalContext", "Intencao do usuario naquele momento.", false)
                    ])
            };
        }

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
    }
}
