import "../styles.css";
import React, { PropsWithChildren } from "react";
import { Metadata } from "next";
import { Inter, Roboto } from "next/font/google";
import Providers from "./providers";
import MainNavbar from "@components/navbar/Navbar";
import process from "process";
import { getImagesBaseUrl } from "@web/api";
import CookieConsentBannerModal from "@components/CookieConsentBannerModal";
import { notFound } from "next/navigation";
import { NextIntlClientProvider } from "next-intl";
import Footer from "./footer";

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

async function ChatifyLayout({ children, params: { locale }, ...rest }: PropsWithChildren & { params: any }) {
   // Validate that the incoming `locale` parameter is valid
   let messages;
   try {
      messages = (await import(`../../messages/${locale}.json`)).default;
   } catch (error) {
      notFound();
   }

   return (
      <html lang={locale} className={`${inter.className} bg-gray-950 text-white`}>
      <body className={`dark`}>
      <Providers isDevelopment={__IS_DEV__()}>
         <NextIntlClientProvider locale={locale} messages={messages}>
            <MainNavbar baseImagesUrl={getImagesBaseUrl()} />
            <main className="app">{children}</main>
            <CookieConsentBannerModal />
            <Footer />
         </NextIntlClientProvider>
      </Providers>
      </body>
      </html>
   );
}

export default ChatifyLayout;
