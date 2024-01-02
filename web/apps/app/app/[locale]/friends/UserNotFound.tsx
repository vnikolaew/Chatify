"use client";
import { AlertCircle } from "lucide-react";

export interface UserNotFoundProps {

}

// @ts-ignore
export const UserNotFound = ({}: UserNotFoundProps) => {
   return (
      <div
         className={`w-full gap-2 flex items-center justify-center text-center mt-8`}
      >
         <AlertCircle
            className={`stroke-default-400`}
            size={28}
         />
         <span className={`text-default-400`}>
            No user was found with the given handle.
         </span>
      </div>
   );
};
