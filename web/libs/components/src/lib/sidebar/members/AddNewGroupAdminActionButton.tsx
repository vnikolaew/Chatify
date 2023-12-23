"use client";
import React, { Fragment, useCallback } from "react";
import {
   Button,
   CircularProgress,
   Skeleton,
   useDisclosure,
   User,
} from "@nextui-org/react";
import { useAddChatGroupAdmin, useGetChatGroupDetailsQuery } from "@web/api";
import { useCurrentChatGroup, useGetNewAdminSuggestions } from "@web/hooks";
import { TooltipWithPopoverActionButton } from "../../common";
import { PlusIcon, SadFaceIcon } from "@web/components";

export interface AddNewGroupAdminActionButtonProps {
}

const AddNewGroupAdminActionButton =
   ({}: AddNewGroupAdminActionButtonProps) => {
      const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });

      return (
         <TooltipWithPopoverActionButton
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            popoverContent={<AddNewAdminPopover onOpenChange={onOpenChange} />}
            tooltipProps={{
               size: `sm`,
               shadow: `sm`,
               placement: isOpen ? `top` : `bottom`,
               showArrow: true,
               classNames: {
                  base: `text-xs `,
                  content: `text-[10px] h-5`,
               },
            }}
            popoverProps={{
               classNames: {
                  base: `pl-4 pr-0`,
               },
               showArrow: true,
               placement: `bottom`,
            }}
            chipProps={{
               classNames: {
                  content: `w-8 h-8 p-0 flex items-center justify-center`,
               },
               className: `w-8 h-8 p-0`,
            }}
            tooltipContent={"Add new admin"}
            icon={<PlusIcon className={"fill-foreground"} size={16} />}
         />
      );
   };

interface AddNewAdminPopoverProps {
   onOpenChange: () => void;
}

const AddNewAdminPopover = ({ onOpenChange }: AddNewAdminPopoverProps) => {
   const groupId = useCurrentChatGroup()!;
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
         onOpenChange();
         await addNewAdmin({
            chatGroupId: groupId,
            newAdminId,
         });
      },
      [addNewAdmin, groupId, onOpenChange],
   );

   return (
      <div
         className={`flex max-h-[300px] px-2 py-2 my-2 flex-col items-start gap-3`}
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
               <span className={`text-xs`}>You have no suggestions for new admins </span>
            </div>
         )}
         {addAdminSuggestedUsers?.map((user, i) => (
            <div
               className={`flex w-full items-center justify-between gap-5`}
               key={user.id}
            >
               <User
                  avatarProps={{
                     color: "danger",
                     src: user.profilePicture!.mediaUrl!,
                     size: "sm",
                     classNames: { img: `w-5 h-5`, icon: `w-5 h-5`, base: `w-5 h-5` },
                     name: user.username!,
                  }}
                  classNames={{ name: `text-xs font-normal` }}
                  name={user.username}
                  key={user.id}
               />
               <Button
                  radius={"full"}
                  onPress={() => handleAddNewAdmin(user.id!)}
                  size={"sm"}
                  className={`bg-transparent !w-fit !m-0 !min-w-5 !max-w-5 !h-5 p-0 hover:bg-default-300 duration-300 transition-background`}
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
                        <PlusIcon className={`fill-default-500`} size={10} />
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
