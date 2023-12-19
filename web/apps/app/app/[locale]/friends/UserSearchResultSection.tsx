"use client";
import React, { useMemo } from "react";
import { Avatar, AvatarProps, Badge, Button, Link, Spinner } from "@nextui-org/react";
import { FriendInvitationStatus, UserDetailsEntry, UserStatus } from "@openapi";
import moment from "moment/moment";
import { useAcceptFriendInviteMutation, useDeclineFriendInviteMutation, useSendFriendInviteMutation } from "@web/api";
import { useTranslations } from "next-intl";

export interface UserSearchResultSectionProps {
   user: UserDetailsEntry;

}

const UserSearchResultSection = ({ user }: UserSearchResultSectionProps) => {
   const t = useTranslations(`Friends`);
   const {
      mutateAsync: sendFriendInvite,
      isLoading: inviteLoading,
      error: inviteError,
   } = useSendFriendInviteMutation();

   const statusColor = useMemo<AvatarProps["color"]>(() => {
      if (!user) return "default";
      switch (user.user.status) {
         case UserStatus.AWAY:
            return "warning";
         case UserStatus.ONLINE:
            return "success";
         case UserStatus.OFFLINE:
            return "default";
      }
      return "default";
   }, [user]);

   const handleSendFriendInvite = async (userId: string) => {
      console.log(userId);
      await sendFriendInvite({ userId });
   };

   return (
      <div
         className={`w-full mt-12 border-b-1  border-b-default-100  rounded-md pb-4 px-6 transition-background duration-100 self-center flex items-center justify-between`}
      >
         <div className={` flex items-center gap-4`}>
            <Badge
               content={""}
               shape={"circle"}
               placement={"bottom-right"}
               variant={"solid"}
               color={statusColor}
               size={`sm`}
            >
               <Avatar
                  src={user.user?.profilePicture.mediaUrl}
                  color={"danger"}
                  size={`md`}
                  isBordered={true}
               />
            </Badge>
            <div
               className={`flex flex-col items-start justify-evenly gap-0 `}
            >
               <h2 className={`text-foreground text-small`}>
                  {user.user?.displayName}
               </h2>
               <h3 className={`text-default-400 text-xs`}>
                  {user.user?.userHandle}{" "}
                  {user.friendInvitation?.status ===
                     FriendInvitationStatus.PENDING &&
                     ` - Sent you an invite ${moment(
                        new Date(user.friendInvitation.createdAt),
                     ).fromNow()}`}
                  {user.friendsRelation &&
                     ` - Friends since ${moment(
                        new Date(user.friendsRelation.createdAt),
                     ).format("DD/MM/YYYY")}`}
               </h3>
            </div>
         </div>
         {user?.friendInvitation &&
            user.friendInvitation?.status ===
            FriendInvitationStatus.PENDING && (
               <FriendInvitationSection
                  userId={user?.user.id}
                  inviteId={user.friendInvitation?.id}
               />
            )}
         {user?.friendsRelation && (
            <Button
               as={Link}
               href={`/?c=${user?.friendsRelation.groupId}`}
               size={"sm"}
               className={`px-6`}
               variant={`flat`}
               color={`warning`}
            >
               Go to DM
            </Button>
         )}
         {!user?.friendsRelation && !user.friendInvitation && (
            <Button
               onPress={async (_) =>
                  await handleSendFriendInvite(user.user?.id)
               }
               isLoading={inviteLoading}
               spinner={<Spinner color={`white`} size={`sm`} />}
               isDisabled={inviteLoading}
               size={"md"}
               variant={`light`}
               color={`warning`}
            >
               {inviteLoading ? "Loading ..." : t("SendRequest")}
            </Button>
         )}
      </div>
   );
};

interface FriendInvitationSectionProps {
   inviteId: string;
   userId: string;
}

const FriendInvitationSection = ({
                                    inviteId,
                                    userId,
                                 }: FriendInvitationSectionProps) => {
   const {
      mutateAsync: acceptInvite,
      isLoading: acceptLoading,
      error: acceptError,
   } = useAcceptFriendInviteMutation();
   const {
      mutateAsync: declineInvite,
      isLoading,
      error,
   } = useDeclineFriendInviteMutation();

   return (
      <div className={`flex items-center gap-2`}>
         <Button
            onPress={async (_) => await acceptInvite({ inviteId, userId })}
            isLoading={acceptLoading}
            spinner={<Spinner size={`sm`} color={`white`} />}
            isDisabled={acceptLoading}
            className={`px-8 py-0`}
            size={`sm`}
            radius={"md"}
            variant={"light"}
            color={"success"}
         >
            Accept
         </Button>
         <Button
            onPress={async (_) => await declineInvite({ inviteId })}
            isLoading={isLoading}
            isDisabled={isLoading}
            radius={"md"}
            spinner={<Spinner size={`sm`} color={`white`} />}
            size={`sm`}
            className={`px-8 py-0`}
            variant={"light"}
            color={"danger"}
         >
            Decline
         </Button>
      </div>
   );
};

export default UserSearchResultSection;
