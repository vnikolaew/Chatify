"use client";
import React, { SVGProps } from "react";

export interface PlusIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const PlusIcon = ({ size, fill, ...rest }: PlusIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         // fill={fill ?? "white"}
         xmlns="http://www.w3.org/2000/svg"
         viewBox="0 0 24 24"
         id="plus"
         {...rest}
      >
         <path d="M19,11H13V5a1,1,0,0,0-2,0v6H5a1,1,0,0,0,0,2h6v6a1,1,0,0,0,2,0V13h6a1,1,0,0,0,0-2Z"></path>
      </svg>
   );
};

export default PlusIcon;
