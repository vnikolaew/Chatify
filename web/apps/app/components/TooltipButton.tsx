"use client";
import {
   Chip,
   ChipProps,
   Tooltip,
   TooltipProps,
   useDisclosure,
} from "@nextui-org/react";
import React from "react";

export interface TooltipButtonProps extends TooltipProps {
   icon: React.ReactNode;
   content: React.ReactNode;
   onClick?: (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => void;
   chipProps?: ChipProps;
}

const TooltipButton = ({
   icon,
   content,
   onClick,
   chipProps,
   ...rest
}: TooltipButtonProps) => {
   const { isOpen, onOpenChange, onClose, onOpen } = useDisclosure({
      defaultOpen: false,
   });
   return (
      <Tooltip
         shadow={"md"}
         closeDelay={100}
         classNames={{
            base: "text-xs px-4 py-2",
         }}
         showArrow
         radius={"sm"}
         color={"default"}
         placement={"bottom"}
         offset={2}
         isOpen={isOpen}
         size={"md"}
         content={content}
         {...rest}
      >
         <Chip
            radius={"full"}
            onClick={(_) => {
               onClose();
               onClick?.(_);
            }}
            color={"default"}
            onMouseEnter={(_) => onOpen()}
            onMouseLeave={(_) => onClose()}
            classNames={{
               base: "h-10 w-10 px-0",
            }}
            // content={""}
            variant={"light"}
            className={`cursor-pointer transition-background duration-200 hover:bg-default-200`}
            {...chipProps}
         >
            {icon}
         </Chip>
      </Tooltip>
   );
};

export default TooltipButton;
