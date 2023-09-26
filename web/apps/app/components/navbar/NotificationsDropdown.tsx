"use client";
import {
   Badge,
   Button,
   Spinner,
   Tab,
   Tabs,
   Tooltip,
   useDisclosure,
} from "@nextui-org/react";
import React, { useState } from "react";
import { BellIcon } from "@icons";
import {
   sleep,
   useGetPaginatedNotificationsQuery,
   useGetUnreadNotificationsQuery,
   useMarkNotificationsAsReadMutation,
} from "@web/api";
import { NotificationsTab } from "@components/navbar";
import CheckIcon from "@components/icons/CheckIcon";
import TooltipWithPopoverActionButton from "@components/common/TooltipWithPopoverActionButton";
import { useTranslations } from "next-intl";

export interface NotificationsDropdownProps {}

export enum NotificationTab {
   ALL = "all",
   UNREAD = "unread",
}

export const NotificationsDropdown = ({}: NotificationsDropdownProps) => {
   const { isOpen, onOpenChange } = useDisclosure({
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
      enabled: selectedTab === NotificationTab.UNREAD && isOpen,
   });
   const {
      mutateAsync: markAsRead,
      isLoading: markLoading,
      error: markError,
   } = useMarkNotificationsAsReadMutation();

   const handleMarkNotificationsAsRead = async () => {
      // await markAsRead({}, {});
      await sleep(2000);
   };
   const t = useTranslations('MainNavbar.Popups');

   console.log(notifications?.pages?.[0]);
   return (
      <div className={`flex items-center gap-4`}>
         <TooltipWithPopoverActionButton
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            popoverContent={
               <div className={`flex flex-col items-start`}>
                  <div
                     className={`w-full p-2 flex items-center justify-between`}
                  >
                     <h2 className={`text-medium text-foreground`}>
                        Notifications
                     </h2>
                     <Tooltip
                        color={"default"}
                        classNames={{
                           base: "bg-default-200",
                        }}
                        size={"sm"}
                        placement={"top"}
                        showArrow
                        content={
                           <span className={`bg-default-200 px-1 py-0 text-xs`}>
                              Mark all as read
                           </span>
                        }
                        offset={2}
                        radius={"sm"}
                     >
                        <Button
                           className={`bg-transparent self-end mr-4 overflow-visible border-none hover:bg-default-200`}
                           onPress={handleMarkNotificationsAsRead}
                           isLoading={markLoading}
                           isDisabled={
                              !notifications?.pages.flatMap((_) => _.items)
                                 ?.length
                           }
                           variant={"faded"}
                           spinner={<Spinner size={"sm"} color={"danger"} />}
                           size={"sm"}
                           radius={"full"}
                           color={"default"}
                           isIconOnly
                        >
                           <CheckIcon className={`fill-foreground`} size={18} />
                        </Button>
                     </Tooltip>
                  </div>
                  <Tabs
                     selectedKey={selectedTab}
                     onSelectionChange={setSelectedTab as any}
                     variant={"light"}
                     classNames={{
                        // tabContent: "w-[500px]",
                        panel: "w-[400px] mr-auto mb-4 self-start",
                     }}
                     color={"primary"}
                     radius={"full"}
                     aria-label={`Notifications type`}
                  >
                     <Tab title={"All"} key={NotificationTab.ALL}>
                        <NotificationsTab
                           notifications={
                              notifications?.pages?.[0]?.items ?? []
                           }
                           loading={isLoading}
                        />
                     </Tab>
                     <Tab title={"Unread"} key={NotificationTab.UNREAD}>
                        <NotificationsTab
                           noNotificationsMessage={
                              "No new unread notifications."
                           }
                           notifications={unreadNotifications}
                           loading={unreadLoading}
                        />
                     </Tab>
                  </Tabs>
               </div>
            }
            tooltipContent={t(`Notifications`)}
            icon={
               notifications?.length > 0 ? (
                  <Badge
                     content={1}
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
               ) : (
                  <BellIcon className={`fill-foreground`} size={24} />
               )
            }
         />
      </div>
   );
};
