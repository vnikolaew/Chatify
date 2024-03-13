// env.d.ts
declare global {
   namespace NodeJS {
      interface ProcessEnv {
         NEXT_PUBLIC_BACKEND_API_URL: string;
         NEXT_PUBLIC_GOOGLE_CLIENT_ID: string;
         NEXT_PUBLIC_GITHUB_CLIENT_SECRET: string;
         NEXT_PUBLIC_APPLICATION_COOKIE_NAME: string;
         // Add more variables as needed
      }
   }
}

declare module "uuid";
