# Usar imagem .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar o cssproj e as dependências
COPY ["PraOndeFoi.csproj", "."]
RUN dotnet restore "PraOndeFoi.csproj"

# Copiar o restante e buildar
COPY . .
RUN dotnet build "PraOndeFoi.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "PraOndeFoi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Usar a imagem do .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar dependências do sistema para PostgreSQL
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Expor na porta 8080
EXPOSE 8080

# Definir ambiente de produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Rodar
ENTRYPOINT ["dotnet", "PraOndeFoi.dll"]