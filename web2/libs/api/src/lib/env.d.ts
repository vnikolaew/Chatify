// env.d.ts
declare global {
   namespace NodeJS {
      interface ProcessEnv {
         BASE_API_URL: string;
         // Add more variables as needed
      }
   }
}
