"use client";
import React from "react";
import { Badge, Button, Link, ListboxItemProps } from "@nextui-org/react";
import moment from "moment";
import { UserNotification, UserNotificationType } from "@openapi";
import {
   useAcceptFriendInviteMutation,
   useDeclineFriendInviteMutation,
} from "@web/api";
import { useQueryClient } from "@tanstack/react-query";

export interface NotificationEntryProps extends ListboxItemProps {
   startContent?: React.ReactNode;
   message?: React.ReactNode;
   notificationTypeIcon?: React.ReactNode;
   notification?: UserNotification;
}

export const NotificationEntry = ({
                                     notificationTypeIcon,
                                     id,
                                     startContent,
                                     notification,
                                     message,
                                     ...rest
                                  }: NotificationEntryProps) => {
   const client = useQueryClient();
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
            className={`flex h-full grow-[1] flex-col justify-center items-start gap-0`}
         >
            <p className={`text-small`}>{message}</p>
            <time className={`text-[.6rem] text-primary-400`}>
               {moment(new Date(notification.createdAt)).fromNow()}
            </time>
            {notification.type ===
               UserNotificationType.INCOMING_FRIEND_INVITE && (
                  <div className={`w-full mt-0 flex items-center justify-evenly`}>
                     <Button
                        className={`px-12 text-xs py-0`}
                        onPress={async () => {
                           await acceptFriendInvite(
                              {
                                 inviteId: (notification as any).inviteId,
                                 userId: notification.userId,
                              },
                              {
                                 onSuccess: async (_, { inviteId }) => {
                                    client.setQueriesData<UserNotification[]>(
                                       {
                                          exact: false,
                                          queryKey: [`notifications`],
                                       },
                                       (old) =>
                                          old.filter(
                                             (n) =>
                                                (n as any).inviteId !== inviteId,
                                          ),
                                    );
                                 },
                              },
                           );
                        }}
                        isLoading={isLoading}
                        radius={"full"}
                        size={"sm"}
                        variant={"light"}
                        color={"success"}
                     >
                        Accept
                     </Button>
                     <Button
                        className={`px-8 py-0 text-xs`}
                        onPress={async (_) => {
                           await declineFriendInvite(
                              {
                                 inviteId: (notification as any).inviteId,
                              },
                              {
                                 onSuccess: async (_, { inviteId }) => {
                                    client.setQueriesData<UserNotification[]>(
                                       {
                                          exact: false,
                                          queryKey: [`notifications`],
                                       },
                                       (old) =>
                                          old.filter(
                                             (n) =>
                                                (n as any).inviteId !== inviteId,
                                          ),
                                    );
                                 },
                              },
                           );
                        }}
                        isLoading={declineLoading}
                        radius={"full"}
                        size={"sm"}
                        variant={"light"}
                        color={"danger"}
                     >
                        Decline
                     </Button>
                  </div>
               )}
            {notification.type ===
               UserNotificationType.ACCEPTED_FRIEND_INVITE && (
                  <div className={`w-full mt-1 flex items-center justify-center`}>
                     <Button
                        href={`?c=${(notification as any).chatGroupId}`}
                        as={Link}
                        size={"sm"}
                        variant={"shadow"}
                        color={"primary"}
                     >
                        Go to new chat group
                     </Button>
                  </div>
               )}
         </div>
         {!notification.read && (
            <div className={`w-2 h-2 rounded-full bg-primary`} />
         )}
      </div>
   );
};
