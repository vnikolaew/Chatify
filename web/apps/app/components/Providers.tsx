"use client";
import React, { PropsWithChildren, useEffect } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@web/api";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { USER_LOCATION_LOCAL_STORAGE_KEY } from "@web/api";

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
         console.log(position);
      });
   }, []);

   return (
      <QueryClientProvider client={queryClient}>
         {children}
         {isDevelopment && (
            <ReactQueryDevtools position={"bottom-left"} initialIsOpen={true} />
         )}
      </QueryClientProvider>
   );
};

export default Providers;
