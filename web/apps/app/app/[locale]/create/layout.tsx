import React, { PropsWithChildren } from "react";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { RedirectType } from "next/dist/client/components/redirect";

export interface LayoutProps extends PropsWithChildren {}

const Layout = async ({ children }: LayoutProps) => {
   const isUserLoggedIn = !!cookies().has(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );
   if (!isUserLoggedIn) return redirect(`signin`, RedirectType.push);

   return <section>{children}</section>;
};

export default Layout;
