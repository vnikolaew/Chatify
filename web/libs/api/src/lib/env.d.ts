// env.d.ts
declare global {
   namespace NodeJS {
      interface ProcessEnv {
         NEXT_PUBLIC_BACKEND_API_URL: string;
         // Add more variables as needed
      }
   }
}
