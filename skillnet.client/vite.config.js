import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 5173,
        proxy: {
            '^/api': {
                target: 'http://localhost:5090',
                secure: false
            }
        }
    }
});