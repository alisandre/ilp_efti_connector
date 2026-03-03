import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { Icon } from '../ui/Icon'
import type { PayloadFormValues } from '../../schemas/payloadSchema'
import type { FieldErrors } from 'react-hook-form'

interface Props {
  index: number
  onRemove: () => void
  errors: FieldErrors<PayloadFormValues>
}

export function PackageRow({ index, onRemove, errors }: Props) {
  const { register } = useFormContext<PayloadFormValues>()
  const e = errors.consignmentItems?.packages?.[index]

  return (
    <div className="rounded-md border border-gray-100 bg-gray-50 p-3">
      <div className="mb-2 flex items-center justify-between">
        <span className="text-xs font-medium text-gray-500">Collo {index + 1}</span>
        <button
          type="button"
          onClick={onRemove}
          className="inline-flex items-center gap-0.5 text-xs text-red-500 hover:text-red-700"
        >
          <Icon name="delete" size={20} /> Rimuovi
        </button>
      </div>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
        <Field
          label="Qtà"
          type="number"
          min={1}
          error={e?.itemQuantity}
          {...register(`consignmentItems.packages.${index}.itemQuantity`, { valueAsNumber: true, shouldUnregister: true })}
        />
        <Field
          label="Peso Lordo (kg)"
          type="number"
          step="0.01"
          error={e?.grossWeight}
          {...register(`consignmentItems.packages.${index}.grossWeight`, { valueAsNumber: true, shouldUnregister: true })}
        />
        <Field
          label="Tipo"
          placeholder="es. BOX"
          error={e?.typeCode}
          {...register(`consignmentItems.packages.${index}.typeCode`, { shouldUnregister: true })}
        />
        <Field
          label="Marchi Spedizione"
          placeholder="opzionale"
          error={e?.shippingMarks}
          {...register(`consignmentItems.packages.${index}.shippingMarks`, { shouldUnregister: true })}
        />
      </div>
    </div>
  )
}
