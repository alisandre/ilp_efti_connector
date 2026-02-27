#!/usr/bin/env bash
set -euo pipefail

echo "▶ Avvio stack EFTI Connector..."
cd infra/docker

# Copia .env se non esiste
if [ ! -f .env ]; then
  echo "⚠  File .env non trovato — copio .env.example"
  cp .env.example .env
  echo "✏  Edita infra/docker/.env prima di continuare."
  exit 1
fi

docker compose --profile dev up -d --build

echo ""
echo "✅  Stack avviato. Servizi disponibili:"
echo "   MariaDB      → localhost:3306"
echo "   RabbitMQ     → http://localhost:15672  (guest/guest)"
echo "   Redis        → localhost:6379"
echo "   Keycloak     → http://localhost:8080"
echo "   Seq (logs)   → http://localhost:8888"
echo "   Grafana      → http://localhost:3001   (admin/admin)"
echo "   Prometheus   → http://localhost:9090"
echo "   MILOS Mock   → http://localhost:9999"
echo ""
echo "▶ Esegui le migrations:"
echo "   ./scripts/db-migrate.sh"
