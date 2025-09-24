# API de Controle de Estoque

Esta � uma API RESTful desenvolvida em ASP.NET para gerenciar um sistema de controle de estoque. Ela permite opera��es CRUD (Criar, Ler, Atualizar, Deletar) para recursos como Produtos, Clientes, Usu�rios e Pedidos, utilizando um banco de dados MongoDB.

## Pr�-requisitos

Para rodar este projeto localmente, voc� precisar� ter as seguintes ferramentas instaladas:
* [.NET SDK](https://dotnet.microsoft.com/download) (vers�o 8.0 ou superior).
* [Git](https://git-scm.com/downloads).
* Um editor de c�digo de sua prefer�ncia (como Visual Studio 2022 ou VS Code).

## Como Rodar o Projeto Localmente

Siga os passos abaixo para configurar e executar a aplica��o no seu ambiente de desenvolvimento.

**1. Clone o Reposit�rio:**
Abra um terminal e clone o projeto do GitHub para a sua m�quina.
```bash
git clone URL_DO_SEU_REPOSITORIO_AQUI
cd nome-da-pasta-do-projeto
```

**2. Configure suas Credenciais do Banco de Dados:**
A aplica��o precisa se conectar a um banco de dados MongoDB. As credenciais de conex�o s�o armazenadas localmente em um arquivo que **n�o** � enviado para o GitHub por seguran�a.

* Na raiz do projeto, crie um novo arquivo chamado `appsettings.Development.json`.
* Copie e cole o conte�do abaixo neste novo arquivo:

    ```json
    {
      "MongoDbSettings": {
        "ConnectionString": "SUA_CONNECTION_STRING_DO_MONGODB_AQUI",
        "DatabaseName": "NOME_DO_SEU_BANCO_DE_DADOS"
      }
    }
    ```
* ***Importante:*** *Substitua `SUA_CONNECTION_STRING_DO_MONGODB_AQUI` e `NOME_DO_SEU_BANCO_DE_DADOS` pelos valores corretos do seu banco.*

**3. Execute a Aplica��o:**
Ainda no seu terminal, na pasta raiz do projeto, execute o seguinte comando:
```bash
dotnet run
```
A API ser� iniciada e estar� ouvindo em um endere�o local, como `https://localhost:7202` e `http://localhost:5132`. O terminal mostrar� as URLs exatas.

**4. Teste os Endpoints:**
Com a aplica��o rodando, abra seu navegador e acesse a URL do Swagger para ver e testar todos os endpoints de forma interativa (lembre-se de usar a porta HTTPS correta):
`https://localhost:7202/swagger/index.html`

---

## Endpoints da API

A URL base para todos os endpoints � `https://localhost:[PORTA]/api`.

### Produtos (`/api/produtos`)
* `GET /api/produtos`: Retorna uma lista de todos os produtos.
* `GET /api/produtos/{id}`: Retorna um produto espec�fico pelo seu `id`.
* `POST /api/produtos`: Cria um novo produto. O corpo da requisi��o deve ser um JSON:
    ```json
    {
      "nome": "Notebook Gamer",
      "quantidade": 15,
      "tipo": "Eletr�nico",
      "preco": 7500.50
    }
    ```
* `PUT /api/produtos/{id}`: Atualiza um produto existente.
* `DELETE /api/produtos/{id}`: Deleta um produto espec�fico.

### Clientes (`/api/clientes`)
* `GET /api/clientes`: Retorna uma lista de todos os clientes.
* `GET /api/clientes/{id}`: Retorna um cliente espec�fico.
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
* `DELETE /api/clientes/{id}`: Deleta um cliente espec�fico.

### Usu�rios (`/api/users`)
* `GET /api/users`: Retorna uma lista de todos os usu�rios.
* `GET /api/users/{id}`: Retorna um usu�rio espec�fico.
* `POST /api/users`: Cria um novo usu�rio.
    ```json
    {
      "nome": "Administrador",
      "email": "admin@exemplo.com",
      "tipo": "Admin",
      "status": true
    }
    ```
* `PUT /api/users/{id}`: Atualiza um usu�rio existente.
* `DELETE /api/users/{id}`: Deleta um usu�rio espec�fico.

### Pedidos (`/api/pedidos`)
* `GET /api/pedidos`: Retorna uma lista de todos os pedidos.
* `GET /api/pedidos/{id}`: Retorna um pedido espec�fico.
* `POST /api/pedidos`: Cria um novo pedido.
    ```json
    {
      "nomeProduto": "Notebook Gamer",
      "quantidadeProduto": 2,
      "nomeCliente": "Empresa Exemplo LTDA"
    }
    ```
* `PUT /api/pedidos/{id}`: Atualiza um pedido existente.
* `DELETE /api/pedidos/{id}`: Deleta um pedido espec�fico.