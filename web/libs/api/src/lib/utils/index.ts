import process from "process";

export const __IS_SERVER__ =
   global.window === undefined || global.window === null;

export function sleep(ms: number): Promise<void> {
   return new Promise((res) => setTimeout(res, ms));
}

export function getImagesBaseUrl() {
   const url = new URL(process.env["NEXT_PUBLIC_BACKEND_API_URL"]!);
   return `${url.protocol}//${url.hostname}:${url.port}/static`;
}

export function isLocalUrl(url: string) {
   const currentHostname = window.location.hostname;
   try {
      const urlToCheck = new URL(url);
      return urlToCheck.hostname === currentHostname;
   } catch (err) {
      return true;
   }
}

export function getMediaUrl(url: string) {
   if (!url) return null!;
   return isLocalUrl(url) ? `${getImagesBaseUrl()}/${url}` : url;
}

const URL_PATTERN_REGEX = /^(https?|ftp):\/\/[^\s/$.?#].\S*$/i;

export function isValidURL(url: string | null) {
   // Regular expression pattern to match a valid absolute URL
   return URL_PATTERN_REGEX.test(url ?? "");
}
