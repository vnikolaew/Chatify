"use client";
import React, { SVGProps } from "react";

export interface PinIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const PinIcon = ({ size, fill, ...rest }: PinIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         fill={fill}
         xmlns="http://www.w3.org/2000/svg"
         viewBox="0 0 32 32"
         id="pin"
         {...rest}
      >
         <path d="M30 13.66a2.12 2.12 0 0 1-2 1.3h-2.47L19 21.45V24a2.12 2.12 0 0 1-2.12 2.12 2.09 2.09 0 0 1-1.5-.62l-3.75-3.75-7 7a1 1 0 0 1-1.42 0 1 1 0 0 1 0-1.42l7-7-3.7-3.75A2.12 2.12 0 0 1 8 13h2.54L17 6.47V3.94a2.11 2.11 0 0 1 3.61-1.5l8.91 8.91a2.11 2.11 0 0 1 .48 2.31Z"></path>
      </svg>
   );
};

export default PinIcon;
