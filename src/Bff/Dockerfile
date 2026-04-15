# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AI.Forged.TourOps.sln ./
COPY src/AI.Forged.TourOps.Bff/AI.Forged.TourOps.Bff.csproj src/AI.Forged.TourOps.Bff/
RUN dotnet restore src/AI.Forged.TourOps.Bff/AI.Forged.TourOps.Bff.csproj

COPY src ./src
RUN dotnet publish src/AI.Forged.TourOps.Bff/AI.Forged.TourOps.Bff.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AI.Forged.TourOps.Bff.dll"]
