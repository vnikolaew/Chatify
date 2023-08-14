export function isServer() {
   return global.window === undefined || global.window === null;
}
