import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import process from 'process'; // <-- 1. This fixes the 'process' error

// Your target definition
const target = process.env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
    : process.env.ASPNETCORE_URLS
        ? process.env.ASPNETCORE_URLS.split(';')[0]
        : 'https://localhost:7198';

export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            // 2. Only ONE '^/api' block here!
            '^/api': {
                target,
                secure: false
            }
        },
        port: 5173
    }
});