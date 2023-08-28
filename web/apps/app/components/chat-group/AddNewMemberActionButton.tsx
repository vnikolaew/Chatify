"use client";
import React, { Fragment } from "react";
import { Button, Skeleton, useDisclosure, User } from "@nextui-org/react";
import { AddUserIcon, PinIcon, PlusIcon } from "@icons";
import { useGetMyFriendsQuery } from "@web/api";
import TooltipWithPopoverActionButton from "@components/TooltipWithPopoverActionButton";

export interface AddNewMemberActionButtonProps {}

export const PinnedMessagesActionButton = ({}: {}) => {
   const { isOpen, onOpenChange } = useDisclosure({ defaultOpen: false });

   return (
      <TooltipWithPopoverActionButton
         isOpen={isOpen}
         onOpenChange={onOpenChange}
         popoverContent={"Content"}
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
   console.log(friends);

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
         {friends?.map((friend, i) => (
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
                  size={"sm"}
                  className={`bg-transparent hover:bg-default-300 duration-300 transition-background`}
                  startContent={
                     <PlusIcon className={`fill-default-500`} size={12} />
                  }
                  isIconOnly
               />
            </div>
         ))}
      </div>
   );
};
export default AddNewMemberActionButton;
