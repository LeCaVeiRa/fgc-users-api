# === STAGE 1: BUILD ===
FROM mcr.microsoft.com/dotnet/sdk:8.0.427 AS build
WORKDIR /src

# Pacotes locais (Fgc.MessageContracts)
COPY LocalPackages ./LocalPackages
COPY nuget.config .

# Copia apenas os .csproj pra cache do restore
COPY ["Fgc.Users/src/Fgc.Users.Api/Fgc.Users.Api.csproj", "Fgc.Users/src/Fgc.Users.Api/"]
COPY ["Fgc.Users/src/Fgc.Users.Application/Fgc.Users.Application.csproj", "Fgc.Users/src/Fgc.Users.Application/"]
COPY ["Fgc.Users/src/Fgc.Users.Domain/Fgc.Users.Domain.csproj", "Fgc.Users/src/Fgc.Users.Domain/"]
COPY ["Fgc.Users/src/Fgc.Users.Infrastructure/Fgc.Users.Infrastructure.csproj", "Fgc.Users/src/Fgc.Users.Infrastructure/"]
RUN dotnet restore "Fgc.Users/src/Fgc.Users.Api/Fgc.Users.Api.csproj"

# Copia o resto e faz o publish
COPY . .
WORKDIR "/src/Fgc.Users/src/Fgc.Users.Api"
RUN dotnet publish "Fgc.Users.Api.csproj" -c Release -o /app/publish

# === STAGE 2: RUNTIME ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0.33 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
# Usuário não-root embutido na imagem aspnet (UID 1654); porta 8080 não exige privilégio.
USER $APP_UID
ENTRYPOINT ["dotnet", "Fgc.Users.Api.dll"]