import { useOperationStatus } from '../../hooks/useOperationStatus'

interface Props {
  operationId: string
}

const STATUS_COLORS: Record<string, string> = {
  PENDING_VALIDATION: 'bg-yellow-100 text-yellow-800',
  VALIDATED: 'bg-blue-100 text-blue-800',
  SENT: 'bg-indigo-100 text-indigo-800',
  ACKNOWLEDGED: 'bg-green-100 text-green-800',
  FAILED: 'bg-red-100 text-red-800',
  REJECTED: 'bg-orange-100 text-orange-800',
}

function Row({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null
  return (
    <div className="flex justify-between py-1.5 text-sm">
      <span className="text-gray-500">{label}</span>
      <span className="font-medium text-gray-800">{value}</span>
    </div>
  )
}

export function OperationStatusCard({ operationId }: Props) {
  const { data, isLoading, isError } = useOperationStatus(operationId)

  if (isLoading)
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 text-center text-sm text-gray-500">
        Recupero stato in corso…
      </div>
    )

  if (isError || !data)
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
        Impossibile recuperare lo stato dell&apos;operazione.
      </div>
    )

  const colorClass = STATUS_COLORS[data.status] ?? 'bg-gray-100 text-gray-700'

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700">Stato Operazione</h3>
        <span className={`rounded-full px-3 py-1 text-xs font-semibold ${colorClass}`}>
          {data.status}
        </span>
      </div>
      <div className="divide-y divide-gray-100">
        <Row label="ID Operazione" value={data.transportOperationId} />
        <Row label="Codice" value={data.operationCode} />
        <Row label="Dataset" value={data.datasetType} />
        <Row label="Gateway" value={data.gatewayProvider} />
        <Row label="ID Esterno" value={data.externalId} />
        <Row label="Tentativi" value={data.retryCount > 0 ? String(data.retryCount) : null} />
        <Row
          label="Inviato"
          value={data.sentAt ? new Date(data.sentAt).toLocaleString('it-IT') : null}
        />
        <Row
          label="Confermato"
          value={
            data.acknowledgedAt
              ? new Date(data.acknowledgedAt).toLocaleString('it-IT')
              : null
          }
        />
        <Row
          label="Creato"
          value={new Date(data.createdAt).toLocaleString('it-IT')}
        />
      </div>
      {!['ACKNOWLEDGED', 'FAILED', 'REJECTED'].includes(data.status) && (
        <p className="mt-3 text-center text-xs text-gray-400">
          Aggiornamento automatico ogni 3 secondi…
        </p>
      )}
    </div>
  )
}
