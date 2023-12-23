"use client";
import React, { SVGProps } from "react";

export interface ItalicIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const ItalicIcon = ({ size, fill, ...rest }: ItalicIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         // fill={fill ?? "white"}
         xmlns="http://www.w3.org/2000/svg"
         data-name="Layer 1"
         {...rest}
         viewBox="0 0 24 24"
         id="italic"
      >
         <path d="M17,6H11a1,1,0,0,0,0,2h1.52l-3.2,8H7a1,1,0,0,0,0,2h6a1,1,0,0,0,0-2H11.48l3.2-8H17a1,1,0,0,0,0-2Z"></path>
      </svg>
   );
};

export default ItalicIcon;
