# Estágio 1: Buildar a aplicação
# Usamos a imagem do SDK do .NET 8 para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo .csproj primeiro e restaura os pacotes NuGet
# Isso cria um "cache layer" - se os pacotes não mudarem, o Docker reutiliza essa etapa
COPY ControleEstoque.Api.csproj .
RUN dotnet restore "ControleEstoque.Api.csproj"

# Copia o resto do código-fonte e faz o build (publish)
COPY . .
RUN dotnet publish "ControleEstoque.Api.csproj" -c Release -o /app/publish

# Estágio 2: Criar a imagem final de runtime
# Usamos a imagem menor 'aspnet', que só tem o necessário para rodar a API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define o ponto de entrada para rodar a DLL da API
# O Render espera que a aplicação escute na porta 10000 (o que faremos via variável de ambiente)
# A imagem base aspnet:8.0 escuta na porta 8080 por padrão, o que o Render também aceita.
# Vamos manter o entrypoint padrão da imagem base que é mais simples.
# A imagem base já sabe como rodar a DLL.

# Expõe a porta 8080 (padrão da imagem aspnet 8.0)
EXPOSE 8080