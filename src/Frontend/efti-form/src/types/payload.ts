// Mirror of SourcePayloadDto and related records from the backend

export const DATASET_TYPES = ['ECMR', 'EDDT', 'eAWB', 'eBL', 'eRSD', 'eDAD'] as const
export type DatasetType = (typeof DATASET_TYPES)[number]

export interface ConsignorAddressDto {
  streetName?: string
  postCode?: string
  cityName: string
  countryCode: string
  countryName?: string
}

export interface ConsigneeDto {
  name: string
  playerType: string
  taxRegistration?: string
  eoriCode?: string
  streetName?: string
  postCode?: string
  cityName: string
  countryCode: string
  countryName?: string
}

export interface CarrierDto {
  sortOrder: number
  name: string
  playerType: string
  tractorPlate: string
  taxRegistration?: string
  eoriCode?: string
  equipmentCategory?: string
  streetName?: string
  postCode?: string
  cityName: string
  countryCode: string
  countryName?: string
}

export interface AcceptanceLocationDto {
  streetName?: string
  postCode?: string
  cityName: string
  countryCode: string
  countryName?: string
  date?: string
}

export interface DeliveryLocationDto {
  streetName?: string
  postCode?: string
  cityName: string
  countryCode: string
  countryName?: string
}

export interface PackageDto {
  sortOrder: number
  shippingMarks?: string
  itemQuantity: number
  typeCode?: string
  grossWeight: number
  grossVolume?: number
}

export interface ConsignmentItemsDto {
  totalItemQuantity: number
  totalWeight: number
  totalVolume?: number
  packages: PackageDto[]
}

export interface TransportDetailsDto {
  cargoType?: string
  incoterms?: string
}

export interface HashcodeDto {
  value: string
  algorithm: string
}

export interface SourcePayloadDto {
  operationCode: string
  datasetType: string
  customerCode: string
  customerName: string
  customerVat?: string
  customerEori?: string
  destinationCode?: string
  consignorAddress?: ConsignorAddressDto
  consignee: ConsigneeDto
  carriers: CarrierDto[]
  acceptanceLocation?: AcceptanceLocationDto
  deliveryLocation?: DeliveryLocationDto
  consignmentItems?: ConsignmentItemsDto
  transportDetails?: TransportDetailsDto
  hashcode?: HashcodeDto
}

// API responses
export interface FormSubmitResponse {
  transportOperationId: string
  correlationId: string
  status: string
}

export interface FormOperationStatusResponse {
  transportOperationId: string
  operationCode: string
  datasetType: string
  status: string
  gatewayProvider?: string
  externalId?: string
  retryCount: number
  sentAt?: string
  acknowledgedAt?: string
  createdAt: string
  updatedAt: string
}

export interface ValidationError {
  propertyName: string
  errorMessage: string
}

export interface FormValidationResult {
  isValid: boolean
  errors: ValidationError[]
}

export interface CustomerItem {
  code: string
  name: string
  vat?: string
  eori?: string
}
