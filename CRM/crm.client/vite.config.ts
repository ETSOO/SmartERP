import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-react";
import fs from "fs";
import { visualizer } from "rollup-plugin-visualizer";

const keyFilePath = process.env.HTTPS_KEY_FILE || "./../../certs/dev.key";
const certFilePath =
  process.env.HTTPS_CERT_FILE || "./../../certs/dev.pem";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    plugin(),
    visualizer({
      open: true,
      filename: "dist/stats.html",
      gzipSize: true,
      brotliSize: true,
      template: "treemap" // 也可以是 sunburst 或 network
    })
  ],
  define: {
    "process.env.DRAGGABLE_DEBUG": false
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url))
    }
  },
  server: {
    proxy: {
      "^/api/": {
        target: "https://localhost:9010",
        secure: false
      }
    },
    cors: {
      origin: [/^localhost:[0-9]+$/, /\.app\.local$/],
      exposedHeaders: ["Etsoo-Refresh-Token", "Content-Disposition"],
      credentials: true
    },
    host: true,
    port: 9011,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath)
    }
  }
});
