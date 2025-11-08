import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    fs: {
      // Allow serving files from the EXTRAS directory at the repo root so Home page images resolve
      allow: [
        '.',
        '..',
        // Absolute path fallback (replace if user moves project)
        'c:/Users/enzod/Documents/CSCI Final/CSCI316_ProjectAPI/EXTRAS'
      ]
    }
  }
})
