import React, { PropsWithChildren } from "react";
import { ChatifyHubInitializer, ChatClientProvider } from "apps/app/hub";
import MainLayout from "./MainLayout";

export interface LayoutProps extends PropsWithChildren {
   params: Record<string, any>;
}

const Layout = async ({ children }: LayoutProps) => {
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
