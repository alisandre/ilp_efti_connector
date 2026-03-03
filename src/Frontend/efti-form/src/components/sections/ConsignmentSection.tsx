import { useEffect, useState } from 'react'
import { useFieldArray, useFormContext } from 'react-hook-form'
import { SectionCard } from '../ui/SectionCard'
import { AddButton } from '../ui/AddButton'
import { RemoveButton } from '../ui/RemoveButton'
import { Field } from '../ui/Field'
import { Icon } from '../ui/Icon'
import { PackageRow } from './PackageRow'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

const DEFAULT_PACKAGE = { sortOrder: 1, itemQuantity: 1, grossWeight: 0, shippingMarks: '', typeCode: '' }

export function ConsignmentSection() {
  const {
    register,
    control,
    unregister,
    formState: { errors },
  } = useFormContext<PayloadFormValues>()

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'consignmentItems.packages',
    shouldUnregister: true,
  })

  const [showSection, setShowSection] = useState(false)

  // Deregistra i campi quando la sezione è nascosta per evitare
  // che useFieldArray includa packages:[] nel payload anche se non compilati
  useEffect(() => {
    if (!showSection) {
      unregister('consignmentItems', { keepValue: false })
      unregister('transportDetails', { keepValue: false })
    }
  }, [showSection, unregister])

  const e = errors.consignmentItems

  if (!showSection) {
    return <AddButton label="Aggiungi Merce" onClick={() => setShowSection(true)} />
  }

  return (
    <SectionCard title="Merce (ConsignmentItems)" optional icon="inventory_2">
      <div className="space-y-4">
        {/* Totali */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Field
            label="Quantità Totale"
            required
            type="number"
            min={1}
            error={e?.totalItemQuantity}
            {...register('consignmentItems.totalItemQuantity', { valueAsNumber: true, shouldUnregister: true })}
          />
          <Field
            label="Peso Totale (kg)"
            required
            type="number"
            step="0.01"
            min={0.01}
            error={e?.totalWeight}
            {...register('consignmentItems.totalWeight', { valueAsNumber: true, shouldUnregister: true })}
          />
          <Field
            label="Volume Totale (m³)"
            type="number"
            step="0.01"
            min={0}
            error={e?.totalVolume}
            {...register('consignmentItems.totalVolume', { valueAsNumber: true, shouldUnregister: true })}
          />
        </div>

        {/* Colli */}
        <div className="space-y-3">
          <p className="text-xs font-medium text-gray-500">Colli</p>
          {fields.map((field, index) => (
            <PackageRow key={field.id} index={index} onRemove={() => remove(index)} errors={errors} />
          ))}
          <button
            type="button"
            onClick={() => append({ ...DEFAULT_PACKAGE, sortOrder: fields.length + 1 })}
            className="inline-flex items-center justify-center gap-1.5 w-full rounded-md border border-dashed border-gray-300 py-1.5 text-xs text-gray-500 hover:border-brand-400 hover:text-brand-600"
          >
            <Icon name="add_box" size={20} /> Aggiungi collo
          </button>
        </div>

        {/* Dettagli trasporto */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field
            label="Tipo Merce"
            placeholder="opzionale"
            error={errors.transportDetails?.cargoType}
            {...register('transportDetails.cargoType', { shouldUnregister: true })}
          />
          <Field
            label="Incoterms"
            placeholder="es. DAP"
            error={errors.transportDetails?.incoterms}
            {...register('transportDetails.incoterms', { shouldUnregister: true })}
          />
        </div>

        <RemoveButton label="Rimuovi Merce" onClick={() => setShowSection(false)} />
      </div>
    </SectionCard>
  )
}
