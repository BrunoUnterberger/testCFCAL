FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PartageTexte.Api/PartageTexte.Api.csproj", "src/PartageTexte.Api/"]
COPY ["src/PartageTexte.Application/PartageTexte.Application.csproj", "src/PartageTexte.Application/"]
COPY ["src/PartageTexte.Domain/PartageTexte.Domain.csproj", "src/PartageTexte.Domain/"]
COPY ["src/PartageTexte.Infrastructure/PartageTexte.Infrastructure.csproj", "src/PartageTexte.Infrastructure/"]
RUN dotnet restore "src/PartageTexte.Api/PartageTexte.Api.csproj"
COPY . .
RUN dotnet publish "src/PartageTexte.Api/PartageTexte.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PartageTexte.Api.dll"]
