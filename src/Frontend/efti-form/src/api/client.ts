import axios from 'axios'
import keycloak from '../keycloak'

// The fixed source ID for manual form entry (TEST_FRONTEND seed)
export const TEST_FRONTEND_SOURCE_ID = '22222222-2222-2222-2222-222222222222'

const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Inject JWT + X-Source-Id on every request
apiClient.interceptors.request.use((config) => {
  if (keycloak.token) {
    config.headers.Authorization = `Bearer ${keycloak.token}`
  }
  config.headers['X-Source-Id'] = TEST_FRONTEND_SOURCE_ID
  return config
})

// Auto-refresh token when close to expiry
apiClient.interceptors.response.use(
  (r) => r,
  async (error) => {
    if (error.response?.status === 401) {
      try {
        const refreshed = await keycloak.updateToken(30)
        if (refreshed && error.config) {
          error.config.headers.Authorization = `Bearer ${keycloak.token}`
          return apiClient.request(error.config)
        }
      } catch {
        keycloak.login()
      }
    }
    return Promise.reject(error)
  },
)

export default apiClient
