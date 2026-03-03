// Mirror dei DTO AuditLog del QueryProxyService

export const AUDIT_ENTITY_TYPES = [
  'Customer',
  'CustomerDestination',
  'Source',
  'TransportOperation',
  'EftiMessage',
  'User',
] as const

export const AUDIT_ACTION_TYPES = [
  'Create',
  'Read',
  'Update',
  'Delete',
  'Send',
  'Receive',
  'Query',
  'Export',
] as const

export type AuditEntityType = (typeof AUDIT_ENTITY_TYPES)[number]
export type AuditActionType = (typeof AUDIT_ACTION_TYPES)[number]

export interface AuditLogEntry {
  id: string
  entityType: string
  entityId: string
  actionType: string
  performedByUserId: string | null
  performedBySourceId: string | null
  description: string
  ipAddress: string | null
  createdAt: string
}

export interface AuditLogDetail extends AuditLogEntry {
  oldValueJson: string | null
  newValueJson: string | null
  userAgent: string | null
}

export interface AuditLogFilters {
  entityType?: string
  actionType?: string
  from?: string
  to?: string
  page: number
  pageSize: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}
