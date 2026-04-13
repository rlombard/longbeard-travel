# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AI.Forged.TourOps.sln ./
COPY src/AI.Forged.TourOps.Api/AI.Forged.TourOps.Api.csproj src/AI.Forged.TourOps.Api/
COPY src/AI.Forged.TourOps.Application/AI.Forged.TourOps.Application.csproj src/AI.Forged.TourOps.Application/
COPY src/AI.Forged.TourOps.Domain/AI.Forged.TourOps.Domain.csproj src/AI.Forged.TourOps.Domain/
COPY src/AI.Forged.TourOps.Infrastructure/AI.Forged.TourOps.Infrastructure.csproj src/AI.Forged.TourOps.Infrastructure/
RUN dotnet restore src/AI.Forged.TourOps.Api/AI.Forged.TourOps.Api.csproj

COPY src ./src
RUN dotnet publish src/AI.Forged.TourOps.Api/AI.Forged.TourOps.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AI.Forged.TourOps.Api.dll"]
