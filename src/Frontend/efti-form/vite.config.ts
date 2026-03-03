import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3100,
    proxy: {
      // QueryProxyService — deve venire PRIMA di /api
      '/api/query': {
        target: 'http://localhost:5021',
        changeOrigin: true,
      },
      // FormInputService
      '/api': {
        target: 'http://localhost:5006',
        changeOrigin: true,
      },
    },
  },
})
