"use client";
import React from "react";
import {
   ChipProps,
   Popover,
   PopoverContent,
   PopoverProps,
   PopoverTrigger,
   TooltipProps,
} from "@nextui-org/react";
import { TooltipButton } from "@components/common";

export interface TooltipWithPopoverActionButtonProps {
   popoverContent: React.ReactNode;
   popoverProps?: Omit<PopoverProps, "children">;
   chipProps?: ChipProps;
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
   chipProps,
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
                  chipProps={chipProps}
               />
            </div>
         </PopoverTrigger>
         <PopoverContent>{popoverContent}</PopoverContent>
      </Popover>
   );
};

export default TooltipWithPopoverActionButton;
