# Usa a imagem oficial do SDK do .NET 10 para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o arquivo do projeto e baixa as dependências
COPY ["MatrizApi.csproj", "./"]
RUN dotnet restore "./MatrizApi.csproj"

# Copia o resto do código e publica a versão final otimizada
COPY . .
RUN dotnet publish "MatrizApi.csproj" -c Release -o /app/publish

# Usa a imagem mais leve do .NET 10 apenas para rodar o aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Diz para o servidor usar a porta padrão do Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Inicia a aplicação
ENTRYPOINT ["dotnet", "MatrizApi.dll"]