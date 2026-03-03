import { useState } from 'react'
import { TopBar } from '../ui/TopBar'
import { FormContent } from '../form/FormContent'
import { AuditLogPage } from '../auditlog/AuditLogPage'
import { TabBar } from '../ui/TabBar'

type Tab = 'form' | 'audit'

export function AppContent() {
  const [tab, setTab] = useState<Tab>('form')

  return (
    <>
      <TopBar />

      {/* Hero header con gradiente brand */}
      <div className="bg-gradient-to-r from-brand-600 to-brand-400 text-white">
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 py-8">
          <h3 className="text-3xl font-bold tracking-tight">eFTI Connector</h3>
          <p className="mt-1 text-brand-50 text-lg">Pannello di gestione operatore</p>
        </div>
      </div>

      <div className="min-h-screen bg-gray-50 py-8 px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-5xl">
          <TabBar activeTab={tab} onTabChange={setTab} />

          <div className={tab === 'form' ? '' : 'hidden'}>
            <FormContent />
          </div>
          {tab === 'audit' && <AuditLogPage />}
        </div>
      </div>
    </>
  )
}
