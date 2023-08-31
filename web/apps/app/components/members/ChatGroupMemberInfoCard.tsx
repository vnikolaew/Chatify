"use client";
import React, { Fragment, useMemo } from "react";
import {
   useGetChatGroupDetailsQuery,
   useGetUserDetailsQuery,
   useGetUserMembershipDetailsQuery,
   useSendFriendInviteMutation,
} from "@web/api";
import {
   Avatar,
   AvatarProps,
   Button,
   Card,
   CardBody,
   CardHeader,
   Skeleton,
   Spinner,
} from "@nextui-org/react";
import { PlusIcon } from "@icons";
import { useCurrentChatGroup, useCurrentUserId } from "@hooks";
import moment from "moment";
import { FriendInvitationStatus, UserStatus } from "@openapi";

export interface ChatGroupMemberInfoCardProps {
   userId: string;
}

export const ChatGroupMemberInfoCard = ({
   userId,
}: ChatGroupMemberInfoCardProps) => {
   const chatGroupId = useCurrentChatGroup();
   const meId = useCurrentUserId();
   const {
      data: membership,
      error: memberError,
      isLoading: memberLoading,
   } = useGetUserMembershipDetailsQuery(
      { chatGroupId, userId },
      {
         enabled: !!chatGroupId,
         networkMode: "offlineFirst",
         staleTime: 60 * 30 * 1000,
      }
   );
   const {
      mutateAsync: sendFriendInvite,
      error: friendInviteError,
      isLoading: friendInviteLoading,
   } = useSendFriendInviteMutation();

   const { data: chatGroupDetails } = useGetChatGroupDetailsQuery(chatGroupId, {
      networkMode: "offlineFirst",
   });

   const {
      data: userDetails,
      error,
      isLoading,
      isFetching,
   } = useGetUserDetailsQuery(userId, {
      enabled: !!userId,
      networkMode: "offlineFirst",
   });

   const statusColor = useMemo<AvatarProps["color"]>(() => {
      if (!userDetails) return "default";
      switch (userDetails.user.status) {
         case UserStatus.AWAY:
            return "warning";
         case UserStatus.ONLINE:
            return "success";
         case UserStatus.OFFLINE:
            return "default";
      }
      return "default";
   }, [userDetails]);

   console.log(membership);
   if (isLoading && isFetching) {
      return <Spinner size={"sm"} color={"danger"} />;
   }

   const handleSendFriendInvite = async () => {
      await sendFriendInvite(
         { userId },
         {
            onSuccess: (data) => console.log(data),
         }
      );
   };

   return (
      <Card
         classNames={{ base: "border-none  border-0" }}
         className={`max-w-[500px] bg-transparent shadow-none border-none`}
      >
         <CardHeader className={`flex gap-3`}>
            <Avatar
               color={statusColor}
               size={"md"}
               isBordered
               radius={"full"}
               src={userDetails.user.profilePicture.mediaUrl}
            />
            <div className={`flex items-start justify-evenly h-full flex-col`}>
               <div className={`text-md`}>
                  {userDetails.user.username}{" "}
                  {meId === userDetails.user.id && (
                     <span className={`text-default-300`}>(you)</span>
                  )}
               </div>
               <div
                  className={`text-xs flex items-center gap-1 text-default-500`}
               >
                  <Avatar
                     name={""}
                     icon={null!}
                     showFallback
                     className={`w-2 h-2`}
                     radius={"full"}
                     color={statusColor}
                  />
                  {userDetails.user.status}
               </div>
            </div>
         </CardHeader>
         <CardBody className={`flex justify-center`}>
            <div className={`mb-4 items-start flex-col flex gap-1`}>
               <span className={`text-foreground text-xs uppercase`}>
                  Member since
               </span>
               <div className={`flex items-center gap-4`}>
                  <div className={`flex items-center gap-1`}>
                     <Avatar
                        src={`/favicon.ico`}
                        radius={"full"}
                        className={`w-4 h-4`}
                     />
                     <time className={`text-xs text-default-400`}>
                        {moment(new Date(userDetails.user.createdAt)).format(
                           "MMM DD, YYYY"
                        )}
                     </time>
                  </div>
                  <div
                     className={`bg-default-200 rounded-full w-[4px] h-[4px]`}
                  />
                  <div className={`flex items-center gap-1`}>
                     {memberLoading ? (
                        <Fragment>
                           <Skeleton className={`w-3 h-3 rounded-full`} />
                           <Skeleton className={`w-8 h-3 rounded-full`} />
                        </Fragment>
                     ) : (
                        <Fragment>
                           <Avatar
                              src={chatGroupDetails.chatGroup.picture.mediaUrl}
                              radius={"full"}
                              className={`w-4 h-4`}
                           />
                           <time className={`text-xs text-default-400`}>
                              {moment(new Date(membership?.createdAt)).format(
                                 "MMM DD, YYYY"
                              )}
                           </time>
                        </Fragment>
                     )}
                  </div>
               </div>
            </div>
            <div className={`my-0 items-start flex-col flex gap-1`}>
               <span className={`text-foreground text-xs uppercase`}>
                  Roles
               </span>
               <span className={`text-default-300 text-xs`}>
                  {chatGroupDetails.chatGroup.adminIds.some(
                     (_) => _ === userDetails.user.id
                  )
                     ? " - Admin"
                     : "None."}
               </span>
            </div>
            {userDetails.friendInvitation?.status ===
               FriendInvitationStatus.PENDING && (
               <div className={`mt-4`}>
                  {" "}
                  Friend invite{" "}
                  {userDetails.friendInvitation.inviterId ===
                  userDetails.user.id
                     ? "from"
                     : "to"}{" "}
                  {userDetails.user.username} is pending
               </div>
            )}
            {userDetails.friendInvitation?.status ===
               FriendInvitationStatus.ACCEPTED &&
               !!userDetails.friendsRelation && (
                  <div className={`text-default-300 mt-4`}>
                     Friends since{" "}
                     {moment(
                        new Date(userDetails.friendsRelation.createdAt)
                     ).format("MMM DD, YYYY")}
                  </div>
               )}
            {!userDetails.friendsRelation && !userDetails.friendInvitation && (
               <Button
                  isLoading={friendInviteLoading}
                  startContent={
                     <PlusIcon
                        size={16}
                        fill={"white"}
                        className={`fill-foreground`}
                     />
                  }
                  spinner={<Spinner color={"white"} size={"sm"} />}
                  onPress={(_) => handleSendFriendInvite()}
                  className={`mt-4 self-center w-3/5`}
                  variant={"shadow"}
                  color={"primary"}
                  size={"sm"}
               >
                  {friendInviteLoading ? "" : "Add as friend"}
               </Button>
            )}
         </CardBody>
      </Card>
   );
};
