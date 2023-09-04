"use client";
import React, { Fragment, useCallback } from "react";
import {
   Button,
   CircularProgress,
   Skeleton,
   useDisclosure,
   User,
} from "@nextui-org/react";
import { AddUserIcon, PinIcon, PlusIcon } from "@icons";
import {
   useAddChatGroupMember,
   useGetChatGroupPinnedMessages,
   useGetMyFriendsQuery,
} from "@web/api";
import TooltipWithPopoverActionButton from "@components/TooltipWithPopoverActionButton";
import { useCurrentChatGroup } from "@hooks";
import SadFaceIcon from "@components/icons/SadFaceIcon";
import { useGetNewMemberSuggestions } from "@hooks";

export interface AddNewMemberActionButtonProps {}

export const PinnedMessagesActionButton = ({}: {}) => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });
   const groupId = useCurrentChatGroup();
   const {
      data: pinnedMessages,
      isLoading,
      error,
   } = useGetChatGroupPinnedMessages(
      { groupId },
      {
         enabled: false,
      }
   );

   return (
      <TooltipWithPopoverActionButton
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={
            true ? (
               <div className={`flex my-2 flex-col items-start gap-2`}>
                  {Array.from({ length: 3 }).map((_, i) => (
                     <div className={`flex items-center gap-1`} key={i}>
                        <Skeleton className={`w-5 h-5 rounded-full`} />
                        <div className={`flex flex-col gap-1`}>
                           <Skeleton className={`w-24 h-2 rounded-full`} />
                           <Skeleton className={`w-12 h-1 rounded-full`} />
                        </div>
                     </div>
                  ))}
               </div>
            ) : (
               null!
            )
         }
         tooltipContent={"Pinned messages"}
         icon={<PinIcon fill={"white"} size={24} />}
      />
   );
};

const AddNewMemberActionButton = ({}: AddNewMemberActionButtonProps) => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });
   return (
      <TooltipWithPopoverActionButton
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={<AddNewMemberPopover />}
         tooltipContent={"Add a new member"}
         icon={<AddUserIcon fill={"white"} size={24} />}
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

   // Count only users that are friends but not group members:
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
export default AddNewMemberActionButton;
