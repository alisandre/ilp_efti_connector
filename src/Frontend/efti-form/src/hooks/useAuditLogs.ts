import { useQuery, keepPreviousData } from '@tanstack/react-query'
import { getAuditLogs, getAuditLogById } from '../api/auditLogs'
import type { AuditLogFilters } from '../types/auditLog'

export function useAuditLogs(filters: AuditLogFilters) {
  return useQuery({
    queryKey: ['auditLogs', filters],
    queryFn:  () => getAuditLogs(filters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  })
}

export function useAuditLogDetail(id: string | null) {
  return useQuery({
    queryKey: ['auditLog', id],
    queryFn:  () => getAuditLogById(id!),
    enabled:  !!id,
    staleTime: 60_000,
  })
}
