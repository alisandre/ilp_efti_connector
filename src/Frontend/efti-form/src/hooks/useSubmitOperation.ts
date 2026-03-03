import { useMutation } from '@tanstack/react-query'
import { submitOperation, validatePayload } from '../api/transportOperations'
import type { SourcePayloadDto } from '../types/payload'

export function useSubmitOperation() {
  return useMutation({
    mutationFn: (payload: SourcePayloadDto) => submitOperation(payload),
  })
}

export function useValidateOperation() {
  return useMutation({
    mutationFn: (payload: SourcePayloadDto) => validatePayload(payload),
  })
}
