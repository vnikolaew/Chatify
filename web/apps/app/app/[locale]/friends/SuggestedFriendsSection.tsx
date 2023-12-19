"use client";
import React, { useCallback } from "react";
import { FriendInvitationStatus, User, UserStatus } from "@openapi";
import { Avatar, Badge, BadgeProps, Button, Chip, Skeleton, Spinner } from "@nextui-org/react";
import { useGetFriendSuggestions, useGetUserDetailsQuery, useSendFriendInviteMutation } from "@web/api";
import { useTranslations } from "next-intl";
import { AddUserIcon } from "@icons";

export interface SuggestedFriendsSectionProps {
}

const SuggestedFriendsSection = ({}: SuggestedFriendsSectionProps) => {
   const { data: suggestedFriends, isLoading, isFetching } = useGetFriendSuggestions();

   return (
      <section className={`mt-4`}>
         <h2 className={`text-default-400 text-large`}>Some user suggestions:</h2>
         <div className={`flex flex-col ${isLoading && isFetching ? `gap-2` : `gap-4`} mt-4`}>
            {(isLoading && isFetching || true) && Array.from({ length: 3 }).map((_, i) => (
               <div
                  key={i}
                  className={`w-11/12 ml-4 mt-2 flex items-center gap-2`}
               >
                  <Skeleton className={`rounded-full h-12 w-12 `} />
                  <div className={`flex flex-col gap-2 flex-1`}>
                     <Skeleton className={`rounded-full h-4 w-2/5`} />
                     <Skeleton className={`rounded-full h-3 w-1/5`} />
                  </div>
                  <Skeleton className={`rounded-full h-7 w-20`} />
               </div>
            ))}
            {suggestedFriends?.map((user, i) => (
               <SuggestedFriend key={user.id} user={user} />
            ))}
         </div>
      </section>
   );
};

export interface SuggestedFriendProps {
   user: User;
}

const SuggestedFriend = ({ user }: SuggestedFriendProps) => {
   const t = useTranslations(`Friends`);
   const {
      mutateAsync: sendFriendInvite,
      isLoading: inviteLoading,
      error: inviteError,
   } = useSendFriendInviteMutation();
   const { data: userDetails } = useGetUserDetailsQuery(user.id, { networkMode: "offlineFirst" });

   const statusColor = useCallback<(user: User) => BadgeProps["color"]>((user: User) => {
      if (!user) return "default";
      switch (user.status) {
         case UserStatus.AWAY:
            return "warning";
         case UserStatus.ONLINE:
            return "success";
         case UserStatus.OFFLINE:
            return "default";
      }
      return "default";
   }, []);
   console.log({ userDetails });

   const handleSendFriendInvite = async (userId: string) => {
      console.log(userId);
      await sendFriendInvite({ userId });
   };

   return (
      <div key={user.id} className={`flex w-full items-center justify-between gap-4`}>
         <div className={`flex items-center justify-between gap-4`}>
            <Badge
               content={""}
               shape={"circle"}
               placement={"bottom-right"}
               variant={"solid"}
               color={statusColor(user)}
               size={`sm`}
            >
               <Avatar
                  src={user?.profilePicture.mediaUrl}
                  color={"danger"}
                  size={`md`}
                  isBordered={true}
               />
            </Badge>
            <div
               className={`flex flex-col items-start justify-evenly gap-0 `}
            >
               <h2 className={`text-foreground text-small`}>
                  {user?.displayName}
               </h2>
               <h3 className={`text-default-400 text-xs`}>
                  {user?.userHandle}
               </h3>
            </div>
         </div>
         {userDetails?.friendInvitation?.status === FriendInvitationStatus.PENDING ? (
            <Chip className={`!px-3`} radius={`sm`} size={`sm`} variant={`shadow`} color={`primary`}>Friend invite is pending.</Chip>
         ) : (
            <Button
               onPress={async (_) => await handleSendFriendInvite(user?.id)}
               isLoading={inviteLoading}
               startContent={<AddUserIcon size={12} />}
               spinner={<Spinner color={`white`} size={`sm`} />}
               isDisabled={inviteLoading}
               radius={`sm`}
               size={"sm"}
               variant={`flat`}
               color={`primary`}
            >
               {inviteLoading ? t(`SendingRequest`) : t("SendRequest")}
            </Button>
         )}
      </div>
   );
};

export default SuggestedFriendsSection;
