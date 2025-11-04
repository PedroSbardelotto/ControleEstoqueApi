# API de Controle de Estoque

Esta é uma API RESTful desenvolvida em ASP.NET para gerenciar um sistema de controle de estoque. A API utiliza autenticação via JSON Web Tokens (JWT) para proteger seus endpoints.

Ela permite operações CRUD (Criar, Ler, Atualizar, Deletar) para recursos como Produtos, Clientes, Usuários e Pedidos, utilizando um banco de dados **SQL Server** e **Entity Framework Core (EF Core)**.

## Pré-requisitos

Para rodar este projeto localmente, você precisará ter as seguintes ferramentas instaladas:
* [.NET SDK](https://dotnet.microsoft.com/download) (versão 8.0 ou superior).
* [Git](https://git-scm.com/downloads).
* **SQL Server Express LocalDB** (geralmente instalado com o Visual Studio) ou outra instância do SQL Server.
* Um editor de código de sua preferência (como Visual Studio 2022 ou VS Code).

## Como Rodar o Projeto Localmente

Siga os passos abaixo para configurar e executar a aplicação no seu ambiente de desenvolvimento.

**1. Clone o Repositório:**
Abra um terminal e clone o projeto do GitHub para a sua máquina.
```bash
git clone URL_DO_SEU_REPOSITORIO_AQUI
cd nome-da-pasta-do-projeto
```

**2. Configure suas Credenciais:**
A aplicação precisa se conectar ao SQL Server e usar uma chave secreta para JWT. As credenciais são armazenadas localmente em um arquivo que **não** é enviado para o GitHub por segurança.

* Na raiz do projeto, crie um novo arquivo chamado `appsettings.Development.json`.
* Copie e cole o conteúdo abaixo neste novo arquivo:

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ControleEstoqueDB;Trusted_Connection=True;TrustServerCertificate=True"
      },
      "JwtSettings": {
        "SecretKey": "SUA_CHAVE_SECRETA_ALEATORIA_LONGA_AQUI"
      }
    }
    ```
* **Importante:** A `ConnectionStrings.DefaultConnection` acima funciona para a maioria das instalações do Visual Studio. Se seu SQL Server local tiver outro nome (ex: `.\SQLEXPRESS`), ajuste-o. A `JwtSettings.SecretKey` deve ser uma string aleatória longa e segura.

**3. Crie o Banco de Dados (Migrations):**
Antes de rodar a API pela primeira vez, você precisa criar o banco de dados e suas tabelas. No terminal, na pasta raiz do projeto, execute o comando:
```bash
dotnet ef database update
```

**4. Execute a Aplicação:**
Agora que o banco está criado, execute a aplicação:
```bash
dotnet run
```
A API será iniciada e estará ouvindo em um endereço local, como `https://localhost:7202`.

**5. Acesse a Documentação Interativa:**
Com a aplicação rodando, abra seu navegador e acesse a URL do Swagger para ver e testar todos os endpoints:
`https://localhost:7202/swagger/index.html` (lembre-se de usar a porta HTTPS correta).

---

## Autenticação (JWT) - Como Testar

Todos os endpoints, exceto `/api/auth/login`, são protegidos. Além disso, endpoints críticos como `POST /api/users` (criar novo usuário) só podem ser acessados por usuários com o papel de **"Admin"**.

**Passo 1: Obtenha Credenciais de Admin**
* Para testar, você precisa ter um usuário Admin no banco.
* **Se este é o primeiro login:** Você precisará **comentar temporariamente** o atributo `[Authorize(Roles = "Admin")]` (e qualquer `[Authorize]` na classe) no `UsersController.cs`, rodar a API, usar o `POST /api/users` (público) para criar seu usuário com `"tipo": "Admin"`, e depois parar a API e **colocar o atributo de volta** para proteger o endpoint.

**Passo 2: Obtenha um Token de Acesso**
* Vá até o endpoint `POST /api/auth/login`.
* No corpo da requisição, insira o email e a senha do seu usuário Admin:
    ```json
    {
      "email": "seu_email@admin.com",
      "senha": "sua_senha"
    }
    ```
* Clique em "Execute". Copie o `token` JWT da resposta.

**Passo 3: Autorize suas Requisições no Swagger**
* No topo direito da página do Swagger, clique no botão **"Authorize"**.
* Na janela, digite `Bearer ` (a palavra "Bearer", um espaço) e **cole o seu token**.
* Clique em "Authorize" e "Close".

**Passo 4: Teste os Endpoints Protegidos**
* O cadeado "Authorize" estará fechado. Agora você pode testar todos os endpoints, incluindo a criação de outros usuários e produtos.

---

## Endpoints da API

⚠️ **Importante:** Todos os endpoints abaixo, exceto `/api/auth/login`, são protegidos e requerem um token JWT.

### Autenticação (`/api/auth`)
* `POST /api/auth/login`: (Público) Realiza o login de um usuário e retorna um token JWT.

### Relatórios (`/api/relatorios`)
* `GET /api/relatorios/estoque/visaogeral`: Retorna um resumo do estoque, incluindo o `totalItensUnicos` e o `valorTotalEstoque` (Valor * Quantidade).

### Produtos (`/api/produtos`)
* `GET /api/produtos`: Retorna uma lista de todos os produtos.
* `GET /api/produtos/{id}`: Retorna um produto específico pelo seu `id` (ex: `/api/produtos/1`).
* `POST /api/produtos`: Cria um novo produto. O JSON **não deve** conter `id` ou `pedidos`.
    ```json
    {
      "nome": "Caixa de Papelão",
      "quantidade": 1500,
      "tipo": "Embalagem",
      "preco": 3.25
    }
    ```
* `PUT /api/produtos/{id}`: Atualiza um produto existente.
* `DELETE /api/produtos/{id}`: Deleta um produto específico.

### Clientes (`/api/clientes`)
* `GET /api/clientes`: Retorna uma lista de todos os clientes.
* `GET /api/clientes/{id}`: Retorna um cliente específico (ex: `/api/clientes/1`).
* `POST /api/clientes`: Cria um novo cliente.
* `PUT /api/clientes/{id}`: Atualiza um cliente existente.
* `DELETE /api/clientes/{id}`: Deleta um cliente específico.

### Usuários (`/api/users`)
* `GET /api/users`: Retorna uma lista de todos os usuários.
* `GET /api/users/{id}`: Retorna um usuário específico (ex: `/api/users/1`).
* `POST /api/users`: **(Requer Role "Admin")** Cria um novo usuário (ex: "Funcionário").
* `PUT /api/users/{id}`: Atualiza um usuário existente.
* `DELETE /api/users/{id}`: **(Requer Role "Admin")** Deleta um usuário específico.

### Pedidos (`/api/pedidos`)
* `GET /api/pedidos`: Retorna uma lista de todos os pedidos.
* `GET /api/pedidos/{id}`: Retorna um pedido específico (ex: `/api/pedidos/1`), incluindo os dados do Cliente e Produto relacionados.
* `POST /api/pedidos`: Cria um novo pedido usando os IDs do Cliente e Produto. Este endpoint **verifica o estoque** e **deduz a quantidade** do produto.
    ```json
    {
      "produtoId": 1,
      "clienteId": 2,
      "quantidadeProduto": 10
    }
    ```
* `PUT /api/pedidos/{id}`: Atualiza um pedido existente.
* `DELETE /api/pedidos/{id}`: Deleta um pedido específico.