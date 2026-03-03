import { Icon } from './Icon'

interface Props {
  label: string
  onClick: () => void
}

export function AddButton({ label, onClick }: Props) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex items-center gap-2 rounded-md border border-dashed border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-500 hover:border-brand-400 hover:text-brand-600 transition-colors w-full justify-center"
    >
      <Icon name="add_circle" size={20} />
      {label}
    </button>
  )
}
