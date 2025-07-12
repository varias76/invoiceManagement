import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173, // Asegura que Vite siempre intente usar este puerto
    proxy: {
      '/api': {
        target: 'http://localhost:5296', // ¡VERIFICA ESTE PUERTO! Debe ser el de tu backend.
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
        // Ya no incluimos la función 'configure' aquí
        // proxy.on('proxyReq', (proxyReq, req, res) => {
        //   console.log('Proxying request (frontend to backend):', req.method, req.url, '->', proxy.target + proxyReq.path);
        // });
        // proxy.on('error', (err, req, res) => {
        //   console.error('Proxy error:', err);
        // }) // <-- ¡ESTA COMA ES LA QUE HAY QUE ELIMINAR!

        // ELIMINAR LA COMA DE LA LÍNEA 19 SI QUEDÓ AL COMENTAR proxy.on('error', ...)
        // Dejar la configuración del proxy limpia como estaba antes, sin el configure ni los on.
        // La solución más sencilla es que quede así:
        // '/api': {
        //   target: 'http://localhost:5296',
        //   changeOrigin: true,
        //   rewrite: (path) => path.replace(/^\/api/, ''),
        // },
        // La captura muestra que ya eliminaste la función configure, pero la coma puede haber quedado.
        // Asegúrate que la línea 19 NO TENGA COMA al final si la línea 20 (el proxy.on('error')) está comentada o eliminada.
      } // <-- Aquí termina el objeto para '/api'. NO DEBE HABER COMA SI NO HAY OTRO OBJETO PROXY DESPUÉS.
    },
  },
});