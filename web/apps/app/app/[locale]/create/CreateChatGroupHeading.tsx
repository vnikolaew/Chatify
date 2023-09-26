"use client";
import React from "react";
import { Divider } from "@nextui-org/react";
import { twMerge } from "tailwind-merge";

export interface CreateChatGroupHeadingProps
   extends React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLDivElement>,
      HTMLDivElement
   > {}

const CreateChatGroupHeading = ({
   className,
   ...rest
}: CreateChatGroupHeadingProps) => {
   return (
      <div className={twMerge(`w-full`, className)} {...rest}>
         <h2 className={`text-default-700 text-2xl`}>Create a chat group</h2>
         <Divider
            orientation={"horizontal"}
            className={`h-[1.5px] mt-2 rounded-full shadow-md w-full bg-default-300`}
         />
      </div>
   );
};

export default CreateChatGroupHeading;
