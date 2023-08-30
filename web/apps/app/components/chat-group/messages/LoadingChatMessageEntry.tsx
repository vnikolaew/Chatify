"use client";
import { twMerge } from "tailwind-merge";
import { Skeleton } from "@nextui-org/react";
import React from "react";

export const LoadingChatMessageEntry = ({
   reversed = false,
}: {
   reversed?: boolean;
}) => {
   return (
      <div
         className={twMerge(
            `flex px-2 py-1 rounded-lg gap-3 items-center`,
            reversed && `flex-row-reverse`
         )}
      >
         <Skeleton className={`w-12 h-12 rounded-lg`} />
         <div
            className={twMerge(
               `flex my-auto flex-col h-full justify-between self-start items-start gap-2`,
               reversed && `items-end`
            )}
         >
            <div
               className={twMerge(
                  `items-center flex gap-2`,
                  reversed && `flex-row-reverse`
               )}
            >
               <Skeleton className={`h-3 w-24 rounded-full`} />
               <Skeleton className={`h-2 w-20 rounded-full`} />
            </div>
            <Skeleton className={`h-4 w-96 rounded-full`} />
            <Skeleton className={`h-2 w-36 rounded-full`} />
         </div>
      </div>
   );
};
