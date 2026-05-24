import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";
import fs from "fs";
const keyFilePath = process.env.HTTPS_KEY_FILE || "./../../data/certs/dev.key";
const certFilePath = process.env.HTTPS_CERT_FILE || "./../../data/certs/dev.pem";
// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        plugin(),
        svgr({ include: ["**/*.svg?react", "**/*.svg?url&react"] })
    ],
    resolve: {
        alias: {
            "@": fileURLToPath(new URL("./src", import.meta.url))
        }
    },
    server: {
        headers: {
            "x-frame-options": "deny",
            "frame-ancestors": "none"
        },
        proxy: {
            "^/api/": {
                target: "https://localhost:9001",
                secure: false
            }
        },
        cors: {
            origin: [/^localhost:[0-9]+$/, /\.app\.local$/],
            exposedHeaders: ["Etsoo-Refresh-Token", "Content-Disposition"],
            credentials: true
        },
        host: true,
        port: 9002,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath)
        }
    }
});
