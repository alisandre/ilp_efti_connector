import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { SectionCard } from '../ui/SectionCard'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

export function ConsigneeSection() {
  const {
    register,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

  const e = errors.consignee

  return (
    <SectionCard title="Destinatario (Consignee)" icon="person_pin">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field
          label="Nome"
          required
          placeholder="es. Luca Bianchi SpA"
          error={e?.name}
          {...register('consignee.name')}
        />
        <Field
          label="Player Type"
          required
          placeholder="es. CARRIER, SHIPPER"
          error={e?.playerType}
          {...register('consignee.playerType')}
        />
        <Field
          label="Partita IVA"
          placeholder="opzionale"
          error={e?.taxRegistration}
          {...register('consignee.taxRegistration')}
        />
        <Field
          label="Codice EORI"
          placeholder="opzionale"
          error={e?.eoriCode}
          {...register('consignee.eoriCode')}
        />
        <Field
          label="Via"
          placeholder="opzionale"
          error={e?.streetName}
          {...register('consignee.streetName')}
        />
        <Field
          label="CAP"
          placeholder="opzionale"
          error={e?.postCode}
          {...register('consignee.postCode')}
        />
        <Field
          label="Città"
          required
          placeholder="es. Milano"
          error={e?.cityName}
          {...register('consignee.cityName')}
        />
        <Field
          label="Codice Paese (ISO)"
          required
          placeholder="es. IT"
          maxLength={2}
          error={e?.countryCode}
          {...register('consignee.countryCode')}
        />
        <Field
          label="Nome Paese"
          placeholder="opzionale"
          error={e?.countryName}
          {...register('consignee.countryName')}
        />
      </div>
    </SectionCard>
  )
}
