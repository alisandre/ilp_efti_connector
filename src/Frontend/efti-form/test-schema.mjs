// This file can be deleted
import { z } from './node_modules/zod/lib/index.mjs'

const payloadSchema = z.object({
  operationCode: z.string().min(1, 'OperationCode è obbligatorio'),
  customerCode: z.string().min(1, 'CustomerCode è obbligatorio'),
  customerName: z.string().min(1, 'CustomerName è obbligatorio'),
  datasetType: z.enum(['ECMR', 'EDDT', 'eAWB', 'eBL', 'eRSD', 'eDAD']),
  consignee: z.object({
    name: z.string().min(1, 'Nome obbligatorio'),
    playerType: z.string().min(1),
    cityName: z.string().min(1),
    countryCode: z.string().length(2),
  }),
  carriers: z.array(z.object({
    name: z.string().min(1, 'Nome vettore obbligatorio'),
    tractorPlate: z.string().min(1),
    playerType: z.string().min(1),
    sortOrder: z.number().int().min(1),
    cityName: z.string().min(1),
    countryCode: z.string().length(2),
  })).min(1),
})

// Simulate what RHF would pass to zodResolver when operationCode = 'op1'
const testValues = {
  operationCode: 'op1',
  datasetType: 'ECMR',
  customerCode: 'CLI-001',
  customerName: 'Giuseppe Verdi srl',
  customerVat: '',
  customerEori: '',
  destinationCode: '',
  consignee: {
    name: '',          // <-- empty, should fail
    playerType: 'CONSIGNEE',
    cityName: 'Rome',
    countryCode: 'IT',
  },
  carriers: [{
    sortOrder: 1,
    name: '',          // <-- empty, should fail
    playerType: 'CARRIER',
    tractorPlate: '',  // <-- empty, should fail
    cityName: 'Rome',
    countryCode: 'IT',
  }],
}

const result = payloadSchema.safeParse(testValues)
if (!result.success) {
  const errors = result.error.errors
  console.log('All errors:')
  errors.forEach(e => console.log(`  path: [${e.path.join('.')}], message: "${e.message}"`, e.code))
  const opCodeError = errors.find(e => e.path[0] === 'operationCode')
  console.log('\noperationCode error:', opCodeError ?? 'NONE - field is valid ✓')
} else {
  console.log('Validation passed ✓')
}
