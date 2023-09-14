import React, { PropsWithChildren } from "react";
import { cookies } from "next/headers";
import process from "process";
import { redirect } from "next/navigation";
import { RedirectType } from "next/dist/client/components/redirect";

export interface LayoutProps extends PropsWithChildren {
   redirectTo: string;
}

const ProtectedLayout = ({ children, redirectTo }: LayoutProps) => {
   const isUserLoggedIn = !!cookies().has(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );
   if (!isUserLoggedIn) return redirect(redirectTo, RedirectType.push);

   return <section>{children}</section>;
};

export default ProtectedLayout;
