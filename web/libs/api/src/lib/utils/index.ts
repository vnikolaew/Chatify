export function isServer() {
   return global.window === undefined || global.window === null;
}

export function sleep(ms: number): Promise<void> {
   return new Promise((res) => setTimeout(res, ms));
}
