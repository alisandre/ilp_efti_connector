import apiClient from './client'
import type {
  FormOperationStatusResponse,
  FormSubmitResponse,
  FormValidationResult,
  SourcePayloadDto,
} from '../types/payload'

const BASE = '/forms/transport-operations'

export const validatePayload = (payload: SourcePayloadDto) =>
  apiClient
    .post<FormValidationResult>(`${BASE}/validate`, payload)
    .then((r) => r.data)

export const submitOperation = (payload: SourcePayloadDto) =>
  apiClient
    .post<FormSubmitResponse>(BASE, payload)
    .then((r) => r.data)

export const getOperationStatus = (id: string) =>
  apiClient
    .get<FormOperationStatusResponse>(`${BASE}/${id}/status`)
    .then((r) => r.data)
