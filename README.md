<div align="center">

<img src="https://github.com/rafael-figueiredo-alves/eTasks/blob/main/assets/eTasks_logo_new.png" alt="eTasks Logo" width="180"/>

# eTasks Server

### Backend ASP.NET Core para o ecossistema eTasks

<br/>

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/rafael-figueiredo-alves/eTasks-server?include_prereleases&color=%230d6efd&label=versao&style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server/releases)
[![GitHub Stars](https://img.shields.io/github/stars/rafael-figueiredo-alves/eTasks-server?color=yellow&style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/rafael-figueiredo-alves/eTasks-server?color=%230d6efd&style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server/network/members)
[![GitHub Issues](https://img.shields.io/github/issues/rafael-figueiredo-alves/eTasks-server?color=red&style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server/issues)
[![GitHub License](https://img.shields.io/github/license/rafael-figueiredo-alves/eTasks-server?style=for-the-badge)](LICENSE)
[![GitHub last commit](https://img.shields.io/github/last-commit/rafael-figueiredo-alves/eTasks-server?style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server/commits)

<br/>

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)

</div>

---

## Indice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Ecossistema eTasks](#ecossistema-etasks)
- [Clientes Suportados](#clientes-suportados)
- [Documentacao da API](#documentacao-da-api)
- [Como Comecar](#como-comecar)
- [Pre-requisitos](#pre-requisitos)
- [Instalacao](#instalacao)
- [Configuracao](#configuracao)
- [Arquitetura](#arquitetura)
- [Roadmap](#roadmap)
- [Contribuindo](#contribuindo)
- [Autor](#autor)
- [Licenca](#licenca)

---

## Sobre o Projeto

O **eTasks Server** e o backend do ecossistema eTasks. Ele centraliza autenticacao, regras de negocio, persistencia de dados, sincronizacao dos clientes e o painel administrativo web.

O projeto e construido com **ASP.NET Core**, **Minimal APIs**, **Blazor Server interativo** para o painel administrativo, **EF Core** e **MySQL**. A API principal fica versionada em `/api/v2` e atende clientes externos como app web/PWA, Windows e Android.

O servidor tambem hospeda um painel administrativo no proprio projeto web. Esse painel usa autenticacao por cookie, enquanto os clientes externos usam JWT com refresh token.

---

## Funcionalidades

- **Autenticacao externa com JWT**: login, registro, confirmacao de conta, refresh token, logout, recuperacao e troca de senha.
- **Autenticacao web administrativa com cookie**: login tradicional por formulario, logout e protecao do painel administrativo.
- **Sincronizacao de recursos do usuario**: tarefas, metas, notas, leituras, listas de compras e financas.
- **Perfil do usuario**: dados cadastrais, foto de perfil, configuracoes, bonus e exportacao de dados.
- **Painel administrativo Blazor**: dashboard, usuarios, versoes, configuracoes do servidor, banco de dados, logs, auditoria, bonus e notificacoes.
- **Gamificacao**: regras de pontuacao, conquistas e resumo de bonus.
- **Notificacoes**: cadastro de dispositivos e envio/gestao de notificacoes administrativas.
- **Assistente de IA**: endpoints e servicos para assistencia contextual usando provedor configuravel.
- **Logs e auditoria**: Serilog, logs em tempo real no painel, retencao de logs e auditoria operacional com MongoDB opcional.
- **Documentacao interativa**: OpenAPI e Scalar protegidos por autenticacao.
- **Health check**: monitoramento basico da aplicacao e banco.

---

## Tecnologias Utilizadas

| Tecnologia | Uso no projeto |
|---|---|
| **C# / .NET 10** | Linguagem e runtime da solucao |
| **ASP.NET Core** | Host web, pipeline HTTP, autenticacao e autorizacao |
| **Minimal APIs** | Endpoints versionados em `/api/v2` |
| **Blazor Server / Razor Components** | Painel administrativo hospedado no proprio servidor |
| **MudBlazor** | Componentes visuais do painel administrativo |
| **Entity Framework Core** | Acesso a dados e mapeamento das entidades |
| **MySQL / Pomelo EF Core** | Banco relacional principal |
| **Serilog** | Logs estruturados em console, arquivo e painel em tempo real |
| **Scalar / OpenAPI** | Referencia interativa da API |
| **MongoDB Atlas opcional** | Auditoria operacional, quando configurada |
| **SMTP** | Envio de e-mails de confirmacao e recuperacao de senha |

---

## Ecossistema eTasks

O eTasks e formado por clientes e servicos que compartilham o mesmo dominio de produtividade pessoal.

| Projeto | Papel |
|---|---|
| **eTasks Server** | Este repositorio. Backend, API e painel administrativo |
| **eTasks** | Aplicativo nativo Delphi para Windows e Android |
| **eTasks Web/PWA** | Cliente web que consome a API |

Este repositorio substitui a dependencia historica de backend externo para os fluxos principais do eTasks, concentrando dados, autenticacao e administracao em uma API propria.

---

## Clientes Suportados

| Cliente | Autenticacao | Observacao |
|---|---|---|
| Web/PWA | JWT | User agent esperado: `web` |
| Windows Delphi | JWT | User agent esperado: `windows` |
| Android Delphi | JWT | User agent esperado: `android` |
| Painel administrativo | Cookie | User agent administrativo e politicas web |

Os fluxos sao separados de proposito: clientes externos usam `Authorization: Bearer`, enquanto o painel administrativo usa cookie auth.

---

## Documentacao da API

A API e registrada sob o prefixo:

```text
/api/v2
```

Grupos principais de endpoints:

- `auth`
- `web-auth`
- `usuarios`
- `users`
- `tasks`
- `goals`
- `notes`
- `readings`
- `shopping`
- `finances`
- `notifications`
- `ai`
- `dashboard`
- `database`
- `application-logs`
- `operation-audit`
- `bonus`
- `version`
- `utils`

A referencia OpenAPI e publicada pelo app, e a interface Scalar fica em:

```text
/docs
```

O acesso a documentacao interativa e protegido por autenticacao.

---

## Como Comecar

### Pre-requisitos

Para compilar e executar o projeto localmente, voce precisa de:

- **.NET SDK 10**
- **MySQL** acessivel para a connection string configurada
- Opcional: **MongoDB Atlas** para auditoria operacional
- Opcional: servidor **SMTP** para envio real de e-mails

### Instalacao

1. Clone o repositorio:

```bash
git clone https://github.com/rafael-figueiredo-alves/eTasks-server.git
cd eTasks-server
```

2. Restaure os pacotes:

```bash
dotnet restore eTasks-server.slnx
```

3. Compile a solucao:

```bash
dotnet build eTasks-server.slnx
```

4. Execute o host web:

```bash
dotnet run --project eTasks-server/eTasks-server/eTasks-server.csproj
```

### Configuracao

As principais configuracoes ficam em `appsettings.json`, `appsettings.Development.json`, variaveis de ambiente ou provider equivalente no ambiente de hospedagem.

Configuracoes importantes:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `APIKEY_ADMIN`
- `ApiSettings:BaseUrl`
- `ApiSettings:ApiV2Path`
- `Security:DataEncryptionKey`
- `Smtp:*`

Nao use os valores de exemplo em producao. Segredos devem ser fornecidos por variaveis de ambiente, secrets manager ou configuracao segura do provedor de hospedagem.

---

## Arquitetura

A solucao e organizada em projetos com responsabilidades separadas:

```text
eTasks-server/
+-- eTasks-server.slnx
+-- eTasks-server/
|   +-- eTasks-server/
|       +-- Program.cs
|       +-- Extensions/
|       +-- Endpoints/
|       +-- Components/
|       +-- Client/
|       +-- wwwroot/
+-- eTasks-server.Core/
|   +-- BusinessLogicLayers/
|   +-- Data/
|   +-- Helpers/
|   +-- Services/
+-- eTasks-server.Models/
|   +-- DTOs/
|   +-- Entities/
|   +-- Enums/
|   +-- Exceptions/
|   +-- Utils/
+-- eTasks-server.Tests/
```

Responsabilidades principais:

- **Web Host (`eTasks-server/eTasks-server`)**: bootstrap, DI, middlewares, endpoints, OpenAPI, Scalar, autenticacao, autorizacao e painel Blazor.
- **Client do painel (`eTasks-server/eTasks-server/Client`)**: paginas administrativas, layout, dialogs e servicos internos da UI.
- **Core (`eTasks-server.Core`)**: regras de negocio, EF Core, servicos internos, autenticacao, auditoria, logs, e-mail e integracoes.
- **Models (`eTasks-server.Models`)**: entidades, DTOs, constantes, enums, exceptions e contratos compartilhados.
- **Tests (`eTasks-server.Tests`)**: projeto de testes automatizados presente na solucao.

Fluxo de inicializacao:

```text
builder.RegisterServices()
builder.Build()
app.RegisterMiddlewares()
app.AddAPIEndpoints()
app.MapResourcesEndpoints()
app.Run()
```

---

## Roadmap

- [x] API versionada em `/api/v2`
- [x] Autenticacao JWT para clientes externos
- [x] Autenticacao por cookie para painel administrativo
- [x] Painel administrativo Blazor no mesmo host
- [x] CRUD e sincronizacao de tarefas, metas, notas, leituras, compras e financas
- [x] Logs em tempo real e retencao de logs
- [x] Auditoria operacional com MongoDB opcional
- [x] Configuracoes administrativas do servidor
- [x] Integracao com assistente de IA
- [ ] Ampliar cobertura de testes automatizados
- [ ] Documentar contratos dos principais endpoints com exemplos
- [ ] Melhorar automacao de deploy e observabilidade

---

## Contribuindo

Contribuicoes sao bem-vindas. Para colaborar:

1. Faca um fork do projeto.
2. Crie uma branch para sua alteracao (`git checkout -b feature/minha-feature`).
3. Faca commits objetivos (`git commit -m "feat: adiciona minha feature"`).
4. Envie a branch (`git push origin feature/minha-feature`).
5. Abra um pull request.

Antes de abrir o PR, compile a solucao e rode os testes disponiveis:

```bash
dotnet build eTasks-server.slnx
dotnet test eTasks-server.slnx
```

---

## Autor

<div align="center">

<img src="https://github.com/rafael-figueiredo-alves.png" width="100" style="border-radius: 50%"/>

**Rafael de Figueiredo Alves**

Desenvolvedor de Software apaixonado por Delphi, C#, .NET, Blazor, React e muito mais.

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/rafael-figueiredo-alves)

</div>

---

## Licenca

Este projeto esta sob licenca. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">

Feito por **Rafael de Figueiredo Alves**

[![eTasks Server](https://img.shields.io/badge/eTasks%20Server-ASP.NET%20Core-blue?style=for-the-badge)](https://github.com/rafael-figueiredo-alves/eTasks-server)

</div>
