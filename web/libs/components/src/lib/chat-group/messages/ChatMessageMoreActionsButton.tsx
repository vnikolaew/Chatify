"use client";
import { TooltipWithPopoverActionButton } from "libs/components/src/lib/common";
import React, { useState } from "react";
import { EditIcon } from "lucide-react";
import { useDisclosure } from "@nextui-org/react";
import VerticalDotsIcon from "../../icons/VerticalDotsIcon";
import { ThrashIcon } from "@web/components";

export interface ChatMessageMoreActionsButtonProps {
   onOpen?: () => void;
}

const ChatMessageMoreActionsButton = ({}: ChatMessageMoreActionsButtonProps) => {
   const {
      isOpen: isMoreActionsDropdownMenuOpen,
      onOpenChange: onMoreActionsDropdownMenuOpenChange,
   } = useDisclosure({ defaultOpen: false });
   const [popoverOpen, setPopoverOpen] = useState(false);

   return (
      <>
         {/*<Popover placement={`bottom`} isOpen={popoverOpen}>*/}
         {/*   <PopoverTrigger onClick={_ => setPopoverOpen(true)} className={`px-1`}>Hey</PopoverTrigger>*/}
         {/*   <PopoverContent>*/}
         {/*      <MoreMessageActionsDropdown />*/}
         {/*   </PopoverContent>*/}
         {/*</Popover>*/}
         <TooltipWithPopoverActionButton
            popoverContent={<MoreMessageActionsDropdown />}
            tooltipContent={"More actions"}
            icon={
               <VerticalDotsIcon className={`fill-foreground`} size={14} />
            }
            isOpen={isMoreActionsDropdownMenuOpen}
            popoverProps={{
               offset: 0,
               classNames: {
                  base: `p-0 rounded-md`,
               },
            }}
            tooltipProps={{
               placement: "top",
               radius: `md`,
               classNames: { base: `p-0 px-2 text-[.7rem]`, content: `px-2 h-4 text-[.6rem]` },
            }}
            chipProps={{
               color: `default`,
               variant: `light`,
               radius: "sm",
               classNames: {
                  base: "h-7 w-7 px-0",
               },
            }}
            onOpenChange={onMoreActionsDropdownMenuOpenChange}
         />
      </>
   );
};

const MoreMessageActionsDropdown = () => {
   return (
      <div
         onMouseEnter={console.log}
         className={`flex z-[100] cursor-pointer my-2 min-w-[150px] py-1 w-full text-xs flex-col items-start gap-1`}
      >
         <div
            className={`text-foreground py-1 group flex items-center justify-between px-2 w-full hover:bg-primary`}
         >
            <span>Edit message</span>
            <EditIcon className={`invisible group-hover:visible`} size={12} />
         </div>
         <div
            className={`text-danger flex items-center font-light group py-1 transition-all duration-[50ms] hover:text-foreground justify-between px-2 w-full hover:bg-danger`}
         >
            <span className={`group-hover:font-normal`}>Delete message</span>
            <ThrashIcon
               className={`invisible fill-foreground group-hover:visible`}
               size={14}
            />
         </div>
      </div>
   );
};

export default ChatMessageMoreActionsButton;
