"use client";
import React, { SVGProps } from "react";

export interface PinOffIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

export const PinOffIcon = ({ size, fill, ...rest }: PinOffIconProps) => {
   return (
      <svg xmlns="http://www.w3.org/2000/svg" width={size} height={size} viewBox="0 0 24 24" fill={fill}
           stroke="currentColor" strokeWidth="1" strokeLinecap="round" strokeLinejoin="round"
           className="lucide lucide-pin-off" {...rest}>
         <line x1="2" x2="22" y1="2" y2="22" />
         <line x1="12" x2="12" y1="17" y2="22" />
         <path d="M9 9v1.76a2 2 0 0 1-1.11 1.79l-1.78.9A2 2 0 0 0 5 15.24V17h12" />
         <path d="M15 9.34V6h1a2 2 0 0 0 0-4H7.89" />
      </svg>
   );
};
