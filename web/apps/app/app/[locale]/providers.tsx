"use client";
import React, { Fragment, PropsWithChildren, useEffect } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { NextUIProvider } from "@nextui-org/react";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { queryClient, USER_LOCATION_LOCAL_STORAGE_KEY } from "@web/api";
import { ThemeProvider as NextThemesProvider } from "next-themes";

export interface ProvidersProps extends PropsWithChildren {
   isDevelopment: boolean;
}

export const OAuthProvider = ({ children }: PropsWithChildren) => {
   return (
      <GoogleOAuthProvider clientId={process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID}>
         {children}
      </GoogleOAuthProvider>
   );
};

const RememberUserGeolocation = () => {
   useEffect(() => {
      window.navigator.geolocation.getCurrentPosition((position) => {
         queryClient?.setQueryData(["user-geolocation"], position.coords);
         localStorage.setItem(
            USER_LOCATION_LOCAL_STORAGE_KEY,
            `${position.coords.latitude};${position.coords.longitude}`
         );
      });
   }, []);

   return <Fragment />;
};

const Providers = ({ children, isDevelopment }: ProvidersProps) => {
   return (
      <QueryClientProvider client={queryClient}>
         <RememberUserGeolocation />
         <NextUIProvider>
            <NextThemesProvider defaultTheme={"dark"} attribute={"class"}>
               <OAuthProvider>{children}</OAuthProvider>
            </NextThemesProvider>
         </NextUIProvider>
         {isDevelopment && (
            <ReactQueryDevtools position={"bottom-left"} initialIsOpen={true} />
         )}
      </QueryClientProvider>
   );
};

export default Providers;
