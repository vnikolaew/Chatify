import "./styles.css";
import React, { PropsWithChildren } from "react";
import { Metadata } from "next";
import { Inter, Roboto } from "next/font/google";
import Providers from "./providers";
import MainNavbar from "@components/navbar/Navbar";
import process from "process";
import { getImagesBaseUrl } from "@web/api";
import CookieConsentBannerModal from "@components/CookieConsentBannerModal";

export const metadata: Metadata = {
   title: "Chatify 2023",
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
      <html lang={`en`} className={`${inter.className} bg-gray-950 text-white`}>
         <body className={`dark`}>
            <Providers isDevelopment={__IS_DEV__()}>
               <MainNavbar baseImagesUrl={getImagesBaseUrl()} />
               <main className="app">{children}</main>
               <CookieConsentBannerModal />
               <footer
                  className={`w-full text-center mt-20 p-12 text-large border-t border-t-gray-700`}
               >
                  <h2 className={`text-foreground font-medium text-2xl`}>
                     Footer Area
                  </h2>
               </footer>
            </Providers>
         </body>
      </html>
   );
}

export default ChatifyLayout;
