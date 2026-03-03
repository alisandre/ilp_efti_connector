import type { ReactNode } from 'react'

interface Props {
  title: string
  children: ReactNode
  optional?: boolean
}

export function SectionCard({ title, children, optional }: Props) {
  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
      <h3 className="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-500">
        {title}
        {optional && (
          <span className="ml-2 rounded bg-gray-100 px-1.5 py-0.5 text-xs font-normal text-gray-400">
            opzionale
          </span>
        )}
      </h3>
      {children}
    </div>
  )
}
