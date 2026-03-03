import { FormProvider, useForm } from 'react-hook-form'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'react-hot-toast'
import { zodResolver } from '@hookform/resolvers/zod'
import { payloadSchema, type PayloadFormValues } from './schemas/payloadSchema'
import { OperationSection } from './components/sections/OperationSection'
import { ConsigneeSection } from './components/sections/ConsigneeSection'
import { CarriersSection } from './components/sections/CarriersSection'
import { LocationsSection } from './components/sections/LocationsSection'
import { ConsignmentSection } from './components/sections/ConsignmentSection'

const queryClient = new QueryClient()

function FormContent() {
  const methods = useForm<PayloadFormValues>({
    resolver: zodResolver(payloadSchema),
    mode: 'onChange',
    defaultValues: {
      operationCode: '',
      datasetType: undefined,
      customerCode: '',
      customerName: '',
      customerVat: '',
      customerEori: '',
      destinationCode: '',
    },
  })

  return (
    <FormProvider {...methods}>
      <div className="min-h-screen bg-gray-50 py-8 px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-4xl">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">ILP eFTI — Inserimento manuale</h1>
            <p className="mt-2 text-gray-600">Compila il modulo per sottomettere una nuova operazione di trasporto</p>
          </div>

          <form className="space-y-6">
            <OperationSection />
            <ConsigneeSection />
            <CarriersSection />
            <LocationsSection />
            <ConsignmentSection />
          </form>
        </div>
      </div>
    </FormProvider>
  )
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <FormContent />
      <Toaster position="top-right" />
    </QueryClientProvider>
  )
}

