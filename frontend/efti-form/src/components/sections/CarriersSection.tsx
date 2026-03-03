import { useFieldArray, useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { SectionCard } from '../ui/SectionCard'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

const DEFAULT_CARRIER = {
  sortOrder: 1,
  name: '',
  playerType: '',
  tractorPlate: '',
  taxRegistration: '',
  eoriCode: '',
  equipmentCategory: '',
  streetName: '',
  postCode: '',
  cityName: '',
  countryCode: '',
  countryName: '',
}

export function CarriersSection() {
  const {
    register,
    control,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

  const { fields, append, remove } = useFieldArray({ control, name: 'carriers' })

  return (
    <SectionCard title="Vettori (Carriers)">
      <div className="space-y-6">
        {fields.map((field, index) => {
          const e = errors.carriers?.[index]
          return (
            <div key={field.id} className="rounded-md border border-gray-100 bg-gray-50 p-4">
              <div className="mb-3 flex items-center justify-between">
                <span className="text-sm font-medium text-gray-600">Vettore {index + 1}</span>
                {fields.length > 1 && (
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="text-xs text-red-500 hover:text-red-700"
                  >
                    Rimuovi
                  </button>
                )}
              </div>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <Field
                  label="Nome"
                  required
                  error={e?.name}
                  {...register(`carriers.${index}.name`)}
                />
                <Field
                  label="Player Type"
                  required
                  placeholder="es. CARRIER"
                  error={e?.playerType}
                  {...register(`carriers.${index}.playerType`)}
                />
                <Field
                  label="Targa Trattore"
                  required
                  placeholder="es. AB123CD"
                  error={e?.tractorPlate}
                  {...register(`carriers.${index}.tractorPlate`)}
                />
                <Field
                  label="Categoria Equipaggiamento"
                  placeholder="opzionale"
                  error={e?.equipmentCategory}
                  {...register(`carriers.${index}.equipmentCategory`)}
                />
                <Field
                  label="P.IVA"
                  placeholder="opzionale"
                  error={e?.taxRegistration}
                  {...register(`carriers.${index}.taxRegistration`)}
                />
                <Field
                  label="Codice EORI"
                  placeholder="opzionale"
                  error={e?.eoriCode}
                  {...register(`carriers.${index}.eoriCode`)}
                />
                <Field
                  label="Via"
                  placeholder="opzionale"
                  error={e?.streetName}
                  {...register(`carriers.${index}.streetName`)}
                />
                <Field
                  label="CAP"
                  placeholder="opzionale"
                  error={e?.postCode}
                  {...register(`carriers.${index}.postCode`)}
                />
                <Field
                  label="Città"
                  required
                  error={e?.cityName}
                  {...register(`carriers.${index}.cityName`)}
                />
                <Field
                  label="Codice Paese (ISO)"
                  required
                  maxLength={2}
                  placeholder="es. IT"
                  error={e?.countryCode}
                  {...register(`carriers.${index}.countryCode`)}
                />
              </div>
            </div>
          )
        })}
        <button
          type="button"
          onClick={() => append({ ...DEFAULT_CARRIER, sortOrder: fields.length + 1 })}
          className="w-full rounded-md border-2 border-dashed border-gray-300 py-2 text-sm text-gray-500 hover:border-brand-500 hover:text-brand-600"
        >
          + Aggiungi vettore
        </button>
        {errors.carriers?.root && (
          <p className="text-xs text-red-600">{errors.carriers.root.message}</p>
        )}
      </div>
    </SectionCard>
  )
}
