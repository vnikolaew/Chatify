"use client";
import React, { PropsWithChildren, useEffect } from "react";
import ChatGroupsFeed from "@components/feed/ChatGroupsFeed";
import { ChatGroupSidebar } from "@components/sidebar";

export interface MainLayoutProps extends PropsWithChildren {}

export class ChatGroupChangedEvent extends Event {
   public from: string;
   public to: string;

   constructor(type: string, from: string, to: string) {
      super(type);
      this.from = from;
      this.to = to;
   }
}

const MainLayout = ({ children }: MainLayoutProps) => {
   useEffect(() => {
      const { pushState, replaceState } = window.history;
      window.history.pushState = function (...args) {
         const fromId = new URLSearchParams(window.location.search).get(`c`);

         pushState.apply(window.history, args);
         const toId = new URLSearchParams(window.location.search).get(`c`);
         window.dispatchEvent(
            new ChatGroupChangedEvent("pushState", fromId, toId)
         );
      };

      window.history.replaceState = function (...args) {
         const fromId = new URLSearchParams(window.location.search).get(`c`);
         replaceState.apply(window.history, args);
         const toId = new URLSearchParams(window.location.search).get(`c`);
         window.dispatchEvent(
            new ChatGroupChangedEvent("pushState", fromId, toId)
         );
      };
   }, []);

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
