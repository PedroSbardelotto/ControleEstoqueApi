# API de Controle de Estoque

Esta é uma API RESTful desenvolvida em ASP.NET para gerenciar um sistema de controle de estoque. A API utiliza autenticação via JSON Web Tokens (JWT) para proteger seus endpoints.

Ela permite operações CRUD (Criar, Ler, Atualizar, Deletar) para recursos como Produtos, Clientes, Usuários e Pedidos, utilizando um banco de dados MongoDB.

## Pré-requisitos

Para rodar este projeto localmente, você precisará ter as seguintes ferramentas instaladas:
* [.NET SDK](https://dotnet.microsoft.com/download) (versão 8.0 ou superior).
* [Git](https://git-scm.com/downloads).
* Um editor de código de sua preferência (como Visual Studio 2022 ou VS Code).

## Como Rodar o Projeto Localmente

Siga os passos abaixo para configurar e executar a aplicação no seu ambiente de desenvolvimento.

**1. Clone o Repositório:**
Abra um terminal e clone o projeto do GitHub para a sua máquina.
```bash
git clone URL_DO_SEU_REPOSITORIO_AQUI
cd nome-da-pasta-do-projeto
```

**2. Configure suas Credenciais do Banco de Dados:**
A aplicação precisa se conectar a um banco de dados MongoDB. As credenciais são armazenadas localmente em um arquivo que **não** é enviado para o GitHub por segurança.

* Na raiz do projeto, crie um novo arquivo chamado `appsettings.Development.json`.
* Copie e cole o conteúdo abaixo neste novo arquivo:

    ```json
    {
      "MongoDbSettings": {
        "ConnectionString": "SUA_CONNECTION_STRING_DO_MONGODB_AQUI",
        "DatabaseName": "NOME_DO_SEU_BANCO_DE_DADOS"
      },
      "JwtSettings": {
        "SecretKey": "SUA_CHAVE_SECRETA_SUPER_SECRETA_AQUI"
      }
    }
    ```
* **Importante:** Substitua os valores pelos dados corretos do seu banco de dados e defina uma chave secreta para o JWT.

**3. Execute a Aplicação:**
Ainda no seu terminal, na pasta raiz do projeto, execute o seguinte comando:
```bash
dotnet run
```
A API será iniciada e estará ouvindo em um endereço local, como `https://localhost:7202`.

**4. Acesse a Documentação Interativa:**
Com a aplicação rodando, abra seu navegador e acesse a URL do Swagger para ver e testar todos os endpoints:
`https://localhost:7202/swagger/index.html` (lembre-se de usar a porta HTTPS correta).

---

## Autenticação (JWT) - Como Testar

Todos os endpoints, exceto o de login, são protegidos. Para testá-los, você precisa primeiro se autenticar e usar o token recebido.

**Passo 1: Crie um Usuário**
* Use o endpoint `POST /api/users` para criar um usuário de teste. Lembre-se do email e senha que você cadastrou.

**Passo 2: Obtenha um Token de Acesso**
* Vá até o endpoint `POST /api/auth/login`.
* Clique em "Try it out".
* No corpo da requisição, insira o email e a senha do usuário que você criou:
    ```json
    {
      "email": "seu_email@cadastrado.com",
      "senha": "sua_senha"
    }
    ```
* Clique em "Execute". A resposta conterá seu token JWT. Copie a longa string do token.

**Passo 3: Autorize suas Requisições no Swagger**
* No topo direito da página do Swagger, clique no botão **"Authorize"**.
* Na janela que abrir, no campo "Value", digite `Bearer ` (a palavra "Bearer", um espaço) e **cole o token** que você copiou.
* Clique em "Authorize" e depois em "Close".

**Passo 4: Teste os Endpoints Protegidos**
* Agora, com o cadeado no botão "Authorize" aparecendo como "fechado", todas as suas requisições subsequentes incluirão o token de autenticação. Você pode testar qualquer endpoint (`GET /api/produtos`, `POST /api/clientes`, etc.) e eles funcionarão.

---

## Endpoints da API

⚠️ **Importante:** Todos os endpoints abaixo, exceto `/api/auth/login`, são protegidos e requerem um token JWT. Siga as instruções na seção "Autenticação" para poder testá-los.

### Autenticação (`/api/auth`)
* `POST /api/auth/login`: (Público) Realiza o login de um usuário e retorna um token JWT.

### Produtos (`/api/produtos`)
* `GET /api/produtos`: Retorna uma lista de todos os produtos.
* `GET /api/produtos/{id}`: Retorna um produto específico pelo seu `id`.
* `POST /api/produtos`: Cria um novo produto.
* `PUT /api/produtos/{id}`: Atualiza um produto existente.
* `DELETE /api/produtos/{id}`: Deleta um produto específico.

### Clientes (`/api/clientes`)
* `GET /api/clientes`: Retorna uma lista de todos os clientes.
* `GET /api/clientes/{id}`: Retorna um cliente específico.
* `POST /api/clientes`: Cria um novo cliente.
* `PUT /api/clientes/{id}`: Atualiza um cliente existente.
* `DELETE /api/clientes/{id}`: Deleta um cliente específico.

### Usuários (`/api/users`)
* `GET /api/users`: Retorna uma lista de todos os usuários.
* `GET /api/users/{id}`: Retorna um usuário específico.
* `POST /api/users`: Cria um novo usuário (geralmente, este endpoint também deveria ser protegido e acessível apenas por administradores).
* `PUT /api/users/{id}`: Atualiza um usuário existente.
* `DELETE /api/users/{id}`: Deleta um usuário específico.

### Pedidos (`/api/pedidos`)
* `GET /api/pedidos`: Retorna uma lista de todos os pedidos.
* `GET /api/pedidos/{id}`: Retorna um pedido específico.
* `POST /api/pedidos`: Cria um novo pedido.
* `PUT /api/pedidos/{id}`: Atualiza um pedido existente.
* `DELETE /api/pedidos/{id}`: Deleta um pedido específico.