"use client";
import TooltipWithPopoverActionButton from "@components/common/TooltipWithPopoverActionButton";
import React, { Fragment, useCallback } from "react";
import { PlusIcon } from "@icons";
import {
   Button,
   CircularProgress,
   Skeleton,
   useDisclosure,
   User,
} from "@nextui-org/react";
import { useAddChatGroupAdmin, useGetChatGroupDetailsQuery } from "@web/api";
import { useCurrentChatGroup, useGetNewAdminSuggestions } from "@hooks";
import SadFaceIcon from "@components/icons/SadFaceIcon";

export interface AddNewGroupAdminActionButtonProps {}

const AddNewGroupAdminActionButton =
   ({}: AddNewGroupAdminActionButtonProps) => {
      const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });

      return (
         <TooltipWithPopoverActionButton
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            popoverContent={<AddNewAdminPopover />}
            tooltipProps={{
               size: `sm`,
               shadow: `sm`,
               classNames: {
                  base: `text-xs `,
                  content: `text-[10px] h-5`,
               },
            }}
            popoverProps={{
               classNames: {
                  base: `pl-4 pr-0`,
               },
            }}
            chipProps={{
               classNames: {
                  content: `w-8 h-8 p-0 flex items-center justify-center`,
               },
               className: `w-8 h-8 p-0`,
            }}
            tooltipContent={"Add a new member"}
            icon={<PlusIcon className={"fill-foreground"} size={16} />}
         />
      );
   };

const AddNewAdminPopover = () => {
   const groupId = useCurrentChatGroup();
   const { isLoading } = useGetChatGroupDetailsQuery(groupId);
   const {
      mutateAsync: addNewAdmin,
      error: addMemberError,
      isLoading: addMemberLoading,
   } = useAddChatGroupAdmin();

   // Count only users that are friends but not group sidebar:
   const addAdminSuggestedUsers = useGetNewAdminSuggestions(groupId);

   const handleAddNewAdmin = useCallback(
      async (newAdminId: string) => {
         console.log("click");
         await addNewAdmin({
            chatGroupId: groupId,
            newAdminId,
         });
      },
      [addNewAdmin, groupId]
   );

   return (
      <div
         className={`flex max-h-[300px] overflow-y-scroll py-4 flex-col items-start gap-3`}
      >
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
         {addAdminSuggestedUsers?.length === 0 && !isLoading && (
            <div
               className={`text-default-300 my-2 gap-1 flex-col flex items-center w-full`}
            >
               <SadFaceIcon className={`fill-default-300`} size={20} />
               <span>You have no suggestions for new members </span>
            </div>
         )}
         {addAdminSuggestedUsers?.map((user, i) => (
            <div
               className={`flex w-full items-center justify-between gap-4`}
               key={user.id}
            >
               <User
                  avatarProps={{
                     color: "danger",
                     src: user.profilePicture.mediaUrl,
                     size: "sm",
                     name: user.username,
                  }}
                  name={user.username}
                  key={user.id}
               />
               <Button
                  radius={"full"}
                  onPress={() => handleAddNewAdmin(user.id)}
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

export default AddNewGroupAdminActionButton;
