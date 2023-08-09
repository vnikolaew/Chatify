import "./styles.css";
import React, { PropsWithChildren } from "react";
import { Metadata } from "next";
import { Inter, Roboto } from "next/font/google";
import Providers from "../components/Providers";

export const metadata: Metadata = {
   title: "Welcome to app!",
   description: "Chatify - converse with your fellas!",
} satisfies Metadata;

const inter = Inter({
   weight: ["100", "200", "300", "500"],
   subsets: ["latin"],
});

const roboto = Roboto({
   weight: ["100", "300", "500"],
   subsets: ["latin"],
});

function ChatifyLayout({ children }: PropsWithChildren) {
   return (
      <html className={inter.className}>
         <body>
            <main className="app">
               <Providers>{children}</Providers>
            </main>
         </body>
      </html>
   );
}

export default ChatifyLayout;
