"use client";
import React from "react";
import { Badge, Button, ListboxItemProps } from "@nextui-org/react";
import moment from "moment";
import { UserNotificationType } from "@openapi/index";
import {
   useAcceptFriendInviteMutation,
   useDeclineFriendInviteMutation,
} from "@web/api";

export interface NotificationEntryProps extends ListboxItemProps {
   startContent?: React.ReactNode;
   message?: React.ReactNode;
   notificationTypeIcon?: React.ReactNode;
   id?: string;
   type: UserNotificationType;
   date?: Date;
}

const NotificationEntry = ({
   notificationTypeIcon,
   id,
   date,
   startContent,
   message,
   key,
   type,
   ...rest
}: NotificationEntryProps) => {
   const {
      mutateAsync: acceptFriendInvite,
      isLoading,
      error: acceptInviteError,
   } = useAcceptFriendInviteMutation();
   const {
      mutateAsync: declineFriendInvite,
      isLoading: declineLoading,
      error: declineInviteError,
   } = useDeclineFriendInviteMutation();

   return (
      <div className={`flex px-2 items-center justify-between gap-4`}>
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

         <div
            className={`flex h-full grow-[1] flex-col justify-center items-start gap-1`}
         >
            <p className={`text-small`}>{message}</p>
            <time className={`text-xs text-primary-400`}>
               {moment(date).fromNow()}
            </time>
            {type === UserNotificationType.INCOMING_FRIEND_INVITE && (
               <div className={`w-full flex items-center justify-evenly`}>
                  <Button
                     className={`px-8`}
                     isLoading={isLoading}
                     radius={"full"}
                     size={"sm"}
                     variant={"shadow"}
                     color={"success"}
                  >
                     Accept
                  </Button>
                  <Button
                     className={`px-8`}
                     onPress={async (_) => {
                        return await declineInviteError();
                     }}
                     isLoading={declineLoading}
                     radius={"full"}
                     size={"sm"}
                     variant={"shadow"}
                     color={"danger"}
                  >
                     Decline
                  </Button>
               </div>
            )}
         </div>
         <div className={`w-2 h-2 rounded-full bg-primary`} />
      </div>
   );
};

export default NotificationEntry;
