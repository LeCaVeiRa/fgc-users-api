# 🎮 FCG User API

API REST desenvolvida em **.NET 8** como parte do Tech Challenge da pós-graduação **Arquitetura de Sistemas .NET – FIAP**.

O projeto representa o microsserviço de **Usuários** da plataforma de games educacionais (**FCG – FIAP Cloud Games**). É o único emissor de tokens JWT da plataforma (Catalog e Notifications apenas validam) e atua como produtor de eventos de cadastro via mensageria.

---

## 📌 Objetivo do Projeto

Arquitetura de microsserviços orientada a eventos, garantindo:

- Autonomia de código e ciclo de vida (repositório isolado)
- Comunicação assíncrona utilizando Mensageria (**RabbitMQ** / **Amazon MQ** em produção)
- Persistência de dados isolada (SQLite para usuários, DynamoDB para log de eventos)
- Cache com **Redis**
- Containerização com **Docker**
- Base escalável para orquestração em Kubernetes

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8** / **ASP.NET Core Web API**
- **Entity Framework Core** + **SQLite**
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
* `POST /setup/first-admin` — bootstrap do primeiro usuário Admin do ambiente.

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

* **SQLite** via EF Core para a entidade `User` (migrations em `Fgc.Users.Infrastructure/Migrations`).
* **AWS DynamoDB** para o log de eventos de domínio (independente do SQLite).
* **Redis** como cache de leitura (`IDistributedCache`) em consultas de usuário.

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

* .NET SDK 8+
* Docker e Docker Compose (para RabbitMQ)

### Variáveis de Ambiente (`appsettings.json`)

* `ConnectionStrings:UserDb` (ex.: `Data Source=users.db`)
* `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
* `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:VirtualHost`, `RabbitMq:Username`, `RabbitMq:Password`, `RabbitMq:UseSsl` (`true` habilita AMQPS/porta 5671, usado com Amazon MQ em produção)

### Execução via Docker

A partir de `Fgc.Users/` (o `docker-compose.yml` não está na raiz do repositório):

```bash
cd Fgc.Users
docker-compose up -d
```

Para subir o serviço junto com os demais (Catalog, Payments, RabbitMQ, DynamoDB local, Redis, Kong, etc.), use o `docker-compose.yml` central em `fgc-orchestration/`.

### Execução Local

```bash
dotnet restore
dotnet ef database update --project src/Fgc.Users.Infrastructure --startup-project src/Fgc.Users.Api
dotnet run --project src/Fgc.Users.Api
```

Acesse: `http://localhost:5038/swagger` (porta local; em Docker via `fgc-orchestration` também é publicada em 5038).

---

## ☸️ Kubernetes

Manifestos em `k8s/` (`deployment.yaml`, `service.yaml`, `configmap.yaml`, `secret.yaml`). Aplicar com `kubectl apply -f k8s/`.

---

## 👥 Squad 8 – Turma 12NETT

**Integrantes**

* Yan Santos Wendt
