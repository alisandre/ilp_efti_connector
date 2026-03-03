import { useState } from 'react'
import { useAuditLogDetail } from '../../hooks/useAuditLogs'
import type { AuditLogEntry } from '../../types/auditLog'

interface Props {
  entries: AuditLogEntry[]
  isLoading: boolean
}

const ACTION_BADGE: Record<string, string> = {
  Create:  'bg-green-100 text-green-700',
  Update:  'bg-yellow-100 text-yellow-700',
  Delete:  'bg-red-100 text-red-700',
  Send:    'bg-blue-100 text-blue-700',
  Receive: 'bg-purple-100 text-purple-700',
  Read:    'bg-gray-100 text-gray-600',
  Query:   'bg-gray-100 text-gray-600',
  Export:  'bg-indigo-100 text-indigo-700',
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('it-IT', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  })
}

function shortId(id: string | null) {
  if (!id) return '—'
  return id.slice(0, 8) + '…'
}

function JsonBlock({ label, value }: { label: string; value: string | null }) {
  if (!value) return null
  let pretty = value
  try { pretty = JSON.stringify(JSON.parse(value), null, 2) } catch { /* keep raw */ }
  return (
    <div>
      <p className="mb-1 text-xs font-semibold text-gray-500">{label}</p>
      <pre className="max-h-48 overflow-auto rounded bg-gray-50 p-2 text-xs text-gray-700">
        {pretty}
      </pre>
    </div>
  )
}

function DetailRow({ id }: { id: string }) {
  const { data, isLoading } = useAuditLogDetail(id)

  if (isLoading)
    return <p className="p-4 text-sm text-gray-400">Caricamento dettaglio…</p>

  if (!data)
    return <p className="p-4 text-sm text-red-500">Dettaglio non disponibile.</p>

  return (
    <div className="grid grid-cols-1 gap-4 px-4 py-3 sm:grid-cols-2">
      <div className="space-y-2 text-sm">
        <p><span className="font-medium text-gray-500">Entity ID completo: </span>
          <span className="font-mono text-xs">{data.entityId}</span></p>
        {data.performedByUserId && (
          <p><span className="font-medium text-gray-500">User ID: </span>
            <span className="font-mono text-xs">{data.performedByUserId}</span></p>
        )}
        {data.performedBySourceId && (
          <p><span className="font-medium text-gray-500">Source ID: </span>
            <span className="font-mono text-xs">{data.performedBySourceId}</span></p>
        )}
        {data.ipAddress && (
          <p><span className="font-medium text-gray-500">IP: </span>{data.ipAddress}</p>
        )}
        {data.userAgent && (
          <p className="break-all"><span className="font-medium text-gray-500">User Agent: </span>
            <span className="text-xs">{data.userAgent}</span></p>
        )}
      </div>
      <div className="space-y-3">
        <JsonBlock label="Valore precedente" value={data.oldValueJson} />
        <JsonBlock label="Valore aggiornato"  value={data.newValueJson} />
      </div>
    </div>
  )
}

export function AuditLogTable({ entries, isLoading }: Props) {
  const [expandedId, setExpandedId] = useState<string | null>(null)

  const toggle = (id: string) =>
    setExpandedId((prev) => (prev === id ? null : id))

  if (isLoading)
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-8 text-center text-sm text-gray-400 shadow-sm">
        Caricamento…
      </div>
    )

  if (entries.length === 0)
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-8 text-center text-sm text-gray-400 shadow-sm">
        Nessun risultato per i filtri selezionati.
      </div>
    )

  return (
    <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
      <table className="w-full text-sm">
        <thead className="border-b border-gray-200 bg-gray-50">
          <tr>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Data / Ora
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Entità
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Entity ID
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Azione
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Descrizione
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
              Utente / Sorgente
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {entries.map((entry) => (
            <>
              <tr
                key={entry.id}
                onClick={() => toggle(entry.id)}
                className="cursor-pointer hover:bg-gray-50"
              >
                <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-gray-600">
                  {formatDate(entry.createdAt)}
                </td>
                <td className="px-4 py-3 text-gray-700">{entry.entityType}</td>
                <td className="px-4 py-3 font-mono text-xs text-gray-500">
                  {shortId(entry.entityId)}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-block rounded px-2 py-0.5 text-xs font-medium ${ACTION_BADGE[entry.actionType] ?? 'bg-gray-100 text-gray-600'}`}
                  >
                    {entry.actionType}
                  </span>
                </td>
                <td className="max-w-xs truncate px-4 py-3 text-gray-700">
                  {entry.description}
                </td>
                <td className="px-4 py-3 font-mono text-xs text-gray-500">
                  {shortId(entry.performedByUserId ?? entry.performedBySourceId)}
                </td>
              </tr>

              {expandedId === entry.id && (
                <tr key={`${entry.id}-detail`} className="bg-blue-50">
                  <td colSpan={6} className="border-t border-blue-100">
                    <DetailRow id={entry.id} />
                  </td>
                </tr>
              )}
            </>
          ))}
        </tbody>
      </table>
    </div>
  )
}
