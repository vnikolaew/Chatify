"use client";
import React, { PropsWithChildren } from "react";
import ChatGroupsFeed from "@components/feed/ChatGroupsFeed";
import { ChatGroupSidebar } from "@components/members";

export interface MainLayoutProps extends PropsWithChildren {}

const MainLayout = ({ children }: MainLayoutProps) => {
   return (
      <div className={`w-full flex items-start`}>
         <div className={`grow-[1] resize-x max-w-[400px]`} id={`sidebar`}>
            <ChatGroupsFeed />
         </div>
         <div id={`main`} className={`grow-[5]`}>
            {children}
         </div>
         <div id={`members`} className={`grow-[1]`}>
            <ChatGroupSidebar />
         </div>
      </div>
   );
};

export default MainLayout;
