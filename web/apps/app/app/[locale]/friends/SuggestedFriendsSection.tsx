"use client";
import React, { useCallback, useRef } from "react";
import { FriendInvitationStatus, User, UserStatus } from "@openapi";
import { Avatar, AvatarProps, Badge, BadgeProps, Button, Chip, Skeleton, Spinner } from "@nextui-org/react";
import { useGetFriendSuggestions, useGetUserDetailsQuery, useSendFriendInviteMutation } from "@web/api";
import { useTranslations } from "next-intl";
import { AddUserIcon, Divider } from "@web/components";

export interface SuggestedFriendsSectionProps {
}

const SuggestedFriendsSection = ({}: SuggestedFriendsSectionProps) => {
   const { data: suggestedFriends, isLoading, isFetching } = useGetFriendSuggestions();

   return (
      <section className={`mt-4`}>
         <h2 className={`text-default-400 text-large`}>Users you might know:</h2>
         <div className={`flex flex-col ${isLoading && isFetching ? `gap-2` : `gap-4`} mt-4`}>
            {(isLoading && isFetching) && Array.from({ length: 3 }).map((_, i) => (
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
               <div key={user.id}>
                  <SuggestedFriend index={i} key={user.id} user={user} />
                  <Divider className={`w-full mt-3 text-default-300`} orientation={`horizontal`} />
               </div>
            ))}
         </div>
      </section>
   );
};

export interface SuggestedFriendProps {
   user: User;
   index: number;
}

const AVATAR_COLORS: AvatarProps["color"][] = [`primary`, `secondary`, `warning`, `danger`];

const SuggestedFriend = ({ user, index }: SuggestedFriendProps) => {
   const t = useTranslations(`Friends`);
   const {
      mutateAsync: sendFriendInvite,
      isLoading: inviteLoading,
      error: inviteError,
   } = useSendFriendInviteMutation();
   const { data: userDetails } = useGetUserDetailsQuery(user.id, { networkMode: "offlineFirst" });

   const handleSendFriendInvite = async (userId: string) => {
      console.log(userId);
      await sendFriendInvite({ userId });
   };

   return (
      <div key={user.id} className={`flex w-full items-center justify-between gap-4`}>
         <div className={`flex items-center justify-between gap-4`}>

            <Avatar
               src={user?.profilePicture.mediaUrl}
               color={AVATAR_COLORS[index % AVATAR_COLORS.length]}
               size={`md`}
               isBordered={true}
            />
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
            <Chip className={`!px-3`} radius={`sm`} size={`sm`} variant={`shadow`} color={`primary`}>Friend invite is
               pending.</Chip>
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
