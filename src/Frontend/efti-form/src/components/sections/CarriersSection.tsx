import { useFieldArray, useFormContext } from 'react-hook-form'
import { SectionCard } from '../ui/SectionCard'
import { CarrierRow } from './CarrierRow'
import { Icon } from '../ui/Icon'
import type { PayloadFormValues } from '../../schemas/payloadSchema'
import { DEFAULT_CARRIER } from '../../constants/defaultValues'

export function CarriersSection() {
  const {
    control,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

  const { fields, append, remove } = useFieldArray({ control, name: 'carriers' })

  return (
    <SectionCard title="Vettori (Carriers)" icon="local_shipping">
      <div className="space-y-6">
        {fields.map((field, index) => (
          <CarrierRow
            key={field.id}
            index={index}
            showRemove={fields.length > 1}
            onRemove={() => remove(index)}
            errors={errors}
          />
        ))}
        <button
          type="button"
          onClick={() => append({ ...DEFAULT_CARRIER, sortOrder: fields.length + 1 })}
          className="inline-flex items-center justify-center gap-1.5 w-full rounded-md border-2 border-dashed border-gray-300 py-2 text-sm text-gray-500 hover:border-brand-400 hover:text-brand-600"
        >
          <Icon name="add_circle" size={20} /> Aggiungi vettore
        </button>
        {errors.carriers?.root && (
          <p className="text-xs text-red-600">{errors.carriers.root.message}</p>
        )}
      </div>
    </SectionCard>
  )
}
