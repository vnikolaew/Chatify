"use client";
import React from "react";
import {
   Popover,
   PopoverContent,
   PopoverProps,
   PopoverTrigger,
   TooltipProps,
   useDisclosure,
} from "@nextui-org/react";
import TooltipButton from "@components/TooltipButton";

export interface TooltipWithPopoverActionButtonProps {
   popoverContent: React.ReactNode;
   popoverProps?: PopoverProps;
   tooltipProps?: TooltipProps;
   tooltipContent: React.ReactNode;
   icon: React.ReactNode;
   isOpen: boolean;
   onOpenChange: () => void;
}

const TooltipWithPopoverActionButton = ({
   popoverContent,
   tooltipContent,
   icon,
   popoverProps,
   tooltipProps,
   isOpen,
   onOpenChange,
}: TooltipWithPopoverActionButtonProps) => {
   return (
      <Popover
         placement={"bottom"}
         onOpenChange={onOpenChange}
         isOpen={isOpen}
         {...popoverProps}
      >
         <PopoverTrigger>
            <div>
               <TooltipButton
                  onClick={() => onOpenChange()}
                  content={tooltipContent}
                  icon={icon}
                  {...tooltipProps}
               />
            </div>
         </PopoverTrigger>
         <PopoverContent>{popoverContent}</PopoverContent>
      </Popover>
   );
};

export default TooltipWithPopoverActionButton;
