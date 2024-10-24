import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react({ jsxRuntime: 'classic'})],
    root: "./src/CollabGateway.Client",
    server: {
        port: 8080,
        host: true,
        proxy: {
            '/api': import.meta.env.VITE_BASE_URL || 'http://localhost:5000',
        }
    },
    build: {
        outDir: "../../publish/app/public"
    }
})
