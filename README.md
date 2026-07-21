# 🎮 FCG User API – Fase 2

API REST desenvolvida em **.NET 8** como parte do **Desafio da Fase 2** da disciplina **Arquitetura de Sistemas .NET – FIAP**.

O projeto representa o microsserviço de **Usuários** da plataforma de games educacionais (**FCG – FIAP Game Center**). Nesta fase, o monolito foi decomposto, e este serviço passa a ter total autonomia sobre o gerenciamento de identidade, autenticação e atua como produtor de eventos de boas-vindas via mensageria.

---

## 📌 Objetivo do Projeto

Refatorar a aplicação para uma arquitetura de microsserviços orientada a eventos, garantindo:

- Autonomia de código e ciclo de vida (repositório isolado)
- Comunicação assíncrona utilizando Mensageria (**RabbitMQ**)
- Persistência de dados isolada (banco de dados próprio)
- Containerização com **Docker**
- Base escalável para orquestração em Kubernetes

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQLite**
- **MassTransit** (Abstração de Mensageria)
- **RabbitMQ** (Message Broker)
- **Docker**
- **JWT Bearer Authentication**
- **Swagger / OpenAPI**
- **ILogger** para logs estruturados

---

## 🧱 Arquitetura

O projeto segue uma separação clara de responsabilidades, inspirada em princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, promovendo manutenibilidade, clareza e escalabilidade.

### Camadas Principais

- **Api**  
  Controllers, middlewares, configuração da aplicação, geração de tokens JWT e autorização.

- **Application**  
  Serviços de negócio, interfaces, validações, DTOs e publicação de eventos via MassTransit.

- **Domain**  
  Entidades (ex: User), Value Objects e exceções de domínio.

- **Infrastructure**  
  Repositórios, acesso a dados e persistência utilizando Entity Framework Core.

---

## 📁 Estrutura de Pastas

```text
Fcg.User
│
├── ├── src │ ├── Fcg.User.Api
│   ├── Controllers
│   ├── Middlewares
│   ├── Program.cs
│   └── appsettings.json
│
├── Fcg.User.Application
│   ├── Services
│   ├── Interfaces
│   └── DTOs
│
├── Fcg.User.Domain
│   ├── Entities
│   └── Exceptions
│
└── Fcg.User.Infrastructure
│   ├── Persistence
│   └── Repositories
│
└── tests
```
---

## 🔐 Segurança e Autenticação com JWT

* Autenticação via **JWT Bearer**.
* Este microsserviço é o responsável por gerar os tokens JWT após o login com sucesso.
* Controle de acesso utilizando `[Authorize]` e `[Authorize(Roles = "Admin")]`.
* Middleware global para tratamento de exceções.

Após o login, o token JWT deve ser enviado nas requisições protegidas no header:

```http
Authorization: Bearer {token}

```

---

## 📨 Mensageria e Eventos (RabbitMQ)

A comunicação com outros microsserviços ocorre de forma assíncrona.

* **Produtor:** A UserAPI publica o evento de domínio `UserCreatedEvent` no RabbitMQ assim que um novo usuário é cadastrado com sucesso. Isso permite que outros serviços (como um futuro serviço de Notificações) enviem e-mails de boas-vindas de forma desacoplada.

---

## 🔗 Endpoints Principais

### 👤 Usuários e Autenticação

* Login (Gera Token): `POST /auth/login`
* Criar usuário: `POST /users`
* Listar usuários: `GET /users`
* Buscar usuário por ID: `GET /users/{id}`
* Atualizar role (Admin): `PUT /admin/users/{id}/role`
* Remover usuário (Admin): `DELETE /admin/users/{id}`

---

## 📘 Documentação e Testes

### Swagger / OpenAPI

A API possui documentação automática gerada com Swagger, disponível em:
`/swagger`

Por meio do Swagger é possível visualizar endpoints, ver parâmetros e testar requisições autenticando via botão **Authorize**.

---

## ⚠️ Tratamento de Erros e 🪵 Logs

* **Tratamento de Erros:** Middleware global de exceções, responsável por capturar exceções de domínio (ex: `NotFoundException`), traduzir para códigos HTTP adequados (400, 404, 500) e retornar JSON padronizado.
* **Logs:** Registrados com `ILogger` (`LogError`, `LogWarning`, `LogInformation`).

---

## 🗄️ Banco de Dados

* **SQLite**
* **Entity Framework Core**
* Arquivo do banco gerado automaticamente em ambiente de desenvolvimento via Migrations.

---

## ▶️ Como Executar o Projeto

### Pré-requisitos

* .NET SDK 8 ou superior
* Docker e Docker Compose (para o RabbitMQ)

### Variáveis de Ambiente (appsettings.json)

* `ConnectionStrings:DefaultConnection` (Data Source=users.db)
* `JwtSettings:SecretKey` (A chave que será usada para assinar os tokens)
* `RabbitMQ:Host`, `RabbitMQ:Username`, `RabbitMQ:Password`

### Execução via Docker

Para subir a infraestrutura (RabbitMQ) e a API:

```bash
docker-compose up -d

```

### Execução Local

Caso queira rodar apenas a API localmente (com o RabbitMQ já rodando no Docker):

```bash
dotnet restore
dotnet ef database update
dotnet run --project src/Fcg.User.Api

```

Acesse: `http://localhost:5000/swagger`

---

## ✅ Considerações Finais

Este projeto foi refatorado para atender aos requisitos da Fase 2, focando em:

* Autonomia de microsserviços
* Comunicação assíncrona (RabbitMQ / MassTransit)
* Containerização (Docker)
* Preparação para orquestração com Kubernetes

---

## 👥 Squad 8 – Turma 12NETT

**Integrantes**

* Yan Santos Wendt
* Ronnam de Lima da Silva

```

```
