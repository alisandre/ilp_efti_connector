import { z } from 'zod'
import { DATASET_TYPES } from '../types/payload'

const countryCode = z
  .string()
  .length(2, 'Deve essere un codice ISO 3166-1 alpha-2 (2 caratteri)')

export const consignorAddressSchema = z.object({
  streetName: z.string().optional(),
  postCode: z.string().optional(),
  cityName: z.string().min(1, 'CityName è obbligatoria'),
  countryCode,
  countryName: z.string().optional(),
})

export const consigneeSchema = z.object({
  name: z.string().min(1, 'Nome destinatario obbligatorio').max(200),
  playerType: z.string().min(1, 'PlayerType obbligatorio'),
  taxRegistration: z.string().optional(),
  eoriCode: z.string().optional(),
  streetName: z.string().optional(),
  postCode: z.string().optional(),
  cityName: z.string().min(1, 'CityName obbligatoria'),
  countryCode,
  countryName: z.string().optional(),
})

export const carrierSchema = z.object({
  sortOrder: z.number().int().min(1),
  name: z.string().min(1, 'Nome vettore obbligatorio').max(200),
  playerType: z.string().min(1, 'PlayerType obbligatorio'),
  tractorPlate: z.string().min(1, 'Targa trattore obbligatoria').max(20),
  taxRegistration: z.string().optional(),
  eoriCode: z.string().optional(),
  equipmentCategory: z.string().optional(),
  streetName: z.string().optional(),
  postCode: z.string().optional(),
  cityName: z.string().min(1, 'CityName obbligatoria'),
  countryCode,
  countryName: z.string().optional(),
})

export const acceptanceLocationSchema = z.object({
  streetName: z.string().optional(),
  postCode: z.string().optional(),
  cityName: z.string().min(1, 'CityName obbligatoria'),
  countryCode,
  countryName: z.string().optional(),
  date: z.string().optional(),
})

export const deliveryLocationSchema = z.object({
  streetName: z.string().optional(),
  postCode: z.string().optional(),
  cityName: z.string().min(1, 'CityName obbligatoria'),
  countryCode,
  countryName: z.string().optional(),
})

export const packageSchema = z.object({
  sortOrder: z.number().int().min(1),
  shippingMarks: z.string().optional(),
  itemQuantity: z.number().int().min(1, 'Quantità deve essere > 0'),
  typeCode: z.string().optional(),
  grossWeight: z.number().positive('Peso deve essere > 0'),
  grossVolume: z.number().positive().optional(),
})

export const consignmentItemsSchema = z.object({
  totalItemQuantity: z.number().int().min(1, 'TotalItemQuantity deve essere > 0'),
  totalWeight: z.number().positive('TotalWeight deve essere > 0'),
  totalVolume: z.number().positive().optional(),
  packages: z.array(packageSchema),
})

export const transportDetailsSchema = z.object({
  cargoType: z.string().optional(),
  incoterms: z.string().optional(),
})

export const hashcodeSchema = z.object({
  value: z.string().min(1, 'Valore hashcode obbligatorio'),
  algorithm: z.string().min(1, 'Algoritmo obbligatorio'),
})

export const payloadSchema = z.object({
  operationCode: z
    .string()
    .min(1, 'OperationCode è obbligatorio')
    .max(100, 'OperationCode non può superare 100 caratteri'),
  datasetType: z.enum(DATASET_TYPES, {
    errorMap: () => ({ message: `DatasetType non valido. Valori ammessi: ${DATASET_TYPES.join(', ')}` }),
  }),
  customerCode: z.string().min(1, 'CustomerCode è obbligatorio').max(50),
  customerName: z.string().min(1, 'CustomerName è obbligatorio').max(200),
  customerVat: z.string().optional(),
  customerEori: z.string().optional(),
  destinationCode: z.string().optional(),
  consignorAddress: consignorAddressSchema.optional(),
  consignee: consigneeSchema,
  carriers: z
    .array(carrierSchema)
    .min(1, 'Almeno un vettore è obbligatorio'),
  acceptanceLocation: acceptanceLocationSchema.optional(),
  deliveryLocation: deliveryLocationSchema.optional(),
  consignmentItems: consignmentItemsSchema.optional(),
  transportDetails: transportDetailsSchema.optional(),
  hashcode: hashcodeSchema.optional(),
})

export type PayloadFormValues = z.infer<typeof payloadSchema>
