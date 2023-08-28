import React, { PropsWithChildren } from "react";
import ChatGroupMembersSection from "@components/members/ChatGroupMembersSection";
import ChatGroupsFeed from "@components/feed/ChatGroupsFeed";
import { cookies } from "next/headers";
import process from "process";

export interface LayoutProps extends PropsWithChildren {
   params: Record<string, any>;
}

const Layout = async ({ children, params }: LayoutProps) => {
   const isUserLoggedIn = !!cookies().has(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );
   // if (!isUserLoggedIn) return redirect(`/signin`, RedirectType.push);

   return (
      <div className={`flex border-b-1 border-b-default-200 items-start gap-0`}>
         <div className="grow-[1] max-w-[400px]">
            <ChatGroupsFeed />
         </div>
         <div className={`grow-[5]`}>{children}</div>
         <div className={`grow-[2]`}>
            <ChatGroupMembersSection />
         </div>
      </div>
   );
};

export default Layout;
