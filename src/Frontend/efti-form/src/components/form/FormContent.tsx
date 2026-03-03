import { useState } from 'react'
import { FormProvider, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import toast from 'react-hot-toast'
import { payloadSchema, type PayloadFormValues } from '../../schemas/payloadSchema'
import { useSubmitOperation } from '../../hooks/useSubmitOperation'
import { DEFAULT_VALUES } from '../../constants/defaultValues'
import { flattenErrors } from '../../constants/fieldLabels'
import { OperationSection } from '../sections/OperationSection'
import { ConsigneeSection } from '../sections/ConsigneeSection'
import { CarriersSection } from '../sections/CarriersSection'
import { LocationsSection } from '../sections/LocationsSection'
import { ConsignmentSection } from '../sections/ConsignmentSection'
import { OperationStatusCard } from '../status/OperationStatusCard'
import { SpinnerIcon } from '../ui/SpinnerIcon'
import { Icon } from '../ui/Icon'
import type { SourcePayloadDto } from '../../types/payload'

export function FormContent() {
  const [submittedId, setSubmittedId] = useState<string | null>(null)
  const { mutate: submit, isPending } = useSubmitOperation()

  const methods = useForm<PayloadFormValues>({
    resolver: zodResolver(payloadSchema),
    mode: 'onTouched',
    reValidateMode: 'onChange',
    defaultValues: structuredClone(DEFAULT_VALUES),
  })

  const onSubmit = (data: PayloadFormValues) => {
    submit(data as SourcePayloadDto, {
      onSuccess: (res) => {
        setSubmittedId(res.transportOperationId.toString())
        toast.success(`Operazione inviata — ID: ${res.transportOperationId}`)
        methods.reset(structuredClone(DEFAULT_VALUES))
      },
      onError: () => {
        toast.error("Errore durante l'invio. Verificare i dati e riprovare.")
      },
    })
  }

  const onValidationError = (errors: Record<string, unknown>) => {
    const lines = flattenErrors(errors)
    const detail = lines.length ? `\n${lines.join('\n')}` : ''
    toast.error(`Compilare tutti i campi obbligatori prima di inviare.${detail}`, {
      duration: 6000,
      style: { whiteSpace: 'pre-line', maxWidth: '480px' },
    })
    document
      .querySelector('[aria-invalid="true"], .border-red-400')
      ?.scrollIntoView({ behavior: 'smooth', block: 'center' })
  }

  return (
    <FormProvider {...methods}>
      <form
        noValidate
        onSubmit={methods.handleSubmit(onSubmit, onValidationError)}
        className="space-y-6"
      >
        <OperationSection />
        <ConsigneeSection />
        <CarriersSection />
        <LocationsSection />
        <ConsignmentSection />

        {submittedId && (
          <div className="mt-4">
            <OperationStatusCard operationId={submittedId} />
          </div>
        )}

        <div className="flex items-center justify-between border-t border-gray-200 pt-6">
          <button
            type="button"
            onClick={() => { methods.reset(structuredClone(DEFAULT_VALUES)); setSubmittedId(null) }}
            className="inline-flex items-center gap-1.5 rounded-md border border-brand-200 bg-white px-4 py-2 text-sm font-medium text-brand-600 hover:bg-brand-50 transition-colors"
          >
            <Icon name="restart_alt" size={20} />
            Azzera form
          </button>
          <button
            type="submit"
            disabled={isPending}
            className="inline-flex items-center gap-2 rounded-md bg-brand-600 px-6 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-brand-700 disabled:opacity-60 disabled:cursor-not-allowed transition-colors"
          >
            {isPending ? (
              <>
                <SpinnerIcon className="h-4 w-4 animate-spin" />
                Invio in corso…
              </>
            ) : (
              <>
                <Icon name="send" size={20} />
                Invia e-CMR
              </>
            )}
          </button>
        </div>
      </form>
    </FormProvider>
  )
}
