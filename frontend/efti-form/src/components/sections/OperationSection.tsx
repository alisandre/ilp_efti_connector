import { useFormContext } from 'react-hook-form'
import { Field } from '../ui/Field'
import { SelectField } from '../ui/SelectField'
import { SectionCard } from '../ui/SectionCard'
import { DATASET_TYPES } from '../../types/payload'
import type { PayloadFormValues } from '../../schemas/payloadSchema'
import { useCustomers } from '../../hooks/useCustomers'

export function OperationSection() {
  const {
    register,
    formState: { errors },
    setValue,
    watch,
  } = useFormContext<PayloadFormValues>()

  const { data: customers } = useCustomers()
  const selectedCode = watch('customerCode')

  const handleCustomerSelect = (code: string) => {
    const customer = customers?.find((c) => c.code === code)
    if (customer) {
      setValue('customerCode', customer.code)
      setValue('customerName', customer.name)
      if (customer.vat) setValue('customerVat', customer.vat)
      if (customer.eori) setValue('customerEori', customer.eori)
    }
  }

  return (
    <SectionCard title="Operazione di Trasporto">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field
          label="Codice Operazione"
          required
          placeholder="es. OP-2026-001"
          error={errors.operationCode}
          {...register('operationCode')}
        />
        <SelectField
          label="Dataset Type"
          required
          placeholder="Seleziona tipo..."
          options={DATASET_TYPES.map((t) => ({ value: t, label: t }))}
          error={errors.datasetType as never}
          {...register('datasetType')}
        />

        {customers && customers.length > 0 && (
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Cliente (da lista)
            </label>
            <select
              className="w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-brand-500"
              value={selectedCode}
              onChange={(e) => handleCustomerSelect(e.target.value)}
            >
              <option value="">— Seleziona o inserisci manualmente —</option>
              {customers.map((c) => (
                <option key={c.code} value={c.code}>
                  {c.code} — {c.name}
                </option>
              ))}
            </select>
          </div>
        )}

        <Field
          label="Codice Cliente"
          required
          placeholder="es. CLIENTE_001"
          error={errors.customerCode}
          {...register('customerCode')}
        />
        <Field
          label="Nome Cliente"
          required
          placeholder="es. Mario Rossi Srl"
          error={errors.customerName}
          {...register('customerName')}
        />
        <Field
          label="P.IVA Cliente"
          placeholder="es. IT12345678901"
          error={errors.customerVat}
          {...register('customerVat')}
        />
        <Field
          label="EORI Cliente"
          placeholder="es. IT123456789"
          error={errors.customerEori}
          {...register('customerEori')}
        />
        <Field
          label="Codice Destinazione"
          placeholder="opzionale"
          error={errors.destinationCode}
          {...register('destinationCode')}
        />
      </div>
    </SectionCard>
  )
}
