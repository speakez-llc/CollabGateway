import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const baseUrl = process.env.VITE_BASE_URL || 'http://localhost:5000'

export default defineConfig({
    plugins: [react({ jsxRuntime: 'classic'})],
    root: "./src/CollabGateway.Client",
    server: {
        port: 8080,
        host: true,
        proxy: {
            '/api': baseUrl,
        }
    },
    build: {
        outDir: "../../publish/app/public"
    }
})
