import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// A dev-szerver szándékosan a 4200-as porton fut: a backend CORS-beállítása
// (Cors:AllowedOrigin) ezt az origint engedi. A strictPort miatt inkább hibát
// kapunk, mint csendben egy másik portot – az ugyanis CORS-hibához vezetne.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 4200,
    strictPort: true
  }
});
