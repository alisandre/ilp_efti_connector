import keycloak from '../../keycloak'
import { IlpLogo } from './IlpLogo'
import { Icon } from './Icon'

function getRoleLabel(roles: string[]): string {
  if (roles.includes('efti-admin')) return 'Admin'
  if (roles.includes('efti-supervisor')) return 'Supervisor'
  if (roles.includes('efti-operator')) return 'Operatore'
  if (roles.includes('efti-service')) return 'Servizio'
  return ''
}

function getRoleBadgeColor(roles: string[]): string {
  if (roles.includes('efti-admin')) return 'bg-red-50 text-red-700 ring-red-600/20'
  if (roles.includes('efti-supervisor')) return 'bg-amber-50 text-amber-700 ring-amber-600/20'
  if (roles.includes('efti-operator')) return 'bg-blue-50 text-blue-700 ring-blue-600/20'
  return 'bg-gray-50 text-gray-600 ring-gray-500/20'
}

export function TopBar() {
  const token = keycloak.tokenParsed
  const username = token?.preferred_username ?? ''
  const firstName = token?.given_name as string | undefined
  const lastName = token?.family_name as string | undefined
  const fullName = [firstName, lastName].filter(Boolean).join(' ')
  const displayName = fullName || username
  const roles: string[] = (token?.realm_access as { roles?: string[] } | undefined)?.roles ?? []
  const roleLabel = getRoleLabel(roles)
  const roleBadgeColor = getRoleBadgeColor(roles)

  const handleLogout = () => {
    keycloak.logout({ redirectUri: window.location.origin })
  }

  if (!keycloak.authenticated) return null

  return (
    <div className="sticky top-0 z-50 bg-white border-b border-brand-100 shadow-sm">
      <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 h-14 flex items-center justify-between">
        {/* Logo */}
        <IlpLogo />

        <div className="flex items-center gap-3">
          {roleLabel && (
            <span
              className={`hidden sm:inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${roleBadgeColor}`}
            >
              {roleLabel}
            </span>
          )}

          <div className="flex items-center gap-2">
            <div className="flex h-7 w-7 items-center justify-center rounded-full bg-brand-600 text-white select-none">
              <Icon name="person" size={20} filled />
            </div>
            <span className="hidden sm:block text-sm font-medium text-gray-700">
              {displayName}
            </span>
          </div>

          <button
            onClick={handleLogout}
            className="inline-flex items-center gap-1 rounded-md border border-brand-200 px-3 py-1 text-xs font-medium text-brand-600 hover:bg-brand-50 hover:border-brand-400 transition-colors"
          >
            <Icon name="logout" size={20} />
            Logout
          </button>
        </div>
      </div>
    </div>
  )
}
