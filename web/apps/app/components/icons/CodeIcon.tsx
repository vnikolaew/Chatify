"use client";
import React, { SVGProps } from "react";

export interface CodeIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const CodeIcon = ({ size, fill, className, ...rest }: CodeIconProps) => {
   return (
      <div>
         <svg
            width={size}
            height={size}
            // fill={fill ?? "white"}
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 256 256"
            id="code"
            {...rest}
         >
            <rect width="256" height="256" fill="none"></rect>
            <polyline
               fill="none"
               stroke="#000"
               className={className}
               strokeLinecap="round"
               strokeLinejoin="round"
               strokeWidth="24"
               points="64 88 16 128 64 168"
            ></polyline>
            <polyline
               fill="none"
               stroke="#000"
               strokeLinecap="round"
               className={className}
               strokeLinejoin="round"
               strokeWidth="24"
               points="192 88 240 128 192 168"
            ></polyline>
            <line
               x1="160"
               x2="96"
               y1="40"
               y2="216"
               className={className}
               fill="none"
               stroke="#000"
               strokeLinecap="round"
               strokeLinejoin="round"
               strokeWidth="24"
            ></line>
         </svg>
      </div>
   );
};

export default CodeIcon;
