import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default ({ mode }) => {
    const env = loadEnv(mode, process.cwd())
    console.log('VITE_BASE_URL: ', env.VITE_BASE_URL)

    return defineConfig({
        plugins: [react({ jsxRuntime: 'classic' })],
        root: "./src/CollabGateway.Client",
        server: {
            port: 8080,
            host: true,
            proxy: {
                '/api': env.VITE_BASE_URL || 'http://localhost:5000',
            }
        },
        build: {
            outDir: "../../publish/app/public"
        }
    })
}
