import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
var baseFolder = env.APPDATA !== undefined && env.APPDATA !== ""
    ? "".concat(env.APPDATA, "/ASP.NET/https")
    : "".concat(env.HOME, "/.aspnet/https");
fs.mkdirSync(baseFolder, { recursive: true });
var certificateName = "platform.client";
var certFilePath = path.join(baseFolder, "".concat(certificateName, ".pem"));
var keyFilePath = path.join(baseFolder, "".concat(certificateName, ".key"));
if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !==
        child_process.spawnSync("dotnet", [
            "dev-certs",
            "https",
            "--export-path",
            certFilePath,
            "--format",
            "Pem",
            "--no-password"
        ], { stdio: "inherit" }).status) {
        throw new Error("Could not create certificate.");
    }
}
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
