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
   Tab,
   Tabs,
   useDisclosure,
} from "@nextui-org/react";
import React, { useState } from "react";
import { AddUserIcon, BellIcon } from "@icons";
import NotificationEntry from "@components/navbar/NotificationEntry";
import { useGetPaginatedNotificationsQuery } from "@web/api";

export interface NotificationsDropdownProps {}

const NotificationsDropdown = ({}: NotificationsDropdownProps) => {
   const { isOpen, onOpenChange, onClose } = useDisclosure({
      defaultOpen: false,
   });
   const [selectedTab, setSelectedTab] = useState("all");
   const {
      data: notifications,
      isLoading,
      error,
   } = useGetPaginatedNotificationsQuery(
      {
         pageSize: 10,
         pagingCursor: null!,
      },
      { enabled: selectedTab === "all" }
   );

   return (
      <Popover
         size={"md"}
         color={"default"}
         backdrop={"blur"}
         isOpen={isOpen}
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
            <h2 className={`text-medium text-foreground mx-4`}>
               Notifications
            </h2>
            <Tabs
               selectedKey={selectedTab}
               onSelectionChange={setSelectedTab as any}
               variant={"solid"}
               color={"primary"}
               radius={"full"}
               aria-label={`Notifications type`}
            >
               <Tab title={"All"} key={"all"}>
                  Tab 1
               </Tab>
               <Tab title={"Unread"} key={"unread"}>
                  Tab 2
               </Tab>
            </Tabs>
            <Listbox
               onAction={(key) => {
                  console.log(key);
                  setTimeout(onClose, 500);
               }}
               variant={"shadow"}
               className={`px-0`}
            >
               {notifications?.map((notification, i) => (
                  <NotificationEntry
                     message={notification.summary}
                     startContent={
                        <Avatar
                           src={notification.metadata.user_media.mediaUrl}
                           isBordered
                           size={"md"}
                           radius={"full"}
                           color={"primary"}
                        />
                     }
                     notificationTypeIcon={
                        <AddUserIcon className={`fill-foreground`} size={10} />
                     }
                     key={notification.id}
                  />
               ))}
               {Array.from({ length: 3 }).map((_, i) => (
                  <ListboxItem
                     className={`pr-4 `}
                     variant={"faded"}
                     color={"default"}
                     key={i}
                  >
                     <NotificationEntry
                        key={`${i}`}
                        message={`Message ${i}`}
                        notificationTypeIcon={
                           <div className={`w-3 h-3 rounded-full bg-danger`} />
                        }
                        startContent={<Avatar size={"md"} name={"N"} />}
                        date={new Date()}
                     />
                  </ListboxItem>
               ))}
            </Listbox>
         </PopoverContent>
      </Popover>
   );
};

export default NotificationsDropdown;
