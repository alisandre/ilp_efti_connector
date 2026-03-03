import { useQuery } from '@tanstack/react-query'
import { getCustomers } from '../api/customers'

export function useCustomers() {
  return useQuery({
    queryKey: ['customers'],
    queryFn: getCustomers,
    staleTime: 60_000,
  })
}
