FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PartageTexte.Web/PartageTexte.Web.csproj", "src/PartageTexte.Web/"]
COPY ["src/PartageTexte.Application/PartageTexte.Application.csproj", "src/PartageTexte.Application/"]
COPY ["src/PartageTexte.Domain/PartageTexte.Domain.csproj", "src/PartageTexte.Domain/"]
RUN dotnet restore "src/PartageTexte.Web/PartageTexte.Web.csproj"
COPY . .
RUN dotnet publish "src/PartageTexte.Web/PartageTexte.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PartageTexte.Web.dll"]
