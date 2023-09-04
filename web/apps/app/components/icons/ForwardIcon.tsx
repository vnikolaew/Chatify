"use client";
import React, { SVGProps } from "react";

export interface ForwardIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const ForwardIcon = ({ size, fill, ...rest }: ForwardIconProps) => {
   return (
      <div>
         <svg
            width={size}
            height={size}
            // fill={fill ?? "white"}
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            id="forward"
            {...rest}
         >
            <path d="M13 16c.6.3 1.4.2 1.9-.2l5.2-4.3c.5-.4.8-1.1.8-1.7s-.2-1.4-.7-1.8L15 3.7c-.5-.4-1.3-.5-1.9-.2-.6.3-1 .9-1 1.6v1.1h-1.4C6.4 6.3 3 9.6 3 13.7v4.4c0 .8.5 1.5 1.3 1.7.2 0 .3.1.5.1.6 0 1.2-.3 1.5-.9L8 16c.8-1.4 2.3-2.4 3.9-2.7v1c.1.8.5 1.4 1.1 1.7zm-6.6-1L5 17.3v-3.6c0-3 2.5-5.5 5.7-5.5H13c.6 0 1-.4 1-1V5.6l4.9 4c.1 0 .1.1.1.2s0 .1-.1.2L14 14v-1.7c0-.6-.4-1-1-1-2.7 0-5.3 1.4-6.6 3.7z"></path>
         </svg>
      </div>
   );
};

export default ForwardIcon;
