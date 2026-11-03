# 🎮 FCG User API

API REST desenvolvida em **.NET 8** como parte do Tech Challenge da pós-graduação **Arquitetura de Sistemas .NET – FIAP**.

O projeto representa o microsserviço de **Usuários** da plataforma de games educacionais (**FCG – FIAP Cloud Games**). É o único emissor de tokens JWT da plataforma (Catalog e Notifications apenas validam) e atua como produtor de eventos de cadastro via mensageria.

---

## 📌 Objetivo do Projeto

Arquitetura de microsserviços orientada a eventos, garantindo:

- Autonomia de código e ciclo de vida (repositório isolado)
- Comunicação assíncrona utilizando Mensageria (**RabbitMQ** / **Amazon MQ** em produção)
- Persistência de dados isolada (PostgreSQL para usuários, DynamoDB para log de eventos)
- Cache com **Redis**
- Containerização com **Docker**
- Base escalável para orquestração em Kubernetes

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8** / **ASP.NET Core Web API**
- **Entity Framework Core** + **PostgreSQL** (Npgsql; RDS na AWS)
- **MassTransit** + **RabbitMQ** (Amazon MQ/AMQPS em produção)
- **Redis** (`IDistributedCache`, cache de consultas)
- **AWS DynamoDB** (log de eventos de domínio)
- **Prometheus** (`prometheus-net.AspNetCore`, métricas em `/metrics`)
- **BCrypt.Net-Next** (hash de senha)
- **JWT Bearer Authentication** (emissão e validação)
- **Docker**
- **Swagger / OpenAPI**
- **ILogger** para logs estruturados

---

## 🧱 Arquitetura

Clean Architecture / DDD, com quatro projetos sob `src/`:

- **`Fgc.Users.Api`** — Controllers, middlewares, `Security/` (geração de token JWT), `Program.cs`, `appsettings*.json`.
- **`Fgc.Users.Application`** — Serviços de negócio, `Interfaces`, `DTOS`, `Helpers`, publicação do evento `UserCreatedEvent` via MassTransit (pacote `Fgc.MessageContracts`).
- **`Fgc.Users.Domain`** — Entidade `User`, exceções de domínio.
- **`Fgc.Users.Infrastructure`** — `DbContext` (EF Core), `Repositories`, `Configuration/` (mapeamento EF), `Migrations/`, repositório de log de eventos em DynamoDB.

---

## 📁 Estrutura de Pastas

```text
Fgc.Users/
├── src/
│   ├── Fgc.Users.Api/
│   │   ├── Controllers/
│   │   ├── Security/
│   │   ├── Properties/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── Fgc.Users.Application/
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   ├── DTOS/
│   │   └── Helpers/
│   ├── Fgc.Users.Domain/
│   │   ├── Entities/
│   │   └── Exceptions/
│   └── Fgc.Users.Infrastructure/
│       ├── Persistence/
│       ├── Repositories/
│       ├── Configuration/
│       └── Migrations/
├── tests/
│   ├── Fgc.Users.Tests/                # xUnit + Moq, unitários
│   └── Fgc.Users.IntegrationTests/     # WebApplicationFactory
├── k8s/
├── LocalPackages/                      # .nupkg do Fgc.MessageContracts (ver abaixo)
├── nuget.config
├── Dockerfile
└── docker-compose.yml                  # dentro de Fgc.Users/, RabbitMQ standalone
```

---

## 🔐 Segurança e Autenticação com JWT

* Autenticação via **JWT Bearer**.
* Este microsserviço é o **único emissor** de tokens JWT da plataforma — Catalog e Notifications apenas validam.
* Controle de acesso utilizando `[Authorize]` e `[Authorize(Roles = "Admin")]`.
* Middleware global para tratamento de exceções.

```http
Authorization: Bearer {token}
```

---

## 📨 Mensageria e Eventos

* **Produtor apenas** (não há consumidores neste serviço). Publica `Fgc.MessageContracts.Events.UserCreatedEvent` (pacote NuGet compartilhado) tanto no autorregistro (`POST /auth/register`) quanto na criação de usuário por um admin.
* Cada evento publicado também é registrado em um log de auditoria no DynamoDB.

---

## 🔗 Endpoints Principais

### `AuthController` (`/auth`)
* `POST /auth/register` — cria usuário e publica `UserCreatedEvent`.
* `POST /auth/login` — autentica e retorna o token JWT.

### `UserController` (`/users`)
* `PUT /users/{id:guid}` — atualização do próprio usuário autenticado.

### `AdminUserController` (`/admin/users`, role Admin)
* `GET /admin/users` — lista usuários.
* `GET /admin/users/{id:guid}` — busca usuário por id.
* `PUT /admin/users/{id}/role` — atualiza role.
* `DELETE /admin/users/{id}` — remove usuário.

