"use client";
import {
   Badge,
   Button,
   Popover,
   PopoverContent,
   PopoverTrigger,
   Tab,
   Tabs,
   useDisclosure,
} from "@nextui-org/react";
import React, { useState } from "react";
import { BellIcon } from "@icons";
import {
   useGetPaginatedNotificationsQuery,
   useGetUnreadNotificationsQuery,
} from "@web/api";
import NotificationsTab from "@components/navbar/NotificationsTab";

export interface NotificationsDropdownProps {}

export enum NotificationTab {
   ALL = "all",
   UNREAD = "unread",
}

const NotificationsDropdown = ({}: NotificationsDropdownProps) => {
   const { isOpen, onOpenChange, onClose } = useDisclosure({
      defaultOpen: false,
   });
   const [selectedTab, setSelectedTab] = useState<NotificationTab>(
      NotificationTab.ALL
   );
   const {
      data: notifications,
      isLoading,
      error,
   } = useGetPaginatedNotificationsQuery(
      {
         pageSize: 10,
         pagingCursor: undefined!,
      },
      { enabled: selectedTab === NotificationTab.ALL && isOpen }
   );
   const {
      data: unreadNotifications,
      isLoading: unreadLoading,
      error: unreadNotificationsError,
   } = useGetUnreadNotificationsQuery({
      enabled: selectedTab === NotificationTab.UNREAD,
   });

   console.log(notifications);

   return (
      <Popover
         size={"md"}
         color={"default"}
         showArrow
         offset={10}
         // backdrop={"blur"}
         isOpen={true}
         onOpenChange={onOpenChange}
         placement={"bottom"}
      >
         <PopoverTrigger onClick={console.log}>
            <Button
               className={`bg-transparent overflow-visible border-none hover:bg-default-200`}
               variant={"faded"}
               size={"md"}
               radius={"full"}
               color={"default"}
               isIconOnly
            >
               <Badge
                  content={5}
                  size={"sm"}
                  classNames={{
                     badge: "text-xs p-2 flex items-center justify-center",
                     base: "text-[.5rem]",
                  }}
                  variant={"solid"}
                  color={"danger"}
                  placement={"top-right"}
               >
                  <BellIcon className={`fill-foreground`} size={24} />
               </Badge>
            </Button>
         </PopoverTrigger>
         <PopoverContent className={`p-4 flex items-start flex-col gap-2`}>
            <h2 className={`text-medium text-foreground`}>Notifications</h2>
            <Tabs
               selectedKey={selectedTab}
               onSelectionChange={setSelectedTab as any}
               variant={"light"}
               classNames={{
                  // tabContent: "w-[500px]",
                  panel: "w-[400px]",
               }}
               color={"primary"}
               radius={"full"}
               aria-label={`Notifications type`}
            >
               <Tab title={"All"} key={NotificationTab.ALL}>
                  <NotificationsTab
                     notifications={notifications}
                     loading={isLoading}
                  />
               </Tab>
               <Tab title={"Unread"} key={NotificationTab.UNREAD}>
                  <NotificationsTab
                     noNotificationsMessage={"No new unread notifications."}
                     notifications={unreadNotifications}
                     loading={unreadLoading}
                  />
               </Tab>
            </Tabs>
         </PopoverContent>
      </Popover>
   );
};

export default NotificationsDropdown;
