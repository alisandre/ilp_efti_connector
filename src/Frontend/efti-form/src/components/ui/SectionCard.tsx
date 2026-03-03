import type { ReactNode } from 'react'
import { Icon } from './Icon'

interface Props {
  title: string
  children: ReactNode
  optional?: boolean
  icon?: string
}

export function SectionCard({ title, children, optional, icon }: Props) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <div className="mb-4 flex items-center gap-2">
        <span className="inline-block h-4 w-1 rounded-full bg-gradient-to-b from-brand-400 to-cyan-400 flex-shrink-0" />
        {icon && <Icon name={icon} size={20} className="text-brand-500" />}
        <h3 className="text-sm font-semibold uppercase tracking-wide text-brand-600">
          {title}
        </h3>
        {optional && (
          <span className="rounded bg-brand-50 px-1.5 py-0.5 text-xs font-normal text-brand-400 ring-1 ring-brand-100">
            opzionale
          </span>
        )}
      </div>
      {children}
    </div>
  )
}
