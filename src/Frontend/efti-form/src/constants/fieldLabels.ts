/** Mappa path RHF → etichetta leggibile per il toast di errore */
export const FIELD_LABELS: Record<string, string> = {
  operationCode: 'Codice Operazione',
  datasetType: 'Dataset Type',
  customerCode: 'Codice Cliente',
  customerName: 'Nome Cliente',
  customerVat: 'P.IVA Cliente',
  customerEori: 'EORI Cliente',
  destinationCode: 'Codice Destinazione',
  'consignee.name': 'Destinatario – Nome',
  'consignee.playerType': 'Destinatario – Player Type',
  'consignee.cityName': 'Destinatario – Città',
  'consignee.countryCode': 'Destinatario – Paese',
  'consignorAddress.cityName': 'Mittente – Città',
  'consignorAddress.countryCode': 'Mittente – Paese',
  'acceptanceLocation.cityName': 'Luogo Accettazione – Città',
  'acceptanceLocation.countryCode': 'Luogo Accettazione – Paese',
  'deliveryLocation.cityName': 'Luogo Consegna – Città',
  'deliveryLocation.countryCode': 'Luogo Consegna – Paese',
  carriers: 'Vettori (almeno uno richiesto)',
}

/** Appiattisce l'oggetto errori RHF in una lista di messaggi leggibili */
export function flattenErrors(
  errors: Record<string, unknown>,
  prefix = '',
): string[] {
  const messages: string[] = []
  for (const key of Object.keys(errors)) {
    const fullPath = prefix ? `${prefix}.${key}` : key
    const node = errors[key]
    if (!node) continue
    if ((node as { message?: string }).message) {
      const label = FIELD_LABELS[fullPath] ?? fullPath
      messages.push(`• ${label}: ${(node as { message: string }).message}`)
    } else if (Array.isArray(node)) {
      node.forEach((item: Record<string, unknown> | undefined, i: number) => {
        if (item) messages.push(...flattenErrors(item, `${fullPath}[${i}]`))
      })
    } else if (typeof node === 'object') {
      messages.push(...flattenErrors(node as Record<string, unknown>, fullPath))
    }
  }
  return messages
}
