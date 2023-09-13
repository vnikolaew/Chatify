"use client";
import React, { Fragment } from "react";
import { useSearchParams } from "next/navigation";
import { ScrollShadow, Tab, Tabs } from "@nextui-org/react";
import ChatGroupMembersTab from "@components/members/ChatGroupMembersTab";
import ChatGroupAttachmentsTab from "@components/members/ChatGroupAttachmentsTab";

export interface ChatGroupSidebarProps {}

const ChatGroupSidebar = ({}: ChatGroupSidebarProps) => {
   const params = useSearchParams();
   const chatGroupId = params.get("c");

   return (
      <ScrollShadow
         size={60}
         className={`flex flex-col items-start py-2 max-h-[90vh] overflow-y-scroll min-h-[80vh] h-full border-b-1 border-b-default-200 border-l-1 border-l-default-200 pb-10 rounded-medium`}
      >
         {!chatGroupId ? (
            <div
               className={`w-full h-full items-center justify-center px-4 py-2`}
            >
               <h2 className={`text-large text-default-400`}>
                  There's no selected chat group.
               </h2>
            </div>
         ) : (
            <Fragment>
               <Tabs
                  className={`px-4 text-foreground w-full mx-auto py-2`}
                  defaultSelectedKey={`members`}
                  variant={`light`}
                  size={`md`}
                  color={`primary`}
               >
                  <Tab className={`w-full`} title={`Members`} key={`members`}>
                     <ChatGroupMembersTab chatGroupId={chatGroupId} />
                  </Tab>
                  <Tab title={`Media`} key={`media`}>
                     <ChatGroupAttachmentsTab chatGroupId={chatGroupId} />
                  </Tab>
               </Tabs>
            </Fragment>
         )}
      </ScrollShadow>
   );
};

export default ChatGroupSidebar;
