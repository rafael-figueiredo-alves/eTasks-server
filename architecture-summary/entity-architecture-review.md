# Revisao Arquitetural das Entidades Persistidas

Data de referencia: 2026-04-08

Base analisada:

- `eTasks-server.Models/Entities`
- `eTasks-server.Core/Data/AppDbContext.cs`

## Escopo

Este documento ignora DTOs e foca apenas nas entidades relacionadas ao banco de dados atualmente mapeadas pelo `AppDbContext`.

## Inventario Atual

Entidades mapeadas:

1. `eTasksVersion`
2. `User`
3. `RefreshToken`
4. `PasswordResetCode`
5. `LoginLog`
6. `UserSettings`
7. `UserBonusPoint`
8. `BonusAchievement`
9. `UserAchievement`
10. `BonusPointRule`
11. `TaskItem`
12. `TaskRecurrence`
13. `Goal`
14. `ShoppingList`
15. `ShoppingListItem`
16. `NoteItem`
17. `ReadingItem`
18. `FinanceEntry`

## Visao de Dominio

O modelo atual pode ser lido em oito blocos:

1. `Configuracao Global`
   `eTasksVersion`
2. `Identidade e Conta`
   `User`
3. `Autenticacao e Auditoria`
   `RefreshToken`, `PasswordResetCode`, `LoginLog`
4. `Perfil`
   `UserSettings`
5. `Gamificacao`
   `BonusPointRule`, `UserBonusPoint`, `BonusAchievement`, `UserAchievement`
6. `Produtividade`
   `TaskItem`, `TaskRecurrence`, `Goal`
7. `Organizacao Pessoal`
   `ShoppingList`, `ShoppingListItem`, `NoteItem`, `ReadingItem`
8. `Financas`
   `FinanceEntry`

Essa divisao ja e suficiente para orientar futuras separacoes modulares, mesmo que hoje tudo ainda esteja concentrado sob o mesmo contexto.

## Diagrama de Relacoes

```text
                       +----------------------+
                       |    eTasksVersion     |
                       | singleton config     |
                       +----------------------+

                       +----------------------+
                       |   BonusPointRule     |
                       | regras de pontos     |
                       +----------------------+

 +------------------+        1 ----- 1        +------------------+
 |       User       |-------------------------|   UserSettings   |
 | agregado central |                         | preferencias     |
 +------------------+                         +------------------+
          |
          | 1 ----- N
          +---------------------------> RefreshToken
          +---------------------------> PasswordResetCode
          +---------------------------> LoginLog
          +---------------------------> UserBonusPoint
          +---------------------------> UserAchievement ---- N:1 ---- BonusAchievement
          +---------------------------> TaskItem ---------- 1:1 ---- TaskRecurrence
          |                                   |
          |                                   +---- 1:N ---- TaskItem (filhas geradas)
          +---------------------------> Goal
          +---------------------------> ShoppingList ------ 1:N ---- ShoppingListItem
          +---------------------------> NoteItem
          +---------------------------> ReadingItem
          +---------------------------> FinanceEntry
```

Observacao:

- o diagrama acima representa ownership e mapeamentos atuais, nao necessariamente os limites ideais de modulo

## Analise por Bloco

### `Configuracao Global`

`eTasksVersion` continua sendo uma entidade singleton usada como configuracao persistida da versao do app.

Leitura arquitetural:

- baixa complexidade
- baixo risco
- nao e agregado de negocio rico

### `Identidade e Conta`

`User` continua sendo o agregado central do sistema.

Concentra hoje:

- identidade
- credencial
- autorizacao
- estado da conta
- ownership dos recursos pessoais

Forca do modelo:

- simples de consultar
- ownership claro
- facilita filtros por usuario autenticado

Risco estrutural:

- alto acoplamento
- crescimento excessivo do papel de `User`

### `Autenticacao e Auditoria`

`RefreshToken`, `PasswordResetCode` e `LoginLog` continuam formando um bloco bastante coerente.

Pontos positivos:

- escopo claro
- responsabilidade operacional bem definida
- candidato natural a modularizacao futura

Risco estrutural:

- baixo a medio
- `LoginLog` pode crescer em volume mais rapidamente do que o restante do dominio

