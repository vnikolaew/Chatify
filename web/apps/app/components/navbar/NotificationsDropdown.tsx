"use client";
import {
   Avatar,
   Badge,
   Button,
   Listbox,
   ListboxItem,
   Popover,
   PopoverContent,
   PopoverTrigger,
   Skeleton,
   Tab,
   Tabs,
   useDisclosure,
} from "@nextui-org/react";
import React, { useState } from "react";
import { AddUserIcon, BellIcon } from "@icons";
import NotificationEntry from "@components/navbar/NotificationEntry";
import {
   useGetPaginatedNotificationsQuery,
   useGetUnreadNotificationsQuery,
} from "@web/api";
import moment from "moment/moment";

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

   console.log(JSON.parse(notifications?.[0]?.metadata.user_media ?? null));

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
                  {isLoading ? (
                     <LoadingNotifications count={10} />
                  ) : (
                     <Listbox
                        onAction={(key) => {
                           console.log(key);
                           setTimeout(onClose, 500);
                        }}
                        variant={"shadow"}
                        className={`px-1`}
                     >
                        {notifications?.map((notification, i) => (
                           <ListboxItem
                              className={`px-0`}
                              variant={"faded"}
                              color={"default"}
                              key={i}
                           >
                              <NotificationEntry
                                 message={notification.summary}
                                 startContent={
                                    <Avatar
                                       src={
                                          JSON.parse(
                                             notification?.metadata?.user_media
                                          )?.MediaUrl
                                       }
                                       isBordered
                                       size={"sm"}
                                       radius={"full"}
                                       color={"primary"}
                                    />
                                 }
                                 notificationTypeIcon={
                                    <AddUserIcon
                                       className={`fill-foreground`}
                                       size={10}
                                    />
                                 }
                                 key={notification.id}
                              />
                           </ListboxItem>
                        ))}
                     </Listbox>
                  )}
               </Tab>
               <Tab title={"Unread"} key={NotificationTab.UNREAD}>
                  {unreadLoading ? (
                     <LoadingNotifications count={10} />
                  ) : (
                     <Listbox
                        onAction={(key) => {
                           console.log(key);
                           setTimeout(onClose, 500);
                        }}
                        variant={"shadow"}
                        className={`px-1`}
                     >
                        {unreadNotifications?.map((notification, i) => (
                           <ListboxItem
                              className={`px-0`}
                              variant={"faded"}
                              color={"default"}
                              key={i}
                           >
                              <NotificationEntry
                                 message={notification.summary}
                                 startContent={
                                    <Avatar
                                       src={
                                          JSON.parse(
                                             notification?.metadata?.user_media
                                          )?.MediaUrl
                                       }
                                       isBordered
                                       size={"sm"}
                                       radius={"full"}
                                       color={"primary"}
                                    />
                                 }
                                 notificationTypeIcon={
                                    <AddUserIcon
                                       className={`fill-foreground`}
                                       size={10}
                                    />
                                 }
                                 key={notification.id}
                              />
                           </ListboxItem>
                        ))}
                     </Listbox>
                  )}
               </Tab>
            </Tabs>
         </PopoverContent>
      </Popover>
   );
};

const LoadingNotifications = ({ count }: { count: number }) => {
   return (
      <div className={`flex flex-col gap-2`}>
         {Array.from({ length: count }).map((_, i) => (
            <div
               key={i}
               className={`flex px-2 items-center justify-between gap-4`}
            >
               <Skeleton className={`w-10 h-10 rounded-full`} />
               <div
                  className={`flex  h-full grow-[1] flex-col justify-center items-start gap-1`}
               >
                  <Skeleton className={`text-small rounded-full h-3 w-3/4`} />
                  <Skeleton
                     className={`text-xs h-2 w-1/3 rounded-full text-primary-400`}
                  />
               </div>
            </div>
         ))}
      </div>
   );
};
export default NotificationsDropdown;
