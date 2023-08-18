import process from "process";

export function isServer() {
   return global.window === undefined || global.window === null;
}

export function sleep(ms: number): Promise<void> {
   return new Promise((res) => setTimeout(res, ms));
}

export function getImagesBaseUrl() {
   const url = new URL(process.env["NEXT_PUBLIC_BACKEND_API_URL"]!);
   const imagesBaseUrl = `${url.protocol}//${url.hostname}:${url.port}/static`;
   return imagesBaseUrl;
}
