import { useState } from 'react'
import { AUDIT_ACTION_TYPES, AUDIT_ENTITY_TYPES } from '../../types/auditLog'
import type { AuditLogFilters } from '../../types/auditLog'

interface Props {
  onSearch: (filters: Omit<AuditLogFilters, 'page' | 'pageSize'>) => void
}

export function AuditLogFilters({ onSearch }: Props) {
  const [entityType, setEntityType] = useState('')
  const [actionType, setActionType] = useState('')
  const [from,       setFrom      ] = useState('')
  const [to,         setTo        ] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSearch({
      entityType: entityType || undefined,
      actionType: actionType || undefined,
      from:       from       || undefined,
      to:         to         || undefined,
    })
  }

  const handleReset = () => {
    setEntityType('')
    setActionType('')
    setFrom('')
    setTo('')
    onSearch({})
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm"
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {/* Entity Type */}
        <div>
          <label className="mb-1 block text-xs font-medium text-gray-600">
            Tipo entità
          </label>
          <select
            value={entityType}
            onChange={(e) => setEntityType(e.target.value)}
            className="w-full rounded border border-gray-300 bg-white px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          >
            <option value="">Tutte</option>
            {AUDIT_ENTITY_TYPES.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </div>

        {/* Action Type */}
        <div>
          <label className="mb-1 block text-xs font-medium text-gray-600">
            Azione
          </label>
          <select
            value={actionType}
            onChange={(e) => setActionType(e.target.value)}
            className="w-full rounded border border-gray-300 bg-white px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          >
            <option value="">Tutte</option>
            {AUDIT_ACTION_TYPES.map((a) => (
              <option key={a} value={a}>{a}</option>
            ))}
          </select>
        </div>

        {/* From */}
        <div>
          <label className="mb-1 block text-xs font-medium text-gray-600">
            Dal
          </label>
          <input
            type="datetime-local"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          />
        </div>

        {/* To */}
        <div>
          <label className="mb-1 block text-xs font-medium text-gray-600">
            Al
          </label>
          <input
            type="datetime-local"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="w-full rounded border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
          />
        </div>
      </div>

      <div className="mt-4 flex gap-2">
        <button
          type="submit"
          className="rounded bg-blue-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-blue-700"
        >
          Cerca
        </button>
        <button
          type="button"
          onClick={handleReset}
          className="rounded border border-gray-300 px-4 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-50"
        >
          Azzera
        </button>
      </div>
    </form>
  )
}
