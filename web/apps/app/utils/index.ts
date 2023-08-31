export function hexToDecimal(hexString: string) {
   // Remove any leading "0x" if present
   if (hexString.startsWith("0x")) {
      hexString = hexString.substring(2);
   }

   // Convert the hexadecimal string to a decimal number
   return parseInt(hexString, 16);
}
