import { Icon } from './Icon'

type Tab = 'form' | 'audit'

interface Props {
  activeTab: Tab
  onTabChange: (tab: Tab) => void
}

const TABS: { id: Tab; icon: string; label: string }[] = [
  { id: 'form',  icon: 'edit_document', label: 'Inserimento manuale e-CMR' },
  { id: 'audit', icon: 'manage_search',  label: 'Audit Log' },
]

export function TabBar({ activeTab, onTabChange }: Props) {
  return (
    <div className="mb-6 border-b border-gray-200">
      <nav className="-mb-px flex gap-6">
        {TABS.map(({ id, icon, label }) => (
          <button
            key={id}
            onClick={() => onTabChange(id)}
            className={`inline-flex items-center gap-1.5 pb-3 text-sm font-medium transition-colors ${
              activeTab === id
                ? 'border-b-2 border-brand-600 text-brand-600'
                : 'text-gray-500 hover:text-brand-500'
            }`}
          >
            <Icon name={icon} size={20} />
            {label}
          </button>
        ))}
      </nav>
    </div>
  )
}
