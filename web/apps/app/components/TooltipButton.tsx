"use client";
import { Chip, Tooltip } from "@nextui-org/react";
import React from "react";

export interface TooltipButtonProps {
   icon: React.ReactNode;
   content: string;
}

const TooltipButton = ({ icon, content }: TooltipButtonProps) => {
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
         size={"md"}
         content={content}
      >
         <Chip
            radius={"full"}
            color={"default"}
            classNames={{
               base: "h-10 w-10 px-0",
            }}
            // content={""}
            variant={"light"}
            className={`cursor-pointer transition-background duration-200 hover:bg-default-200`}
         >
            {icon}
         </Chip>
      </Tooltip>
   );
};

export default TooltipButton;
