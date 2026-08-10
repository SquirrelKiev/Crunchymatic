import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  // served from wwwroot/subtool/
  base: '/subtool/',
  plugins: [svelte(), tailwindcss()],
  server: {
    port: 5173,
    strictPort: true,
    ws: {
      clientPort: 5173
    },
  },
  build: {
    outDir: '../wwwroot/subtool/',
    emptyOutDir: true,
    rolldownOptions: {
      input: {
        subtool: 'src/main.ts'
      },
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: 'chunks/[name]-[hash].js',
        assetFileNames: '[name][extname]'
      }
    }
  }
})
