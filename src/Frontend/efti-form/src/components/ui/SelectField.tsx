import { forwardRef } from 'react'
import type { SelectHTMLAttributes } from 'react'
import type { FieldError } from 'react-hook-form'

interface Props extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  error?: FieldError
  options: { value: string; label: string }[]
  placeholder?: string
}

export const SelectField = forwardRef<HTMLSelectElement, Props>(
  ({ label, error, options, placeholder, required, ...props }, ref) => {
    return (
      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">
          {label}
          {required && <span className="ml-0.5 text-red-500">*</span>}
        </label>
        <select
          ref={ref}
          {...props}
          aria-required={required}
          className={`w-full rounded-md border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-brand-400 focus:border-brand-400 ${
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
)

SelectField.displayName = 'SelectField'