### `SetupController` (`/setup`)
* `POST /setup/first-admin` — bootstrap do primeiro usuário Admin, **só em Development**. Na AWS o primeiro Admin é criado pelo Job `--migrate` a partir de `Seed__AdminEmail`/`Seed__AdminPassword` (Secrets Manager).

---

## 📘 Documentação e Testes

Swagger disponível em `/swagger` (Authorize para autenticar via JWT).

Testes: `Fgc.Users.Tests` (unitários) e `Fgc.Users.IntegrationTests` (`WebApplicationFactory`).

---

## ⚠️ Tratamento de Erros e 🪵 Logs

* Middleware global de exceções: traduz exceções de domínio para HTTP (400/404/500) com resposta JSON padronizada.
* Logs estruturados via `ILogger`.
* Métricas Prometheus expostas em `/metrics`.

---

## 🗄️ Banco de Dados

* **PostgreSQL** via EF Core para a entidade `User`, com migrations em `Fgc.Users.Infrastructure/Migrations`.
  * No compose, a API aplica as migrations na subida (`Database__MigrateOnStartup`, padrão `true`).
  * No EKS, quem aplica é o Job `dotnet Fgc.Users.Api.dll --migrate`, que também cria o primeiro Admin e encerra.
* **AWS DynamoDB** para o log de eventos de domínio (`FgcEventLog`).
* **Redis** como cache de `GET /admin/users`. Com o Redis fora do ar, as consultas vão direto ao banco: um disjuntor ignora o cache por 30s depois de uma falha, sem adicionar latência a cada requisição.
* Health checks:
  * `/health/live`: o processo está vivo.
  * `/health/ready`: banco e broker. Falha de Redis ou DynamoDB só deixa o serviço *Degraded*.

---

## 📦 Pacote `Fgc.MessageContracts`

Referenciado via NuGet local (`nuget.config` aponta para `./LocalPackages`):

| Projeto | Versão |
|---|---|
| `Fgc.Users.Api` | 1.0.3 |
| `Fgc.Users.Application` | 1.0.1 |

`LocalPackages/` contém os `.nupkg` correspondentes a ambas as versões. O `Dockerfile` copia `LocalPackages/` e `nuget.config` antes do `dotnet restore`.

---

## ▶️ Como Executar o Projeto

### Pré-requisitos

* .NET SDK 8 (e SDK 10 para abrir o `.slnx` pela CLI)
* Docker e Docker Compose

### Variáveis de Ambiente

Sem valor padrão para segredos: se uma variável obrigatória faltar, a API não sobe e informa qual variável está faltando.
Localmente os valores vêm do `.env` de `fgc-orchestration` (via docker-compose). No EKS vêm do Secrets Manager, via External Secrets.
A tabela completa (local × AWS) está em `fgc-orchestration/README.md`.

* `ConnectionStrings__UserDb`: `Host=...;Port=5432;Database=fgc_users;Username=...;Password=...`
* `Jwt__Key` (obrigatória), `Jwt__Issuer`, `Jwt__Audience`
* `RabbitMq__Host`, `RabbitMq__Username`, `RabbitMq__Password` (obrigatórias), `RabbitMq__Port`, `RabbitMq__VirtualHost`, `RabbitMq__UseSsl` (`true` habilita AMQPS/5671, Amazon MQ)
* `Redis__ConnectionString` (obrigatória)
* `AWS__DynamoDB__ServiceUrl` (só local) e `AWS_REGION`
* `Seed__AdminEmail` / `Seed__AdminPassword` (só no modo `--migrate`)

### Execução via Docker

Suba a stack completa a partir de `fgc-orchestration/` (copie `.env.example` para `.env` antes):

```bash
cd ../fgc-orchestration
docker-compose up -d --build
```

### Execução Local

Com Postgres, RabbitMQ e Redis do compose rodando e as variáveis acima definidas:

```bash
dotnet run --project Fgc.Users/src/Fgc.Users.Api
```

Acesse: `http://localhost:5038/swagger`.

---

## 🚀 CI/CD

`.github/workflows/ci-cd.yml`:
- **Pull request:** build e testes.
- **Push em `main`:** testes, imagem com tag = SHA, Trivy (CRITICAL), push no ECR, Job de migração e rolling update no EKS.

Detalhes, credenciais e o ciclo da infra estão em `fgc-orchestration/README.md`.

---

## ☸️ Kubernetes

Manifestos kustomize em `k8s/`, aplicados nesta ordem por `fgc-orchestration/scripts/deploy-aws.sh`: `k8s/` (ServiceAccount com EKS Pod Identity, ConfigMap e ExternalSecret do Secrets Manager `fgc/users`), `k8s/migrate/` (Job `--migrate`) e `k8s/app/` (Deployment com 2 réplicas e rolling update sem indisponibilidade, Service ClusterIP e ServiceMonitor). A tag da imagem (`DEPLOY_TAG`) é trocada pelo SHA do commit no deploy.

---

## 👥 Squad 8 – Turma 12NETT

**Integrantes**

* Yan Santos Wendt
