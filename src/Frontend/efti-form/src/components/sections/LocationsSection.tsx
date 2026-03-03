import { useState } from 'react'
import { useFormContext } from 'react-hook-form'
import { SectionCard } from '../ui/SectionCard'
import { AddButton } from '../ui/AddButton'
import { RemoveButton } from '../ui/RemoveButton'
import { LocationFields } from './LocationFields'
import type { PayloadFormValues } from '../../schemas/payloadSchema'

export function LocationsSection() {
  const { setValue } = useFormContext<PayloadFormValues>()
  const [showConsignor, setShowConsignor] = useState(false)
  const [showAcceptance, setShowAcceptance] = useState(false)
  const [showDelivery, setShowDelivery] = useState(false)

  return (
    <div className="space-y-4">
      {showConsignor ? (
        <SectionCard title="Indirizzo Mittente" optional icon="home_pin">

          <LocationFields prefix="consignorAddress" />
          <RemoveButton label="Rimuovi Indirizzo Mittente" onClick={() => { setShowConsignor(false); setValue('consignorAddress', undefined) }} />
        </SectionCard>
      ) : (
        <AddButton label="Aggiungi Indirizzo Mittente" onClick={() => setShowConsignor(true)} />
      )}

      {showAcceptance ? (
        <SectionCard title="Luogo di Accettazione" optional icon="where_to_vote">
          <LocationFields prefix="acceptanceLocation" showDate />
          <RemoveButton label="Rimuovi Luogo di Accettazione" onClick={() => { setShowAcceptance(false); setValue('acceptanceLocation', undefined) }} />
        </SectionCard>
      ) : (
        <AddButton label="Aggiungi Luogo di Accettazione" onClick={() => setShowAcceptance(true)} />
      )}

      {showDelivery ? (
        <SectionCard title="Luogo di Consegna" optional icon="flag">
          <LocationFields prefix="deliveryLocation" />
          <RemoveButton label="Rimuovi Luogo di Consegna" onClick={() => { setShowDelivery(false); setValue('deliveryLocation', undefined) }} />
        </SectionCard>
      ) : (
        <AddButton label="Aggiungi Luogo di Consegna" onClick={() => setShowDelivery(true)} />
      )}
    </div>
  )
}
