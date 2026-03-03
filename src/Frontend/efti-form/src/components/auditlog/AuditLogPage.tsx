import { useState } from 'react'
import { useAuditLogs } from '../../hooks/useAuditLogs'
import { AuditLogFilters } from './AuditLogFilters'
import { AuditLogTable } from './AuditLogTable'
import type { AuditLogFilters as Filters } from '../../types/auditLog'

const PAGE_SIZE = 20

export function AuditLogPage() {
  const [filters, setFilters] = useState<Filters>({ page: 1, pageSize: PAGE_SIZE })

  const { data, isLoading, isError } = useAuditLogs(filters)

  const handleSearch = (partial: Omit<Filters, 'page' | 'pageSize'>) =>
    setFilters({ ...partial, page: 1, pageSize: PAGE_SIZE })

  const goToPage = (page: number) =>
    setFilters((prev) => ({ ...prev, page }))

  const totalPages = data?.totalPages ?? 1
  const page       = filters.page

  return (
    <div className="space-y-4">
      <AuditLogFilters onSearch={handleSearch} />

      {isError && (
        <div className="rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          Errore nel caricamento dei dati. Verifica che il QueryProxyService sia in esecuzione.
        </div>
      )}

      {/* Contatore risultati */}
      {data && !isLoading && (
        <p className="text-xs text-gray-500">
          {data.totalCount} record trovati
          {data.totalPages > 1 && ` — pagina ${page} di ${totalPages}`}
        </p>
      )}

      <AuditLogTable entries={data?.items ?? []} isLoading={isLoading} />

      {/* Paginazione */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <button
            disabled={page <= 1}
            onClick={() => goToPage(page - 1)}
            className="rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
          >
            ← Precedente
          </button>

          <div className="flex gap-1">
            {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
              const p = totalPages <= 7
                ? i + 1
                : page <= 4
                  ? i + 1
                  : page >= totalPages - 3
                    ? totalPages - 6 + i
                    : page - 3 + i
              return (
                <button
                  key={p}
                  onClick={() => goToPage(p)}
                  className={`min-w-[2rem] rounded border px-2 py-1 text-sm ${
                    p === page
                      ? 'border-blue-600 bg-blue-600 text-white'
                      : 'border-gray-300 text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  {p}
                </button>
              )
            })}
          </div>

          <button
            disabled={page >= totalPages}
            onClick={() => goToPage(page + 1)}
            className="rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
          >
            Successiva →
          </button>
        </div>
      )}
    </div>
  )
}
