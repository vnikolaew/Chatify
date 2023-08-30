"use client";
import React, { SVGProps } from "react";

export interface DownArrowProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const DownArrow = ({ size, fill, ...rest }: DownArrowProps) => {
   return (
      <svg
         width={size}
         height={size}
         xmlns="http://www.w3.org/2000/svg"
         id="arrow"
         x="0"
         y="0"
         version="1.1"
         viewBox="0 0 29 29"
         {...rest}
      >
         <path
            // fill={"red"}
            stroke={`white`}
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeMiterlimit="10"
            strokeWidth="2"
            d="m20.5 11.5-6 6-6-6"
         ></path>
      </svg>
   );
};

export default DownArrow;
