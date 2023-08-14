import "./styles.css";
import React, { PropsWithChildren } from "react";
import { Metadata } from "next";
import { Inter, Roboto } from "next/font/google";
import Providers from "./providers";
import Link from "next/link";

export const metadata: Metadata = {
   title: "Welcome to app!",
   description: "Chatify - converse with your fellas!",
   icons: [],
} satisfies Metadata;

const inter = Inter({
   weight: ["100", "200", "300", "500"],
   subsets: ["latin"],
});

const roboto = Roboto({
   weight: ["100", "300", "500"],
   subsets: ["latin"],
});

export function __IS_DEV__() {
   return process.env.NODE_ENV === "development";
}

async function ChatifyLayout({ children, ...rest }: PropsWithChildren) {
   return (
      <html
         lang={`en`}
         className={`${inter.className} bg-gray-950 text-white dark`}
      >
         <body>
            <main className="app">
               <Providers isDevelopment={__IS_DEV__()}>{children}</Providers>
            </main>
         </body>
      </html>
   );
}

export default ChatifyLayout;
