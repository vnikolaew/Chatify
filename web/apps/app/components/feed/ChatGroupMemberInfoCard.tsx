"use client";
import React, { Fragment, useMemo } from "react";
import { useGetChatGroupDetailsQuery, useGetUserDetailsQuery } from "@web/api";
import {
   Avatar,
   AvatarProps,
   Button,
   Card,
   CardBody,
   CardHeader,
   Divider,
   Skeleton,
   Spinner,
} from "@nextui-org/react";
import { UserStatus } from "@openapi";
import PlusIcon from "../icons/PlusIcon";
import { useGetUserMembershipDetailsQuery } from "@web/api";
import { useCurrentChatGroup } from "../../hooks/chat-groups/useCurrentChatGroup";
import moment from "moment";

export interface ChatGroupMemberInfoCardProps {
   userId: string;
}

const ChatGroupMemberInfoCard = ({ userId }: ChatGroupMemberInfoCardProps) => {
   const chatGroupId = useCurrentChatGroup();
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
      return <Spinner size={"md"} color={"danger"} />;
   }

   return (
      <Card className={`max-w-[500px]`}>
         <CardHeader className={`flex gap-3`}>
            <Avatar
               color={statusColor}
               size={"md"}
               isBordered
               radius={"full"}
               src={userDetails.user.profilePicture.mediaUrl}
            />
            <div className={`flex items-start justify-evenly h-full flex-col`}>
               <div className={`text-md`}>{userDetails.user.username}</div>
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
            <div className={`my-4 items-start flex-col flex gap-1`}>
               <span className={`text-foreground text-xs uppercase`}>
                  Roles
               </span>
            </div>
            {!userDetails.friendsRelation && (
               <Button
                  startContent={
                     <PlusIcon
                        size={16}
                        fill={"white"}
                        className={`fill-foreground`}
                     />
                  }
                  className={`mt-4 self-center w-3/5`}
                  variant={"shadow"}
                  color={"primary"}
                  size={"sm"}
               >
                  Add as friend
               </Button>
            )}
         </CardBody>
      </Card>
   );
};

export default ChatGroupMemberInfoCard;
