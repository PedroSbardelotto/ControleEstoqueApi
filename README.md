# API de Controle de Estoque

Esta é uma API RESTful desenvolvida em ASP.NET para gerenciar um sistema de controle de estoque. Ela permite operações CRUD (Criar, Ler, Atualizar, Deletar) para recursos como Produtos, Clientes, Usuários e Pedidos, utilizando um banco de dados MongoDB.

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
A aplicação precisa se conectar a um banco de dados MongoDB. As credenciais de conexão são armazenadas localmente em um arquivo que **não** é enviado para o GitHub por segurança.

* Na raiz do projeto, crie um novo arquivo chamado `appsettings.Development.json`.
* Copie e cole o conteúdo abaixo neste novo arquivo:

    ```json
    {
      "MongoDbSettings": {
        "ConnectionString": "SUA_CONNECTION_STRING_DO_MONGODB_AQUI",
        "DatabaseName": "NOME_DO_SEU_BANCO_DE_DADOS"
      }
    }
    ```
* ***Importante:*** *Substitua `SUA_CONNECTION_STRING_DO_MONGODB_AQUI` e `NOME_DO_SEU_BANCO_DE_DADOS` pelos valores corretos do seu banco.*

**3. Execute a Aplicação:**
Ainda no seu terminal, na pasta raiz do projeto, execute o seguinte comando:
```bash
dotnet run
```
A API será iniciada e estará ouvindo em um endereço local, como `https://localhost:7202` e `http://localhost:5132`. O terminal mostrará as URLs exatas.

**4. Teste os Endpoints:**
Com a aplicação rodando, abra seu navegador e acesse a URL do Swagger para ver e testar todos os endpoints de forma interativa (lembre-se de usar a porta HTTPS correta):
`https://localhost:7202/swagger/index.html`

---

## Endpoints da API

A URL base para todos os endpoints é `https://localhost:[PORTA]/api`.

### Produtos (`/api/produtos`)
* `GET /api/produtos`: Retorna uma lista de todos os produtos.
* `GET /api/produtos/{id}`: Retorna um produto específico pelo seu `id`.
* `POST /api/produtos`: Cria um novo produto. O corpo da requisição deve ser um JSON:
    ```json
    {
      "nome": "Notebook Gamer",
      "quantidade": 15,
      "tipo": "Eletrônico",
      "preco": 7500.50
    }
    ```
* `PUT /api/produtos/{id}`: Atualiza um produto existente.
* `DELETE /api/produtos/{id}`: Deleta um produto específico.

### Clientes (`/api/clientes`)
* `GET /api/clientes`: Retorna uma lista de todos os clientes.
* `GET /api/clientes/{id}`: Retorna um cliente específico.
* `POST /api/clientes`: Cria um novo cliente.
    ```json
    {
      "nome": "Empresa Exemplo LTDA",
      "cnpj": "12345678000199",
      "email": "contato@exemplo.com",
      "endereco": "Rua dos Exemplos, 123"
    }
    ```
* `PUT /api/clientes/{id}`: Atualiza um cliente existente.
* `DELETE /api/clientes/{id}`: Deleta um cliente específico.

### Usuários (`/api/users`)
* `GET /api/users`: Retorna uma lista de todos os usuários.
* `GET /api/users/{id}`: Retorna um usuário específico.
* `POST /api/users`: Cria um novo usuário.
    ```json
    {
      "nome": "Administrador",
      "email": "admin@exemplo.com",
      "tipo": "Admin",
      "status": true
    }
    ```
* `PUT /api/users/{id}`: Atualiza um usuário existente.
* `DELETE /api/users/{id}`: Deleta um usuário específico.

### Pedidos (`/api/pedidos`)
* `GET /api/pedidos`: Retorna uma lista de todos os pedidos.
* `GET /api/pedidos/{id}`: Retorna um pedido específico.
* `POST /api/pedidos`: Cria um novo pedido.
    ```json
    {
      "nomeProduto": "Notebook Gamer",
      "quantidadeProduto": 2,
      "nomeCliente": "Empresa Exemplo LTDA"
    }
    ```
* `PUT /api/pedidos/{id}`: Atualiza um pedido existente.
* `DELETE /api/pedidos/{id}`: Deleta um pedido específico.