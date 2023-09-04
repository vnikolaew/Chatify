"use client";
import React, { SVGProps } from "react";

export interface VerticalDotsIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const VerticalDotsIcon = ({ size, fill, ...rest }: VerticalDotsIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         // fill={fill ?? "white"}
         xmlns="http://www.w3.org/2000/svg"
         viewBox="0 0 256 256"
         id="dots-three-vertical"
         {...rest}
      >
         <rect width="256" height="256" fill="none"></rect>
         <circle cx="128" cy="64" r="16"></circle>
         <circle cx="128" cy="128" r="16"></circle>
         <circle cx="128" cy="192" r="16"></circle>
      </svg>
   );
};

export default VerticalDotsIcon;
