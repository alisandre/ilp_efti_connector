import axios from 'axios'
import keycloak from '../keycloak'
import type { AuditLogDetail, AuditLogEntry, AuditLogFilters, PagedResult } from '../types/auditLog'

// Istanza separata per il QueryProxyService (porta 5021, proxy su /api/query)
const queryClient = axios.create({
  baseURL: '/api/query',
  headers: { 'Content-Type': 'application/json' },
})

queryClient.interceptors.request.use((config) => {
  if (keycloak.token) {
    config.headers.Authorization = `Bearer ${keycloak.token}`
  }
  return config
})

export const getAuditLogs = (filters: AuditLogFilters) => {
  const params: Record<string, string | number | undefined> = {
    page:     filters.page,
    pageSize: filters.pageSize,
  }
  if (filters.entityType) params.entityType = filters.entityType
  if (filters.actionType) params.actionType = filters.actionType
  if (filters.from)       params.from       = filters.from
  if (filters.to)         params.to         = filters.to

  return queryClient
    .get<PagedResult<AuditLogEntry>>('/audit-logs', { params })
    .then((r) => r.data)
}

export const getAuditLogById = (id: string) =>
  queryClient
    .get<AuditLogDetail>(`/audit-logs/${id}`)
    .then((r) => r.data)
