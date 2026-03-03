import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { SectionCard } from '../ui/SectionCard'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

interface LocationFieldsProps {
  prefix: 'acceptanceLocation' | 'deliveryLocation' | 'consignorAddress'
  showDate?: boolean
}

function LocationFields({ prefix, showDate }: LocationFieldsProps) {
  const {
    register,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

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

export function LocationsSection() {
  return (
    <div className="space-y-4">
      <SectionCard title="Indirizzo Mittente (ConsignorAddress)" optional>
        <LocationFields prefix="consignorAddress" />
      </SectionCard>
      <SectionCard title="Luogo di Accettazione" optional>
        <LocationFields prefix="acceptanceLocation" showDate />
      </SectionCard>
      <SectionCard title="Luogo di Consegna" optional>
        <LocationFields prefix="deliveryLocation" />
      </SectionCard>
    </div>
  )
}
