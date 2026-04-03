FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/core/PrintFlow.Domain/PrintFlow.Domain.csproj src/core/PrintFlow.Domain/
COPY src/core/PrintFlow.Application/PrintFlow.Application.csproj src/core/PrintFlow.Application/
COPY src/infrastructure/PrintFlow.Persistence/PrintFlow.Persistence.csproj src/infrastructure/PrintFlow.Persistence/
COPY src/infrastructure/PrintFlow.Infrastructure/PrintFlow.Infrastructure.csproj src/infrastructure/PrintFlow.Infrastructure/
COPY src/presentation/PrintFlow.API/PrintFlow.API.csproj src/presentation/PrintFlow.API/
COPY Worker/PrintFlow.Worker/PrintFlow.Worker.csproj Worker/PrintFlow.Worker/
COPY Tests/PrintFlow.UnitTests/PrintFlow.UnitTests.csproj Tests/PrintFlow.UnitTests/

RUN dotnet restore src/presentation/PrintFlow.API/PrintFlow.API.csproj

COPY . .
RUN dotnet publish src/presentation/PrintFlow.API/PrintFlow.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PrintFlow.API.dll"]