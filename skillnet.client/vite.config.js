import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 5173,
        proxy: {
            '^/api': {
                target: 'https://localhost:7295',
                secure: false
            }
        }
    }
});