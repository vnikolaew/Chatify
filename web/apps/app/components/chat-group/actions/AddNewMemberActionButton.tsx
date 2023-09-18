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
import { useAddChatGroupMember, useGetMyFriendsQuery } from "@web/api";
import TooltipWithPopoverActionButton from "@components/common/TooltipWithPopoverActionButton";
import { useCurrentChatGroup, useGetNewMemberSuggestions } from "@hooks";
import SadFaceIcon from "@components/icons/SadFaceIcon";

export interface AddNewMemberActionButtonProps {}

export const AddNewMemberActionButton = ({}: AddNewMemberActionButtonProps) => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });
   return (
      <TooltipWithPopoverActionButton
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={<AddNewMemberPopover />}
         tooltipContent={"Add a new member"}
         icon={<AddUserIcon fill={"white"} size={20} />}
      />
   );
};

const AddNewMemberPopover = () => {
   const { data: friends, isLoading, error } = useGetMyFriendsQuery();
   const groupId = useCurrentChatGroup();
   const {
      mutateAsync: addNewMember,
      error: addMemberError,
      isLoading: addMemberLoading,
   } = useAddChatGroupMember();

   // Count only users that are friends but not group sidebar:
   const addMemberSuggestedUsers = useGetNewMemberSuggestions(groupId);

   const handleAddNewMember = useCallback(
      async (friendId: string) => {
         console.log("click");
         await addNewMember({
            newMemberId: friendId,
            chatGroupId: groupId,
            membershipType: 0,
         });
      },
      [addNewMember, groupId]
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
         {addMemberSuggestedUsers?.map((friend, i) => (
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
