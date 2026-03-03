import { useFieldArray, useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { SectionCard } from '../ui/SectionCard'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

export function ConsignmentSection() {
  const {
    register,
    control,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'consignmentItems.packages',
  })

  const e = errors.consignmentItems

  return (
    <SectionCard title="Merce (ConsignmentItems)" optional>
      <div className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Field
            label="Quantità Totale"
            type="number"
            min={1}
            error={e?.totalItemQuantity}
            {...register('consignmentItems.totalItemQuantity', { valueAsNumber: true })}
          />
          <Field
            label="Peso Totale (kg)"
            type="number"
            step="0.01"
            min={0}
            error={e?.totalWeight}
            {...register('consignmentItems.totalWeight', { valueAsNumber: true })}
          />
          <Field
            label="Volume Totale (m³)"
            type="number"
            step="0.01"
            min={0}
            error={e?.totalVolume}
            {...register('consignmentItems.totalVolume', { valueAsNumber: true })}
          />
        </div>

        <div className="space-y-3">
          <p className="text-xs font-medium text-gray-500">Colli</p>
          {fields.map((field, index) => {
            const pe = errors.consignmentItems?.packages?.[index]
            return (
              <div key={field.id} className="rounded-md border border-gray-100 bg-gray-50 p-3">
                <div className="mb-2 flex items-center justify-between">
                  <span className="text-xs font-medium text-gray-500">Collo {index + 1}</span>
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="text-xs text-red-500 hover:text-red-700"
                  >
                    Rimuovi
                  </button>
                </div>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                  <Field
                    label="Qtà"
                    type="number"
                    min={1}
                    error={pe?.itemQuantity}
                    {...register(`consignmentItems.packages.${index}.itemQuantity`, {
                      valueAsNumber: true,
                    })}
                  />
                  <Field
                    label="Peso Lordo (kg)"
                    type="number"
                    step="0.01"
                    error={pe?.grossWeight}
                    {...register(`consignmentItems.packages.${index}.grossWeight`, {
                      valueAsNumber: true,
                    })}
                  />
                  <Field
                    label="Tipo"
                    placeholder="es. BOX"
                    error={pe?.typeCode}
                    {...register(`consignmentItems.packages.${index}.typeCode`)}
                  />
                  <Field
                    label="Marchi Spedizione"
                    placeholder="opzionale"
                    error={pe?.shippingMarks}
                    {...register(`consignmentItems.packages.${index}.shippingMarks`)}
                  />
                </div>
              </div>
            )
          })}
          <button
            type="button"
            onClick={() =>
              append({
                sortOrder: fields.length + 1,
                itemQuantity: 1,
                grossWeight: 0,
                shippingMarks: '',
                typeCode: '',
              })
            }
            className="w-full rounded-md border border-dashed border-gray-300 py-1.5 text-xs text-gray-500 hover:border-brand-500 hover:text-brand-600"
          >
            + Aggiungi collo
          </button>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field
            label="Tipo Merce"
            placeholder="opzionale"
            error={errors.transportDetails?.cargoType}
            {...register('transportDetails.cargoType')}
          />
          <Field
            label="Incoterms"
            placeholder="es. DAP"
            error={errors.transportDetails?.incoterms}
            {...register('transportDetails.incoterms')}
          />
        </div>
      </div>
    </SectionCard>
  )
}
