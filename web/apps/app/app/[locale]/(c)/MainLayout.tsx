"use client";
import React, { PropsWithChildren, useEffect } from "react";
import { ChatGroupsFeed, ChatGroupSidebar } from "@web/components";

export interface MainLayoutProps extends PropsWithChildren {
}

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
      window.history.pushState = function(...args) {
         const fromId = new URLSearchParams(window.location.search).get(`c`);

         pushState.apply(window.history, args);
         const toId = new URLSearchParams(window.location.search).get(`c`);
         window.dispatchEvent(
            new ChatGroupChangedEvent("pushState", fromId, toId),
         );
      };

      window.history.replaceState = function(...args) {
         const fromId = new URLSearchParams(window.location.search).get(`c`);
         replaceState.apply(window.history, args);
         const toId = new URLSearchParams(window.location.search).get(`c`);
         window.dispatchEvent(
            new ChatGroupChangedEvent("pushState", fromId, toId),
         );
      };
   }, []);

   return (
      <div className={`w-full flex items-start`}>
         <section className={`grow-[1] h-full resize-x max-w-[400px]`} id={`sidebar`}>
            <ChatGroupsFeed />
         </section>
         <section id={`main`} className={`grow-[5]`}>
            {children}
         </section>
         <section id={`members`} className={`grow-[1]`}>
            <ChatGroupSidebar />
         </section>
      </div>
   );
};

export default MainLayout;
