"use client";

import { Metadata } from "next";
import "./styles.css";
import React, { PropsWithChildren } from "react";
import {
   QueryCache,
   QueryClient,
   QueryClientProvider,
} from "@tanstack/react-query";

// export const metadata: Metadata = {
//    title: "Welcome to app!",
//    description: "Chatify - converse with your fellas!",
// } satisfies Metadata;

const queryClient = new QueryClient({
   queryCache: new QueryCache({}),
   defaultOptions: {
      queries: {
         refetchOnWindowFocus: false,
      },
   },
});

function ChatifyLayout({ children }: PropsWithChildren) {
   return (
      <html>
         <body>
            <main className="app">
               <QueryClientProvider client={queryClient}>
                  {children}
               </QueryClientProvider>
            </main>
         </body>
      </html>
   );
}

export default ChatifyLayout;
