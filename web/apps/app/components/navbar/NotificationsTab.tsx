"use client";
import React from "react";
import { UserNotification } from "@openapi";
import { Avatar, Listbox, ListboxItem, Skeleton } from "@nextui-org/react";
import { AddUserIcon } from "@icons";
import { NotificationEntry } from "@components/navbar";

export interface NotificationsTabProps {
   notifications: UserNotification[];
   title?: string;
   key?: string;
   loading: boolean;
   noNotificationsMessage?: string;
   onAction?: (key: React.Key) => void;
}

export const NotificationsTab = ({
   notifications,
   onAction,
   key,
   title,
   loading,
   noNotificationsMessage,
}: NotificationsTabProps) => {
   return (
      <div>
         {loading ? (
            <LoadingNotifications count={4} />
         ) : !notifications.length ? (
            <div
               className={`text-default-400 text-small text-center`}
               key={`none`}
            >
               {noNotificationsMessage ?? "You have no new notifications."}
            </div>
         ) : (
            <Listbox
               onAction={(key) => {
                  console.log(key);
                  setTimeout(() => onAction?.(key), 500);
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
                        notification={notification}
                        message={notification.summary}
                        startContent={
                           <Avatar
                              src={notification?.metadata?.userMedia?.mediaUrl}
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
      </div>
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
