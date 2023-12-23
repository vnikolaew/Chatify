"use client";
import React, { SVGProps } from "react";

export interface HamburgerMenuIconProps extends SVGProps<SVGSVGElement> {
   size: number | string;
}

export const HamburgerMenuIcon = ({
   size,
   fill,
   ...rest
}: HamburgerMenuIconProps) => {
   return (
      <svg
         xmlns="http://www.w3.org/2000/svg"
         enableBackground="new 0 0 24 24"
         viewBox="0 0 24 24"
         width={size}
         height={size}
         fill={fill ?? "white"}
         id="justify"
         {...rest}
      >
         <path
            d="M20,11H4c-0.6,0-1,0.4-1,1s0.4,1,1,1h16c0.6,0,1-0.4,1-1S20.6,11,20,11z M4,8h16c0.6,0,1-0.4,1-1s-0.4-1-1-1H4
	C3.4,6,3,6.4,3,7S3.4,8,4,8z M20,16H4c-0.6,0-1,0.4-1,1s0.4,1,1,1h16c0.6,0,1-0.4,1-1S20.6,16,20,16z"
         ></path>
      </svg>
   );
};