### `Perfil`

`UserSettings` segue como boa extensao 1:1 do usuario.

Pontos positivos:

- evita poluir `User` com preferencias
- e um bom padrao para futuras separacoes

### `Gamificacao`

O bloco de gamificacao ficou mais maduro com a inclusao de `BonusPointRule`.

Papeis:

- `BonusPointRule`: regra central por origem
- `UserBonusPoint`: ledger de pontos
- `BonusAchievement`: catalogo de conquistas
- `UserAchievement`: historico de conquista adquirida

Pontos positivos:

- fronteira de subdominio mais clara
- boa rastreabilidade
- melhor base para trocar valores de pontuacao sem hardcode espalhado

Risco estrutural:

- medio
- se o volume de pontos crescer muito, pode surgir necessidade de saldo materializado

### `Produtividade`

Este bloco cobre tarefas e metas.

`TaskItem`:

- representa a tarefa concreta mostrada ao usuario
- suporta autorrelacao para tarefas filhas geradas

`TaskRecurrence`:

- isola a configuracao de recorrencia da tarefa base
- mantem a entidade de tarefa mais limpa para casos nao recorrentes

`Goal`:

- modela objetivos simples com resumo, descricao, tipo, prioridade, pontuacao opcional e status

Pontos positivos:

- boa separacao entre instancia de tarefa e regra de recorrencia
- metas entram como recurso proprio, sem acoplamento direto com tarefas
- o recurso foi mantido propositalmente simples, sem prazo, progresso numerico ou estrutura de subtarefas

Risco estrutural:

- medio
- pode surgir debate futuro sobre unificar motor de recorrencia entre tarefas e financas

### `Organizacao Pessoal`

Este bloco cobre compras, anotacoes e leituras.

`ShoppingList` + `ShoppingListItem`:

- bom desenho agregado pai-filho
- ownership e status de conclusao estao claros

`NoteItem`:

- entidade simples e coesa
- sem mistura indevida com gamificacao
- hoje foi reduzida ao essencial: assunto, texto e datas de criacao/edicao

`ReadingItem`:

- combina progresso, status e recompensa opcional
- encaixa bem entre conhecimento pessoal e gamificacao

Risco estrutural:

- baixo

### `Financas`

`FinanceEntry` concentra entradas e saidas financeiras no mesmo modelo.

Pontos positivos:

- leitura simples
- cobre recorrencia sem tabela adicional
- pronto para calcular saldo por periodo
- agora tambem distingue forma de pagamento no proprio lançamento

Risco estrutural:

- medio
- dependendo do crescimento, pode valer separar no futuro categorias, contas ou recorrencias em entidades proprias

## Analise de Acoplamento

### Centro gravitacional

Hoje `User` e o centro gravitacional do banco.

Impactos disso:

- auth depende de `User`
- gamificacao depende de `User`
- tarefas dependem de `User`
- metas dependem de `User`
- compras dependem de `User`
- notas dependem de `User`
- leituras dependem de `User`
- financas dependem de `User`

Isso nao e necessariamente um erro, mas confirma um desenho fortemente orientado a dados pessoais por conta.

### Coesao dos blocos

Blocos com boa coesao interna:

- `RefreshToken` + `PasswordResetCode` + `LoginLog`
- `BonusPointRule` + `UserBonusPoint` + `BonusAchievement` + `UserAchievement`
- `TaskItem` + `TaskRecurrence`
- `ShoppingList` + `ShoppingListItem`

Bloco com maior risco de centralidade:

- `User`

## Sugestoes de Modularizacao

Estas sugestoes nao implicam refactor imediato. Sao opcoes de evolucao.

### Opcao 1. Modularizacao logica sem quebrar banco agora

Objetivo:

- manter o schema atual
- separar melhor responsabilidades no codigo

Direcao:

- modulo `Identity`
- modulo `Auth`
- modulo `Profile`
- modulo `Gamification`
- modulo `Tasks`
- modulo `Goals`
- modulo `Shopping`
- modulo `Notes`
- modulo `Readings`
- modulo `Finances`
- modulo `AppConfig`

