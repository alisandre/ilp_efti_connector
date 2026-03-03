import React from 'react'
import ReactDOM from 'react-dom/client'
import keycloak from './keycloak'
import './index.css'

// Initialize Keycloak and render the app
const renderApp = () => {
  import('./App').then(({ App }) => {
    ReactDOM.createRoot(document.getElementById('root')!).render(
      <React.StrictMode>
        <App />
      </React.StrictMode>
    )
  })
}

// Try to initialize Keycloak, but allow app to work without it
keycloak
  .init({
    onLoad: 'login-required',
    pkceMethod: 'S256',
    checkLoginIframe: false,
    flow: 'standard',
  })
  .then(() => {
    renderApp()
  })
  .catch((error) => {
    console.warn('Keycloak initialization failed:', error)
    renderApp()
  })

