import { forwardRef } from 'react'
import type { InputHTMLAttributes } from 'react'
import type { FieldError } from 'react-hook-form'

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: FieldError
}

export const Field = forwardRef<HTMLInputElement, Props>(
  ({ label, error, required, ...props }, ref) => {
    return (
      <div>
        <label className="mb-1 block text-sm font-medium text-gray-700">
          {label}
          {required && <span className="ml-0.5 text-red-500">*</span>}
        </label>
        <input
          type="text"
          ref={ref}
          {...props}
          aria-required={required}
          className={`w-full rounded-md border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-brand-400 focus:border-brand-400 ${
            error ? 'border-red-400 bg-red-50' : 'border-gray-300'
          } ${props.disabled ? 'bg-gray-50 text-gray-500' : ''}`}
        />
        {error && <p className="mt-1 text-xs text-red-600">{error.message}</p>}
      </div>
    )
  }
)

Field.displayName = 'Field'
