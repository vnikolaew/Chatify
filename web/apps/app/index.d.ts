/* eslint-disable @typescript-eslint/no-explicit-any */
declare module "*.svg" {
   const content: any;
   export const ReactComponent: any;
   export default content;
}

namespace NodeJS {
   interface ProcessEnv {
      NEXT_PUBLIC_BACKEND_API_URL: string;
      NEXT_PUBLIC_GITHUB_CLIENT_ID: string;
      NEXT_PUBLIC_GOOGLE_CLIENT_ID: string;
      NEXT_PUBLIC_GITHUB_CLIENT_SECRET: string;
      NEXT_PUBLIC_APPLICATION_COOKIE_NAME: string;
   }
}
