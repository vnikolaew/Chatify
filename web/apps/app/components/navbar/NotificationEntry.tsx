"use client";
import React from "react";
import { Badge, ListboxItem, ListboxItemProps } from "@nextui-org/react";
import moment from "moment";

export interface NotificationEntryProps extends ListboxItemProps {
   startContent?: React.ReactNode;
   message?: React.ReactNode;
   notificationTypeIcon?: React.ReactNode;
   id?: string;
   date?: Date;
}

const NotificationEntry = ({
   notificationTypeIcon,
   id,
   date,
   startContent,
   message,
   key,
   ...rest
}: NotificationEntryProps) => {
   return (
      <div className={`flex items-center gap-4`}>
         <Badge
            placement={"bottom-right"}
            classNames={{
               badge: "p-0 border-none",
            }}
            content={notificationTypeIcon}
            size={"sm"}
            shape={"circle"}
         >
            {startContent}
         </Badge>

         <div className={`flex flex-col justify-evenly items-start gap-2`}>
            <p className={`text-small`}>{message}</p>
            <time className={`text-xs text-primary-400`}>
               {moment(date).fromNow()}
            </time>
         </div>
         <div className={`w-2 h-2 ml-6 rounded-full bg-primary`} />
      </div>
   );
};

export default NotificationEntry;
