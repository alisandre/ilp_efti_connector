import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

type LocationPrefix = 'acceptanceLocation' | 'deliveryLocation' | 'consignorAddress'

interface Props {
  prefix: LocationPrefix
  showDate?: boolean
}

export function LocationFields({ prefix, showDate }: Props) {
  const { register, formState: { errors } } = useFormContext<PayloadFormValues>()
  const e = errors[prefix] as Record<string, { message?: string }> | undefined

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <Field
        label="Via"
        placeholder="opzionale"
        error={e?.streetName as never}
        {...register(`${prefix}.streetName` as never)}
      />
      <Field
        label="CAP"
        placeholder="opzionale"
        error={e?.postCode as never}
        {...register(`${prefix}.postCode` as never)}
      />
      <Field
        label="Città"
        required
        error={e?.cityName as never}
        {...register(`${prefix}.cityName` as never)}
      />
      <Field
        label="Codice Paese (ISO)"
        required
        maxLength={2}
        placeholder="es. IT"
        error={e?.countryCode as never}
        {...register(`${prefix}.countryCode` as never)}
      />
      <Field
        label="Nome Paese"
        placeholder="opzionale"
        error={e?.countryName as never}
        {...register(`${prefix}.countryName` as never)}
      />
      {showDate && (
        <Field
          label="Data"
          type="datetime-local"
          error={e?.date as never}
          {...register(`${prefix}.date` as never)}
        />
      )}
    </div>
  )
}
