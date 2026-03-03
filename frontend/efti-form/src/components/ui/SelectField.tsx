import type { SelectHTMLAttributes } from 'react'
import type { FieldError } from 'react-hook-form'

interface Props extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  error?: FieldError
  options: { value: string; label: string }[]
  placeholder?: string
}

export function SelectField({ label, error, options, placeholder, ...props }: Props) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-gray-700">
        {label}
        {props.required && <span className="ml-0.5 text-red-500">*</span>}
      </label>
      <select
        {...props}
        className={`w-full rounded-md border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-brand-500 ${
          error ? 'border-red-400 bg-red-50' : 'border-gray-300'
        } bg-white`}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      {error && <p className="mt-1 text-xs text-red-600">{error.message}</p>}
    </div>
  )
}