### Opcao 2. Preservar `User` como owner, mas nao como concentrador de regras

Objetivo:

- manter a relacao por `UserUid`
- impedir que toda regra nova caia no agregado central

Direcao:

- `User` fica mais focado em identidade e estado da conta
- regras de negocio passam a viver nos subdominios correspondentes

### Opcao 3. Padronizar a ideia de recorrencia no futuro

Objetivo:

- avaliar se o sistema deve ter um modelo unico de recorrencia

Cenario atual:

- tarefas usam `TaskRecurrence`
- financas usam recorrencia embutida em `FinanceEntry`

Comentario:

- a diferenca faz sentido agora
- pode deixar de fazer sentido se mais recursos recorrentes surgirem

### Opcao 4. Preparar recursos compartilhados sem alterar o desenho atual prematuramente

Objetivo:

- manter a simplicidade atual
- admitir evolucao futura para ownership multiplo

Comentario:

- hoje quase tudo e pessoal
- se surgirem listas compartilhadas, tarefas colaborativas ou metas em grupo, o caminho natural sera criar entidades associativas especificas

## Recomendacao Pragmatica

Se o objetivo for tomar decisoes estruturais sem refactor excessivo agora, a melhor sequencia parece ser:

1. manter o schema atual como base do produto
2. modularizar a camada de negocio por subdominio antes de modularizar fisicamente o banco
3. impedir que novas regras de tarefas, metas, compras, leituras e financas entrem diretamente em `User`
4. tratar `Gamification`, `Tasks` e `Finances` como candidatos naturais a crescimento independente
5. revisar mais adiante se ownership individual continua suficiente para todos os recursos

## Decisoes Estruturais que Valem Discussao

1. `User` deve continuar sendo o unico owner de quase todo o dominio?
2. o sistema deve unificar a estrategia de recorrencia entre subdominios?
3. `BonusPointRule` sera apenas configuracao interna ou tera administracao explicita?
4. compras e leituras devem continuar com pontos opcionais ou migrar para regra central obrigatoria?
5. financas deve continuar em um unico agregado simples ou crescer para contas, categorias e centros de custo?

## Resumo Executivo

O modelo atual esta coerente com o produto descrito: um sistema pessoal de organizacao e produtividade com gamificacao. O principal risco nao esta em entidades individuais mal desenhadas, mas na centralidade excessiva de `User` e na necessidade futura de modularizar melhor os subdominios.

Se eu tivesse que resumir a recomendacao em uma linha:

> O proximo passo mais valioso nao e quebrar o banco imediatamente, e sim consolidar os subdominios do produto em torno de ownership por usuario, evitando que `User` vire o lugar onde toda regra nova passa a morar.

## Arquivos de Referencia

- `eTasks-server.Models/Entities/Version/eTasksVersion.cs`
- `eTasks-server.Models/Entities/Users/User.cs`
- `eTasks-server.Models/Entities/Users/RefreshToken.cs`
- `eTasks-server.Models/Entities/Users/PasswordResetCode.cs`
- `eTasks-server.Models/Entities/Users/LoginLog.cs`
- `eTasks-server.Models/Entities/Users/UserSettings.cs`
- `eTasks-server.Models/Entities/Gamification/BonusPointRule.cs`
- `eTasks-server.Models/Entities/Users/UserBonusPoint.cs`
- `eTasks-server.Models/Entities/Users/BonusAchievement.cs`
- `eTasks-server.Models/Entities/Users/UserAchievement.cs`
- `eTasks-server.Models/Entities/Productivity/TaskItem.cs`
- `eTasks-server.Models/Entities/Productivity/TaskRecurrence.cs`
- `eTasks-server.Models/Entities/Goals/Goal.cs`
- `eTasks-server.Models/Entities/Shopping/ShoppingList.cs`
- `eTasks-server.Models/Entities/Shopping/ShoppingListItem.cs`
- `eTasks-server.Models/Entities/Notes/NoteItem.cs`
- `eTasks-server.Models/Entities/Readings/ReadingItem.cs`
- `eTasks-server.Models/Entities/Finances/FinanceEntry.cs`
- `eTasks-server.Core/Data/AppDbContext.cs`
