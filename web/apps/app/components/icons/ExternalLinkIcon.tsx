"use client";
import React, { SVGProps } from "react";

export interface ExternalLinkIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const ExternalLinkIcon = ({
   size,
   fill,
   className,
   ...rest
}: ExternalLinkIconProps) => {
   return (
      <div>
         <svg
            width={size}
            height={size}
            // fill={fill ?? "white"}
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 256 256"
            id="link"
            {...rest}
         >
            <rect width="256" height="256" fill="none"></rect>
            <path
               fill="none"
               stroke="white"
               strokeLinecap="round"
               strokeLinejoin="round"
               className={className}
               strokeWidth="24"
               d="M130.49413,63.28047l11.648-11.648a44,44,0,1,1,62.22539,62.22539l-28.28427,28.28428a44,44,0,0,1-62.2254,0"
            ></path>
            <path
               fill="none"
               stroke="white"
               strokeLinecap="round"
               strokeLinejoin="round"
               strokeWidth="24"
               className={className}
               d="M125.50407,192.72133l-11.64621,11.6462a44,44,0,1,1-62.22539-62.22539l28.28427-28.28428a44,44,0,0,1,62.2254,0"
            ></path>
         </svg>
      </div>
   );
};

export default ExternalLinkIcon;
