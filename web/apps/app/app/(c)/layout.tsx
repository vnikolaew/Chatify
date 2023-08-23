import React, { PropsWithChildren } from "react";
import ChatGroupMembersSection from "@components/members/ChatGroupMembersSection";
import ChatGroupsFeed from "@components/feed/ChatGroupsFeed";

export interface LayoutProps extends PropsWithChildren {}

const Layout = async ({ children }: LayoutProps) => {
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
