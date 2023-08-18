"use client";
import React, { PropsWithChildren, useEffect } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { NextUIProvider } from "@nextui-org/react";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { queryClient, USER_LOCATION_LOCAL_STORAGE_KEY } from "@web/api";
import { ThemeProvider as NextThemesProvider } from "next-themes";

export interface ProvidersProps extends PropsWithChildren {
   isDevelopment: boolean;
}

const Providers = ({ children, isDevelopment }: ProvidersProps) => {
   useEffect(() => {
      window.navigator.geolocation.getCurrentPosition((position) => {
         queryClient?.setQueryData(["user-geolocation"], position.coords);
         localStorage.setItem(
            USER_LOCATION_LOCAL_STORAGE_KEY,
            `${position.coords.latitude};${position.coords.longitude}`
         );
      });
   }, []);

   return (
      <QueryClientProvider client={queryClient}>
         <NextUIProvider>
            <NextThemesProvider defaultTheme={"dark"} attribute={"class"}>
               <GoogleOAuthProvider
                  clientId={process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID}
               >
                  {children}
               </GoogleOAuthProvider>
            </NextThemesProvider>
         </NextUIProvider>
         {isDevelopment && (
            <ReactQueryDevtools position={"bottom-left"} initialIsOpen={true} />
         )}
      </QueryClientProvider>
   );
};

export default Providers;
