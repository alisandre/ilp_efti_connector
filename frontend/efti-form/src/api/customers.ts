import apiClient from './client'
import type { CustomerItem } from '../types/payload'

export const getCustomers = () =>
  apiClient
    .get<CustomerItem[]>('/forms/customers')
    .then((r) => r.data)

export const getCustomerByCode = (code: string) =>
  apiClient
    .get<CustomerItem>(`/forms/customers/${code}`)
    .then((r) => r.data)
