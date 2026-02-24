# ─── Runtime base ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_HTTP_PORTS=8080

# ─── Build ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG SERVICE_PROJECT
WORKDIR /src

# Copia l'intera solution (necessario per le dipendenze cross-project)
COPY . .

RUN dotnet publish "${SERVICE_PROJECT}" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ─── Final ────────────────────────────────────────────────────────────────────
FROM base AS final
ARG SERVICE_ASSEMBLY
ENV SERVICE_ASSEMBLY=${SERVICE_ASSEMBLY}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "exec dotnet ${SERVICE_ASSEMBLY}.dll"]
