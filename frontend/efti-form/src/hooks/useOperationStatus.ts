import { useQuery } from '@tanstack/react-query'
import { getOperationStatus } from '../api/transportOperations'

const TERMINAL_STATUSES = ['ACKNOWLEDGED', 'FAILED', 'REJECTED']

export function useOperationStatus(operationId: string | null) {
  return useQuery({
    queryKey: ['operation-status', operationId],
    queryFn: () => getOperationStatus(operationId!),
    enabled: !!operationId,
    refetchInterval: (query) => {
      const status = query.state.data?.status
      return status && TERMINAL_STATUSES.includes(status) ? false : 3000
    },
  })
}
