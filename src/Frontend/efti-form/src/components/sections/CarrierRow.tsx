import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { Icon } from '../ui/Icon'
import type { PayloadFormValues } from '../../schemas/payloadSchema'
import type { FieldErrors } from 'react-hook-form'

interface Props {
  index: number
  onRemove: () => void
  showRemove: boolean
  errors: FieldErrors<PayloadFormValues>
}

export function CarrierRow({ index, onRemove, showRemove, errors }: Props) {
  const { register } = useFormContext<PayloadFormValues>()
  const e = errors.carriers?.[index]

  return (
    <div className="rounded-md border border-gray-100 bg-gray-50 p-4">
      <div className="mb-3 flex items-center justify-between">
        <span className="text-sm font-medium text-gray-600">Vettore {index + 1}</span>
        {showRemove && (
          <button type="button" onClick={onRemove} className="inline-flex items-center gap-0.5 text-xs text-red-500 hover:text-red-700">
            <Icon name="delete" size={20} /> Rimuovi
          </button>
        )}
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Nome" required error={e?.name} {...register(`carriers.${index}.name`)} />
        <Field label="Player Type" required placeholder="es. CARRIER" error={e?.playerType} {...register(`carriers.${index}.playerType`)} />
        <Field label="Targa Trattore" required placeholder="es. AB123CD" error={e?.tractorPlate} {...register(`carriers.${index}.tractorPlate`)} />
        <Field label="Categoria Equipaggiamento" placeholder="opzionale" error={e?.equipmentCategory} {...register(`carriers.${index}.equipmentCategory`)} />
        <Field label="P.IVA" placeholder="opzionale" error={e?.taxRegistration} {...register(`carriers.${index}.taxRegistration`)} />
        <Field label="Codice EORI" placeholder="opzionale" error={e?.eoriCode} {...register(`carriers.${index}.eoriCode`)} />
        <Field label="Via" placeholder="opzionale" error={e?.streetName} {...register(`carriers.${index}.streetName`)} />
        <Field label="CAP" placeholder="opzionale" error={e?.postCode} {...register(`carriers.${index}.postCode`)} />
        <Field label="Città" required error={e?.cityName} {...register(`carriers.${index}.cityName`)} />
        <Field label="Codice Paese (ISO)" required maxLength={2} placeholder="es. IT" error={e?.countryCode} {...register(`carriers.${index}.countryCode`)} />
      </div>
    </div>
  )
}
