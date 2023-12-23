"use client";
import React, { Fragment } from "react";
import { ScrollShadow, Tab, Tabs } from "@nextui-org/react";
import { Image as ImageIcon, Users } from "lucide-react";
import { useSearchParam, useURLParamState } from "@web/hooks";
import { useTranslations } from "next-intl";
import ChatGroupMembersTab from "./members/ChatGroupMembersTab";
import ChatGroupAttachmentsTab from "./attachments/ChatGroupAttachmentsTab";

export interface ChatGroupSidebarProps {}

const ChatGroupSidebar = ({}: ChatGroupSidebarProps) => {
   const chatGroupId = useSearchParam(`c`);
   const [selectedTab, setSelectedTab] = useURLParamState(`tab`, `members`);
   const t = useTranslations(`Sidebar`);

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
                  className={`w-full`}
                  defaultSelectedKey={`members`}
                  classNames={{
                     tab: `my-2`,
                     base: `border-b-1 px-4 text-foreground w-full mx-auto py-2 border-default-200 rounded-medium`,
                     panel: `w-full pb-6 rounded-medium`,
                     tabList: `w-full`,
                  }}
                  selectedKey={selectedTab!}
                  // @ts-ignore
                  onSelectionChange={setSelectedTab}
                  variant={`underlined`}
                  size={`md`}
                  color={`primary`}
               >
                  <Tab
                     className={`w-full`}
                     title={
                        <div className={`flex items-center gap-2`}>
                           <Users size={12} />
                           <span>{t(`Members.title`)}</span>
                        </div>
                     }
                     key={`members`}
                  >
                     <ChatGroupMembersTab chatGroupId={chatGroupId} />
                  </Tab>
                  <Tab
                     aria-label={`sfdfsd`}
                     aria-labelledby={`sdfsdjfk`}
                     title={
                        <div className={`flex items-center gap-2`}>
                           <ImageIcon size={12} />
                           <span>{t(`SharedMedia.title`)}</span>
                        </div>
                     }
                     className={`mx-0`}
                     key={`media`}
                  >
                     <ChatGroupAttachmentsTab chatGroupId={chatGroupId} />
                  </Tab>
               </Tabs>
            </Fragment>
         )}
      </ScrollShadow>
   );
};

export default ChatGroupSidebar;
