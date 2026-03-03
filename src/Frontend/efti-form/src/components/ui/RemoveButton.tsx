import { Icon } from './Icon'

interface Props {
  label: string
  onClick: () => void
}

export function RemoveButton({ label, onClick }: Props) {
  return (
    <div className="mt-4 flex justify-end">
      <button
        type="button"
        onClick={onClick}
        className="inline-flex items-center gap-1 text-xs text-red-500 hover:text-red-700 transition-colors"
      >
        <Icon name="delete" size={20} />
        {label}
      </button>
    </div>
  )
}
