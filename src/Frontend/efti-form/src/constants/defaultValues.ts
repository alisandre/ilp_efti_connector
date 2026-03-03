import type { PayloadFormValues } from '../schemas/payloadSchema'

export const DEFAULT_VALUES: PayloadFormValues = {
  operationCode: 'OP1',
  datasetType: 'ECMR',
  customerCode: 'CUST-001',
  customerName: 'Giuseppe Verdi srl',
  customerVat: '',
  customerEori: '',
  destinationCode: '',
  consignee: {
    name: 'Mario Rossi SRL',
    playerType: 'CONSIGNEE',
    taxRegistration: '',
    eoriCode: '',
    streetName: '',
    postCode: '',
    cityName: 'Rome',
    countryCode: 'IT',
    countryName: '',
  },
  carriers: [
    {
      sortOrder: 1,
      name: 'Bertoldo SPA',
      playerType: 'CARRIER',
      tractorPlate: 'AA123AA',
      taxRegistration: '',
      eoriCode: '',
      equipmentCategory: '',
      streetName: '',
      postCode: '',
      cityName: 'Rome',
      countryCode: 'IT',
      countryName: '',
    },
  ],
}

export const DEFAULT_CARRIER = {
  sortOrder: 1,
  name: '',
  playerType: 'CARRIER',
  tractorPlate: '',
  taxRegistration: '',
  eoriCode: '',
  equipmentCategory: '',
  streetName: '',
  postCode: '',
  cityName: '',
  countryCode: 'IT',
  countryName: '',
}
