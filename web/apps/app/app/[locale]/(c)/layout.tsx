import React, { PropsWithChildren } from "react";
import { ChatifyHubInitializer, ChatClientProvider } from "apps/app/hub";
import MainLayout from "./MainLayout";

export interface LayoutProps extends PropsWithChildren {
   params: Record<string, any>;
}

const Layout = async ({ children, params }: LayoutProps) => {
   // const isUserLoggedIn = !!cookies().has(
   //    process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   // );
   // if (!isUserLoggedIn) return redirect(`/signin`, RedirectType.push);

   return (
      <div className={`flex items-start gap-0`}>
         <ChatClientProvider>
            <ChatifyHubInitializer />
            <MainLayout>{children}</MainLayout>
         </ChatClientProvider>
      </div>
   );
};

export default Layout;
