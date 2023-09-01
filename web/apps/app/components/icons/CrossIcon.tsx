"use client";
import React, { SVGProps } from "react";

export interface CrossIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const CrossIcon = ({ size, fill, className, ...rest }: CrossIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         xmlns="http://www.w3.org/2000/svg"
         viewBox="0 0 256 256"
         id="x"
         {...rest}
      >
         <rect className={className} width="256" height="256"></rect>
         <line
            x1="200"
            x2="56"
            y1="56"
            y2="200"
            strokeLinecap="round"
            className={className}
            strokeLinejoin="round"
            strokeWidth="24"
         ></line>
         <line
            x1="200"
            x2="56"
            y1="200"
            y2="56"
            className={className}
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="24"
         ></line>
      </svg>
   );
};

export default CrossIcon;
