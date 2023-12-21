import React, { PropsWithChildren } from "react";
import { cookies } from "next/headers";
import process from "process";
import { redirect } from "next/navigation";
import { RedirectType } from "next/dist/client/components/redirect";
import { OAuthProvider } from "../providers";

export interface LayoutProps extends PropsWithChildren {}

const Layout = async ({ children }: LayoutProps) => {
   const isUserLoggedIn = !!cookies().has(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );
   console.log({ isUserLoggedIn  });
   if (isUserLoggedIn) return redirect(`/`, RedirectType.push);

   return (
      <section>
         <OAuthProvider>{children}</OAuthProvider>
      </section>
   );
};

export default Layout;
