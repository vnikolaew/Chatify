import React, { PropsWithChildren } from "react";
import { Metadata } from "next";
import { Inter, Roboto } from "next/font/google";
import Providers from "./providers";
import process from "process";
import { notFound } from "next/navigation";
import { NextIntlClientProvider } from "next-intl";
import Footer from "./footer";
import { CookieConsentBannerModal, MainNavbar } from "@web/components";

export const metadata: Metadata = {
   title: `Chatify - ${new Date().getFullYear()}`,
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

export function __IS_COOKIE_CONSENT_ENABLED__() {
   return process.env.COOKIE_CONSENT_ENABLED === "1";
}

async function ChatifyLayout({
   children,
   params: { locale },
   ...rest
}: PropsWithChildren & { params: any }) {
   // Validate that the incoming `locale` parameter is valid
   let messages;
   try {
      messages = (await import(`../../messages/${locale}.json`)).default;
   } catch (error) {
      notFound();
   }

   return (
      <html
         lang={locale}
         className={`${inter.className} bg-gray-950 text-white`}
      >
         <body className={`dark`}>
            <Providers>
               <NextIntlClientProvider locale={locale} messages={messages}>
                  <MainNavbar />
                  <main className="app">{children}</main>
                  {__IS_COOKIE_CONSENT_ENABLED__() && (
                     <CookieConsentBannerModal />
                  )}
                  <Footer />
               </NextIntlClientProvider>
            </Providers>
         </body>
      </html>
   );
}

export default ChatifyLayout;
