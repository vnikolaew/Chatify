"use client";
import React, { Fragment, useCallback } from "react";
import {
   Button,
   CircularProgress,
   Skeleton,
   useDisclosure,
   User,
} from "@nextui-org/react";
import { AddUserIcon, PlusIcon } from "@icons";
import { useAddChatGroupMember, useGetChatGroupDetailsQuery, useGetMyFriendsQuery } from "@web/api";
import TooltipWithPopoverActionButton from "@components/common/TooltipWithPopoverActionButton";
import {
   useCurrentChatGroup,
   useGetNewMemberSuggestions,
   useIsChatGroupPrivate,
   useIsChatGroupPrivateById,
} from "@hooks";
import SadFaceIcon from "@components/icons/SadFaceIcon";
import { useTranslations } from "next-intl";

export interface AddNewMemberActionButtonProps {
}

export const AddNewMemberActionButton = ({}: AddNewMemberActionButtonProps) => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });
   const t = useTranslations("MainArea.TopBar.Popups");

   return (
      <TooltipWithPopoverActionButton
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={<AddNewMemberPopover />}
         tooltipProps={{ placement: isOpen ? `top` : `bottom` }}
         popoverProps={{ placement: `bottom` }}
         tooltipContent={t(`AddNewMember`)}
         icon={<AddUserIcon fill={"white"} size={20} />}
      />
   );
};

const AddNewMemberPopover = () => {
   const groupId = useCurrentChatGroup();
   const {
      mutateAsync: addNewMember,
      error: addMemberError,
      isLoading: addMemberLoading,
   } = useAddChatGroupMember();

   // Count only users that are friends but not group sidebar:
   const { addMemberSuggestedUsers, isLoading } = useGetNewMemberSuggestions(groupId);
   const isGroupPrivate = useIsChatGroupPrivateById(groupId);

   const handleAddNewMember = useCallback(
      async (friendId: string) => {
         console.log("click");
         await addNewMember({
            newMemberId: friendId,
            chatGroupId: groupId,
            membershipType: 0,
         });
      },
      [addNewMember, groupId],
   );

   return (
      <div className={`flex py-4 flex-col items-start gap-3`}>
         {isLoading && (
            <Fragment>
               {Array.from({ length: 5 }).map((_, i) => (
                  <div
                     className={`flex w-[240px] gap-3 items-center justify-between`}
                     key={i}
                  >
                     <div className={`flex items-center gap-2`}>
                        <Skeleton className={`rounded-full h-8 w-8`} />
                        <Skeleton
                           key={i}
                           className={`rounded-full h-3 w-[120px]`}
                        />
                     </div>
                     <Skeleton
                        className={`rounded-full mr-2 justify-self-end h-4 w-4`}
                     />
                  </div>
               ))}
            </Fragment>
         )}
         {addMemberSuggestedUsers?.length === 0 && !isLoading && (
            <div
               className={`text-default-300 my-2 gap-1 flex-col flex items-center w-full`}
            >
               <SadFaceIcon className={`fill-default-300`} size={20} />
               <span className={`text-xs`}>
                  You have no suggestions for new members{" "}
               </span>
            </div>
         )}
         {isGroupPrivate ? (
            <div
               className={`flex w-full items-center text-default-300 justify-between gap-4`}
            >Cannot add new members to a private group.</div>
         ) : addMemberSuggestedUsers?.map((friend, i) => (
            <div
               className={`flex w-full items-center justify-between gap-4`}
               key={friend.id}
            >
               <User
                  avatarProps={{
                     color: "danger",
                     src: friend.profilePicture.mediaUrl,
                     size: "sm",
                     name: friend.username,
                  }}
                  name={friend.username}
                  key={friend.id}
               />
               <Button
                  radius={"full"}
                  onPress={() => handleAddNewMember(friend.id)}
                  size={"sm"}
                  className={`bg-transparent hover:bg-default-300 duration-300 transition-background`}
                  startContent={
                     addMemberLoading ? (
                        <CircularProgress
                           classNames={{
                              base: `w-3 h-3`,
                              svg: `w-5 h-5`,
                              track: `w-3 h-3`,
                           }}
                           color={"danger"}
                        />
                     ) : (
                        <PlusIcon className={`fill-default-500`} size={12} />
                     )
                  }
                  isIconOnly
               />
            </div>
         ))}
      </div>
   );
};
